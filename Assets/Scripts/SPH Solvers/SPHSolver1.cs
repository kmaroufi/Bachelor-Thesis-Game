using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class SPHSolver1 : AbstractSPHSolver
{
    private struct SPHParticle
    {
        public GameObject go;
		
        public Vector2 velocity;
        public Vector2 position;

        public float density;
        public float pressure;
		public Vector2 forcePhysic;

        public void Init(Vector2 _position, GameObject _go)
        {
            go = _go;
			
            velocity = Vector2.zero;
            position = _position;
			
            density = 0.0f;
            pressure = 0.0f;
            forcePhysic = Vector2.zero;
        }
    }

    // Consts
    private static Vector2 GRAVITY = new Vector2(0.0f, -9.81f);
	
	private float POLY6;
	private float SPIKY_GRAD;
	private float VISC_LAP;

    // Data
    private SPHParticle[] particles;


    private void Start()
    {
    }
	
	public override void InitSPH(List<GameObject> gameObjects)
    {
        particles = new SPHParticle[gameObjects.Count];
        
        for (int i = 0; i < gameObjects.Count; i++)
        {
            particles[i].Init(gameObjects[i].transform.position, gameObjects[i]);
        }
		
		POLY6 = 4.0f / (Mathf.PI * math.pow(settings.smoothingRadius, 8.0f));
		SPIKY_GRAD = -30.0f / (Mathf.PI * math.pow(settings.smoothingRadius, 5.0f));
		VISC_LAP = 40.0f / (Mathf.PI * math.pow(settings.smoothingRadius, 5.0f));
    }

    private void FixedUpdate()
    {
		if (!started) return;
		
		UpdateParticlesInfo();
		ComputeDensityPressure();
		ComputeForces();
		ApplyForces();
    }
	
	private void UpdateParticlesInfo()
    {
        for (int i = 0; i < particles.Length; i++)
        {
			particles[i].velocity = particles[i].go.GetComponent<Rigidbody>().velocity;
			particles[i].position = particles[i].go.transform.position;
        }
    }

    private void ComputeDensityPressure()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].density = 0.0f;

            for (int j = 0; j < particles.Length; j++)
            {
				Vector2 rij = particles[j].position - particles[i].position;
                float r2 = rij.sqrMagnitude;

                if (r2 < settings.smoothingRadiusSq)
                {
					particles[i].density += settings.mass * POLY6 * math.pow(settings.smoothingRadiusSq - r2, 3.0f);
				}
            }

            particles[i].pressure = settings.gasConst * (particles[i].density - settings.restDensity);
        }
    }



    private void ComputeForces()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            Vector2 forcePressure = Vector2.zero;
            Vector2 forceViscosity = Vector2.zero;

            // Physics
            for (int j = 0; j < particles.Length; j++)
            {
                if (i == j) continue;

				Vector2 rij = particles[j].position - particles[i].position;
                float r2 = rij.sqrMagnitude;
                float r = Mathf.Sqrt(r2);

                if (r < settings.smoothingRadius)
                {
                    forcePressure += rij.normalized * settings.mass * (particles[i].pressure + particles[j].pressure) 
										/ (2.0f * particles[j].density) * SPIKY_GRAD * math.pow(settings.smoothingRadius - r, 2.0f);

                    forceViscosity += settings.viscosity * settings.mass * (particles[j].velocity - particles[i].velocity) 
										/ particles[j].density * VISC_LAP * (settings.smoothingRadius - r);
                }
            }
            Vector2 forceGravity = GRAVITY * particles[i].density * settings.gravityMult;
			
            particles[i].forcePhysic = forcePressure + forceViscosity + forceGravity;
        }
    }
	
	private void ApplyForces()
    {
        for (int i = 0; i < particles.Length; i++)
        {
			particles[i].go.GetComponent<Rigidbody>()
				.AddForce((particles[i].forcePhysic) / particles[i].density, ForceMode.Acceleration);
        }
    }
}
