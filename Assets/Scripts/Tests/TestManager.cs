using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestManager : SingletonComponent<TestManager>
{
	[SerializeField] public GameObject spherePrefab;
	[SerializeField] public AbstractSPHSolver sphSolver;
	[SerializeField] public FPSCalculator fpsCalculator;
	[SerializeField] public TMP_Text resultText;
	[SerializeField] public MetaBalls1 metaballsCamera;
	[SerializeField] public List<Material> liquidColors;
	[SerializeField] public Container[] containers;
	[SerializeField] public float simulationTime;
	[SerializeField] public int simulationRepeatNum;
	
	private List<GameObject> gameObjects;
	private int curretSimulationNum;
    private float currentSimulationElapsedTime;
    // Start is called before the first frame update
    void Start()
    {
		gameObjects = new List<GameObject>();
		for (int i = 0; i < containers.Length; i++)
        {
			if (containers[i].gameObject.active) {
				gameObjects.AddRange(containers[i].SpawnParticles(this));
			}
		}
		sphSolver.InitSPH(gameObjects);
		//metaballsCamera.Set();
		StartCoroutine(StartSimulation());
    }
	
	IEnumerator StartSimulation() {
		yield return new WaitForSeconds(5);
		
		sphSolver.StartSimulation();
		fpsCalculator.StartCounting();
	}
	
	public int GetLiquidLayer(LiquidColor? color) {
		if (color == null) return -1;
		return LayerMask.NameToLayer("Liquid" + (((int)color) + 1).ToString());
	}

    // Update is called once per frame
    void Update()
    {
		if (curretSimulationNum == simulationRepeatNum) return;
		
		currentSimulationElapsedTime += Time.deltaTime;
        if (currentSimulationElapsedTime > simulationTime) {
			curretSimulationNum++;
			if (curretSimulationNum == simulationRepeatNum) {
				resultText.SetText("finished");
				fpsCalculator.Stop();
			} else {
				Reset();
			}
		}
    }
	
	private void Reset() {
		for (int i = 0; i < containers.Length; i++)
        {
			if (containers[i].gameObject.active) {
				containers[i].RepositionParticles(this);
			}
		}
		currentSimulationElapsedTime = 0;
	}
}
