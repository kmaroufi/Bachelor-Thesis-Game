using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Container : MonoBehaviour
{
	[SerializeField] public LiquidColor[] liquidColor;
	[SerializeField] public Transform deploy;
	[SerializeField] private int amount = 256;
    [SerializeField] private int rowAmount = 16;
    [SerializeField] private float particlesDistance = 0.3f;
	
	public List<GameObject> gameObjects;
    // Start is called before the first frame update
    void Awake()
    {
		
    }
	
	public List<GameObject> SpawnParticles(TestManager manager) {
		gameObjects = new List<GameObject>(amount);
		for (int i = 0; i < amount; i++)
        {
            //float jitter = (Random.value * 2f - 1f) * parameters[parameterID].particleRadius * 0.1f;
            float x = ((i % rowAmount) - rowAmount / 2) * particlesDistance + Random.Range(-0.05f, 0.05f);
            float z = 0;
            float y = ((i / rowAmount) - (amount / rowAmount / 2)) * particlesDistance + Random.Range(-0.05f, 0.05f);

            GameObject go = Instantiate(manager.spherePrefab);
            go.transform.localScale = Vector3.one * Random.Range(manager.sphSolver.settings.minRadius, manager.sphSolver.settings.maxRadius);
            go.transform.position = deploy.position + new Vector3(x, y, z);
			
			//color = (LiquidColor) Random.Range(0, 4);
			go.layer = manager.GetLiquidLayer(liquidColor[i % liquidColor.Length]);
			go.GetComponent<MeshRenderer> ().sharedMaterial = manager.liquidColors[(int)liquidColor[i % liquidColor.Length]];

			gameObjects.Add(go);
        }
		return gameObjects;
	}
	
	public void RepositionParticles(TestManager manager) {
		for (int i = 0; i < amount; i++)
        {
            //float jitter = (Random.value * 2f - 1f) * parameters[parameterID].particleRadius * 0.1f;
            float x = ((i % rowAmount) - rowAmount / 2) * particlesDistance + Random.Range(-0.05f, 0.05f);
            float z = 0;
            float y = ((i / rowAmount) - (amount / rowAmount / 2)) * particlesDistance + Random.Range(-0.05f, 0.05f);

            gameObjects[i].transform.position = deploy.position + new Vector3(x, y, z);
        }
	}

    // Update is called once per frame
    void Update()
    {
		
    }
}
