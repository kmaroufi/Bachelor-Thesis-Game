using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[SaveNoMembers]
public class SourceTank : MonoBehaviour
{
	[SerializeField] public LiquidColor liquidColor;
	[SerializeField] public Transform deploy;
	[SerializeField][SaveMember] public int initialCount = 20;
	[SerializeField][SaveMember] public GameObject internalTrigger;
	[SerializeField][SaveMember] public GameObject externalTrigger;
	
	[SerializeField] public GameObject nozzle;
	[SerializeField] public List<GameObject> sides;
	[SerializeField] public TMP_Text text;
	
	private GameManager gm;
	public List<Sphere> spheres;
	[SaveMember] private int colorToInt;
    // Start is called before the first frame update
    void Awake()
    {
		if (gm == null) {
			OnValidate();
		}
		gm = GameManager.Instance;
		
		deploySpheres();
		
		text.gameObject.SetActive(gm.debug);
    }
	
	public void deploySpheres() {
		spheres = new List<Sphere>();
		for (int i = 0; i < initialCount; i++) {
			Sphere sphere = Instantiate(gm.spherePrefab).GetComponent<Sphere>();
			sphere.targetSize = -1;
			float sphereSize = Random.Range(0.15f, 0.25f);
			sphere.transform.position = deploy.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
			sphere.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
			//sphere.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
			
			sphere.SetColor(liquidColor);
			//sphere.SetColor((LiquidColor) Random.Range(0, 4));

			spheres.Add(sphere);
		}
	}

    // Update is called once per frame
    void Update()
    {
		
    }
	
	void OnValidate() {
		if (gm == null) gm = GameManager.Instance;
		
		if (gm == null) return;
		
		foreach (GameObject obj in sides) {
			obj.GetComponent<MeshRenderer>().sharedMaterial = gm.defaultMaterial;
		}
		if (liquidColor != null) {
			nozzle.GetComponent<MeshRenderer>().sharedMaterial = gm.wallColors[(int) liquidColor];
		}
	}
	
	public bool IsFull() {
		return spheres.Count == initialCount;
	}
	
	public void OnSave() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were found in the scene when saving the game. Key: id, value: ObjectIdentifier
		colorToInt = (int) liquidColor;
	}

	public void OnLoad() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were reconstructed when loading the game. Key: id, value: ObjectIdentifier
		liquidColor = Util.GetEnumValue<LiquidColor>(colorToInt); 
		
		OnValidate();
		
		gm.OnObjectLoaded(this);
	}
}
