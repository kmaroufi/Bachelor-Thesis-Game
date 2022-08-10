using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kmaroufi;

[SaveNoMembers]
public class Fan : MonoBehaviour
{
	[SerializeField] private GameObject blades;
	[SerializeField][SaveMember] private float omega;
	[SerializeField] public GameObject sign;
	[SerializeField][SaveMember] public Action onTurnOnAction;
	[SerializeField][SaveMember] private float acceleration = 180;
	
	private GameManager gm;
	private bool isOn;
	private bool stop;
	
    // Start is called before the first frame update
    void Start()
    {
		gm = GameManager.Instance;
		transform.Find("Stand").GetComponent<MeshRenderer>().sharedMaterial = gm.wallColors[0];
		transform.Find("Rod").GetComponent<MeshRenderer>().sharedMaterial = gm.defaultMaterial;
		foreach (Transform t in blades.transform) {
			if (t.gameObject.name != "Tip") {
				t.GetComponent<MeshRenderer>().sharedMaterial = gm.wallColors[0];
			} else {
				t.GetComponent<MeshRenderer>().sharedMaterial = gm.defaultMaterial;
			}
		}
		isOn = false;
		stop = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOn) {
			blades.transform.Rotate(new Vector3(omega * Time.deltaTime, 0, 0));
			
			if (stop) {
				omega -= acceleration * Time.deltaTime;
				if (omega < 0) {
					isOn = false;
				}
			}
		}
    }
	
	public void TurnOn() {
		isOn = true;
		
		Invoke("OnTurnOnAction", 0.1f);
	}
	
	private void OnTurnOnAction() {
		//sign.SetActive(false);
		if (onTurnOnAction != null) {
			onTurnOnAction.Do(gameObject);
		}
		Invoke("Stop", 1f);
	}
	
	private void Stop() {
		stop = true;
	}
	
	public void OnSave() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were found in the scene when saving the game. Key: id, value: ObjectIdentifier
	}

	public void OnLoad() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were reconstructed when loading the game. Key: id, value: ObjectIdentifier
		GameManager.Instance.OnObjectLoaded(this);
	}
}
