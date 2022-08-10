using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

public class SPHSolver3 : AbstractSPHSolver
{
    // Consts
	private Vector2 GRAVITY = new Vector2(0.0f, -9.81f);

    // Data
    private GameObject[] particles;
	
	//private Dictionary<Pair<int, int>
	
	//private int count;
	
	private JobHandle prevJobHandle;
	private	NativeArray<Vector2> particlesPosition;
	private	NativeArray<Vector2> particlesVelocity;
	private	NativeArray<float> particlesDensity;
	private	NativeArray<float> particlesPressure;
	private NativeArray<Vector2> particlesForces;
	private	NativeMultiHashMap<int, int> hashMap;
	private	NativeArray<int2> cellOffsetTableNative;
	
	private float POLY6;
	private float SPIKY_GRAD;
	private float VISC_LAP;


    private void Start()
    {	
		
    }
	
	public override void InitSPH(List<GameObject> gameObjects)
    {
		particles = gameObjects.ToArray();
    }
	
	public override void StartSimulation() {
		base.StartSimulation();
		StartCoroutine(SPHUpdate());
	}

    private void FixedUpdate()
    {
		if (!started) return;
		//Debug.Log("FixedUpdate");
		//if (prevJobHandle == null) {
		//	return;
		//}
		
		prevJobHandle.Complete();
		
		ApplyForces();
    }

    private void ApplyForces()
    {
        for (int i = 0; i < particles.Length; i++)
        {
			particles[i].GetComponent<Rigidbody>().AddForce(particlesForces[i] / particlesDensity[i], ForceMode.Acceleration);
        }
    }
	
	[BurstCompile]
    private struct HashPositions : IJobParallelFor
    {
        #pragma warning disable 0649
        [ReadOnly] public float cellRadius;

        [ReadOnly] public NativeArray<Vector2> positions;
        public NativeMultiHashMap<int, int>.ParallelWriter hashMap;
        #pragma warning restore 0649

        public void Execute(int index)
        {
            int hash = GridHash.Hash(positions[index], cellRadius);
            hashMap.Add(hash, index);
        }
    }
	
	[BurstCompile]
    private struct ComputeDensityPressure : IJobParallelFor
    {
        #pragma warning disable 0649
        [ReadOnly] public NativeMultiHashMap<int, int> hashMap;
        [ReadOnly] public NativeArray<int2> cellOffsetTable;
        [ReadOnly] public NativeArray<Vector2> particlesPosition;
        [ReadOnly] public SPHParticleSetting settings;

        public NativeArray<float> densities;
        public NativeArray<float> pressures;
        #pragma warning restore 0649

        private const float PI = 3.14159274F;
        //private const float B = 10f;
		
		public float POLY6;

        public void Execute(int index)
        {
            // Cache
            int particleCount = particlesPosition.Length;
            Vector2 position = particlesPosition[index];
            float density = 0.0f;
            int hash, j;
            int2 gridOffset;
            int2 gridPosition = GridHash.Quantize(position, settings.smoothingRadius);
            bool found;
			
			for (int offsetIndex = 0; offsetIndex < cellOffsetTable.Length; offsetIndex++) {
				gridOffset = cellOffsetTable[offsetIndex];
                hash = GridHash.Hash(gridPosition + gridOffset);
                NativeMultiHashMapIterator<int> iterator;
                found = hashMap.TryGetFirstValue(hash, out j, out iterator);
				//count++;
				//Debug.Log(hashMap.Count());
				
				while (found) {
					//count++;
					//Debug.Log("Found");
					Vector2 rij = particlesPosition[j] - position;
					float r2 = rij.sqrMagnitude;

					if (r2 < settings.smoothingRadiusSq)
					{
						//count++;
						density += settings.mass * POLY6 * math.pow(settings.smoothingRadiusSq - r2, 3.0f);
                    }
					
					found = hashMap.TryGetNextValue(out j, ref iterator);
					
					/*if (found) {
						count++;
					}*/
				}
			}

            // Apply density and compute/apply pressure
            densities[index] = density;
            pressures[index] = settings.gasConst * (density - settings.restDensity);
			//pressures[index] = B * (math.pow(density/settings.restDensity, 7) - 1);
        }
    }
	
	[BurstCompile]
    private struct ComputeForces : IJobParallelFor
    {
        #pragma warning disable 0649
        [ReadOnly] public NativeMultiHashMap<int, int> hashMap;
        [ReadOnly] public NativeArray<int2> cellOffsetTable;
        [ReadOnly] public NativeArray<Vector2> particlesPosition;
        [ReadOnly] public NativeArray<Vector2> particlesVelocity;
        [ReadOnly] public NativeArray<float> particlesPressure;
        [ReadOnly] public NativeArray<float> particlesDensity;
        [ReadOnly] public SPHParticleSetting settings;
		[ReadOnly] public Vector2 GRAVITY;

        public NativeArray<Vector2> particlesForces;
        #pragma warning restore 0649

        private const float PI = 3.14159274F;
		
		public float SPIKY_GRAD;
		public float VISC_LAP;

