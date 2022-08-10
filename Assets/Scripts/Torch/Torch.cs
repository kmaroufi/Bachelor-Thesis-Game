using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kmaroufi;

[SaveNoMembers]
public class Torch : MonoBehaviour
{
	[SerializeField] private ParticleSystem ps;
	[SaveMember] public Action onStopAction;
	
	private GameManager gm;
    // Start is called before the first frame update
    void Start()
    {
		gm = GameManager.Instance;
		transform.Find("Stand").GetComponent<MeshRenderer>().sharedMaterial = gm.wallColors[2];
		transform.Find("Rod").GetComponent<MeshRenderer>().sharedMaterial = gm.defaultMaterial;
        ps.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!ps.isPlaying && onStopAction != null) {
			onStopAction.Do(gameObject);
		}
    }
	
	public void Stop() {
		ps.Stop(true);
	}
	
	public bool IsOff() {
		return !ps.isPlaying;
	}
	
	public void OnSave() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were found in the scene when saving the game. Key: id, value: ObjectIdentifier
	}

	public void OnLoad() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were reconstructed when loading the game. Key: id, value: ObjectIdentifier
		GameManager.Instance.OnObjectLoaded(this);
	}
}
