using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		GameManager gm = GameManager.Instance;
        transform.Find("Stand").GetComponent<MeshRenderer>().sharedMaterial = gm.defaultMaterial;
		transform.Find("Rod").GetComponent<MeshRenderer>().sharedMaterial = gm.blackMaterial;
		transform.Find("Tip").GetComponent<MeshRenderer>().sharedMaterial = gm.blackMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void OnSave() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were found in the scene when saving the game. Key: id, value: ObjectIdentifier
	}

	public void OnLoad() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were reconstructed when loading the game. Key: id, value: ObjectIdentifier
		GameManager.Instance.OnObjectLoaded(this);
	}
}
