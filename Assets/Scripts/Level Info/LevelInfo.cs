using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SaveNoMembers]
public class LevelInfo : MonoBehaviour
{
	[SaveMember] public List<FillingOrder> fillingOrders;

	public float cameraFieldOfView;
    // Start is called before the first frame update
    void Start()
    {
        GameManager gm = GameManager.Instance;
		fillingOrders = gm.fillingOrders;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void OnSave() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were found in the scene when saving the game. Key: id, value: ObjectIdentifier
		fillingOrders = GameManager.Instance.fillingOrders;
	}

	public void OnLoad() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were reconstructed when loading the game. Key: id, value: ObjectIdentifier
		GameManager.Instance.OnObjectLoaded(this);
	}
}
