using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSPHSolver : MonoBehaviour
{
	[System.Serializable]
	public struct SPHParticleSetting
	{
		public float minRadius;
		public float maxRadius;
		public float smoothingRadius;
		public float smoothingRadiusSq;

		public float mass;
		
		public float gasConst;
		public float restDensity;
		public float viscosity;
		public float gravityMult;
	}
	
	[SerializeField] public SPHParticleSetting settings;
	protected bool started = false;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public abstract void InitSPH(List<GameObject> gameObjects);
	public virtual void StartSimulation() {
		started = true;
	}
}
