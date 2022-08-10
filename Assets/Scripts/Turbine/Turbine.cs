using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kmaroufi;

[SaveNoMembers]
public class Turbine : MonoBehaviour
{
	[SerializeField] private GameObject blades;
	[SerializeField][SaveMember] private float acceleration;
	[SerializeField][SaveMember] private float maxOmega;
	[SerializeField][SaveMember] public Action onEnergyGeneratedAction;
	
	private GameManager gm;
	public bool isOn;
	private float omega;
	private int numOfEnteredSpheres;
    // Start is called before the first frame update
    void Start()
    {
		gm = GameManager.Instance;
		foreach (Transform t in blades.transform) {
			if (t.gameObject.name != "Tip") {
				t.GetComponent<MeshRenderer>().sharedMaterial = gm.wallColors[1];
			} else {
				t.GetComponent<MeshRenderer>().sharedMaterial = gm.defaultMaterial;
			}
		}
		
		omega = 0;
		isOn = false;
		numOfEnteredSpheres = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOn) {
			blades.transform.Rotate(new Vector3(omega * Time.deltaTime, 0, 0));
			omega -= maxOmega * Time.deltaTime;
			if ((omega < 0 && maxOmega > 0) || (omega > 0 && maxOmega < 0)) {
				StopRotating();
			}
			if (numOfEnteredSpheres > 15 && onEnergyGeneratedAction != null) {
				onEnergyGeneratedAction.Do(gameObject);
				onEnergyGeneratedAction = null;
			}
			//omega = Mathf.Min(maxOmega, omega + acceleration * Time.deltaTime);
		} else {
			//blades.transform.Rotate(new Vector3(omega * Time.deltaTime, 0, 0));
			//omega = Mathf.Max(0, omega - acceleration * Time.deltaTime);
		}
    }
	
	private void OnTriggerEnter(Collider collider) {
		if (!isOn) BeginRotating();
		
		collider.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, -2, 0);
		if (maxOmega > 0) {
			omega = Mathf.Min(maxOmega + maxOmega / 20, maxOmega);
		}
		else {
			omega = Mathf.Max(maxOmega - maxOmega / 10, maxOmega);
		}
		
		numOfEnteredSpheres++;
	}
	
	private void OnTriggerExit(Collider collider) {

	}
	
	public void BeginRotating() {
		isOn = true;
		omega = maxOmega;
	}
	
	public void StopRotating() {
		isOn = false;
	}
	
	public void OnSave() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were found in the scene when saving the game. Key: id, value: ObjectIdentifier
	}

	public void OnLoad() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were reconstructed when loading the game. Key: id, value: ObjectIdentifier
		GameManager.Instance.OnObjectLoaded(this);
	}
}