        public void Execute(int index)
        {
            // Cache
            int particleCount = particlesPosition.Length;
            Vector2 position = particlesPosition[index];
            Vector2 velocity = particlesVelocity[index];
            float pressure = particlesPressure[index];
            float density = particlesDensity[index];
            Vector2 forcePressure = Vector2.zero;
            Vector2 forceViscosity = Vector2.zero;
            int hash, j;
            int2 gridOffset;
            int2 gridPosition = GridHash.Quantize(position, settings.smoothingRadius);
            bool found;
			
			for (int offsetIndex = 0; offsetIndex < cellOffsetTable.Length; offsetIndex++) {
				gridOffset = cellOffsetTable[offsetIndex];
                hash = GridHash.Hash(gridPosition + gridOffset);
                NativeMultiHashMapIterator<int> iterator;
				found = hashMap.TryGetFirstValue(hash, out j, out iterator);
				//count++;
				
				while (found)
                {
					//count++;
                    // Neighbor found, get density
                    if (index == j)
                    {
                        found = hashMap.TryGetNextValue(out j, ref iterator);
                        continue;
                    }
					
					Vector2 rij = particlesPosition[j] - position;
					float r2 = rij.sqrMagnitude;
					float r = Mathf.Sqrt(r2);
					
					if (r < settings.smoothingRadius)
                    {
                        forcePressure += rij.normalized * settings.mass * (pressure + particlesPressure[j]) / (2.0f * particlesDensity[j]) * SPIKY_GRAD * math.pow(settings.smoothingRadius - r, 2.0f);

                        forceViscosity += settings.viscosity * settings.mass * (particlesVelocity[j] - velocity) / particlesDensity[j] * VISC_LAP * (settings.smoothingRadius - r);
                    }
					
					// Next neighbor
                    found = hashMap.TryGetNextValue(out j, ref iterator);
					/*if (found) {
						count++;
					}*/
				}
			}
            // Gravity
            Vector2 forceGravity = GRAVITY * density * settings.gravityMult;

            // Apply
            particlesForces[index] = forcePressure + forceViscosity + forceGravity;
        }
    }
	
	IEnumerator SPHUpdate() {
		POLY6 = 4.0f / (Mathf.PI * math.pow(settings.smoothingRadius, 8.0f));
		SPIKY_GRAD = -30.0f / (Mathf.PI * math.pow(settings.smoothingRadius, 5.0f));
		VISC_LAP = 40.0f / (Mathf.PI * math.pow(settings.smoothingRadius, 5.0f));
		while(true) {
			//Debug.Log("SPHUpdate");
			if (particlesPosition.IsCreated) {
				particlesPosition.Dispose();
				particlesVelocity.Dispose();
				particlesDensity.Dispose();
				particlesPressure.Dispose();
				particlesForces.Dispose();
				hashMap.Dispose();
				cellOffsetTableNative.Dispose();
			}
			int particleCount = particles.Length;
			particlesPosition = new NativeArray<Vector2>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			particlesVelocity = new NativeArray<Vector2>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            particlesDensity = new NativeArray<float>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            particlesPressure = new NativeArray<float>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			particlesForces = new NativeArray<Vector2>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			hashMap = new NativeMultiHashMap<int, int>(particleCount, Allocator.TempJob);
			cellOffsetTableNative = new NativeArray<int2>(GridHash.cell2DOffsets, Allocator.TempJob);
			
			int batchSize = 64;
			
			for (int i = 0; i < particles.Length; i++)
			{
				particlesPosition[i] = particles[i].transform.position;
				particlesVelocity[i] = particles[i].GetComponent<Rigidbody>().velocity;
			}
			
			HashPositions hashPositionsJob = new HashPositions
            {
                positions = particlesPosition,
                hashMap = hashMap.AsParallelWriter(),
                cellRadius = settings.smoothingRadius
            };
            JobHandle hashPositionsJobHandle = hashPositionsJob.Schedule(particleCount, batchSize);
			
			// Compute density pressure
            ComputeDensityPressure computeDensityPressureJob = new ComputeDensityPressure
            {
                particlesPosition = particlesPosition,
                densities = particlesDensity,
                pressures = particlesPressure,
                hashMap = hashMap,
                cellOffsetTable = cellOffsetTableNative,
                settings = settings,
				POLY6 = POLY6
            };
            JobHandle computeDensityPressureJobHandle = computeDensityPressureJob.Schedule(particleCount, batchSize, hashPositionsJobHandle);
			
			// Compute forces
            ComputeForces computeForcesJob = new ComputeForces
            {
                particlesPosition = particlesPosition,
                particlesVelocity = particlesVelocity,
                particlesForces = particlesForces,
                particlesPressure = particlesPressure,
                particlesDensity = particlesDensity,
                cellOffsetTable = cellOffsetTableNative,
                hashMap = hashMap,
                settings = settings,
				GRAVITY = GRAVITY,
				SPIKY_GRAD = SPIKY_GRAD,
				VISC_LAP = VISC_LAP
            };
            JobHandle computeForcesJobHandle = computeForcesJob.Schedule(particleCount, batchSize, computeDensityPressureJobHandle);
			
			prevJobHandle = computeForcesJobHandle;
			
			yield return new WaitForFixedUpdate();
		}
	}
	
	protected void OnDestroy()
    {
		prevJobHandle.Complete();
		
		particlesPosition.Dispose();
		particlesVelocity.Dispose();
		particlesDensity.Dispose();
		particlesPressure.Dispose();
		particlesForces.Dispose();
		hashMap.Dispose();
		cellOffsetTableNative.Dispose();
    }
}
