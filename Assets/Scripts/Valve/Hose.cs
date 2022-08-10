using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[SaveNoMembers]
public class Hose : MonoBehaviour
{
	private ObiRopeCursor cursor;
	private ObiRope rope;
	public float lastLength;
	private float startingLength;
    // Start is called before the first frame update
    void Start()
    {
		cursor = GetComponentInChildren<ObiRopeCursor>();
		rope = GetComponent<ObiRope>();
		startingLength = rope.restLength;
    }

    // Update is called once per frame
    void Update()
    {
		/*
        if (Input.GetKey(KeyCode.X)){
			cursor.ChangeLength(rope.restLength - 5f * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.Z)){
			cursor.ChangeLength(rope.restLength + 5f * Time.deltaTime);
		}*/
		//return;
		
		if (rope.CalculateLength() > rope.restLength * 1.6f) {
			cursor.ChangeLength(rope.restLength + 2f * Time.deltaTime);
		} else if (rope.CalculateLength() < rope.restLength * 1.4f && rope.restLength > 2) {
			cursor.ChangeLength(rope.restLength - 2f * Time.deltaTime);
		} 
		lastLength = Mathf.Lerp(lastLength, rope.CalculateLength(), 0.5f);
    }
	
	public bool IsLengthIncreased() {
		return lastLength > rope.CalculateLength();
	}
	
	public void SetValve(Valve valve) {
		rope = GetComponent<ObiRope>();
		
		Vector3 offset = new Vector3(1.6735f, -3.24f, 0) - new Vector3(-0.75f, -0.07f, -1.6f);
		transform.position = valve.transform.position - offset;
		transform.parent = GameManager.Instance.obiSolver.transform;
		/*
		cursor.ChangeLength(startingLength);
		rope.ResetParticles();
		*/
		ObiParticleAttachment[] attachments = rope.GetComponents<ObiParticleAttachment>();
		foreach (ObiParticleAttachment attachment in attachments) {
			if (attachment.particleGroup.name == "Start" || attachment.particleGroup.name == "Start2") {
				attachment.target = valve.start.transform;
			} else if (attachment.particleGroup.name == "End" || attachment.particleGroup.name == "End2") {
				attachment.target = valve.nozzle.transform;
			} 
		}
	}
	
	public void OnSave() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were found in the scene when saving the game. Key: id, value: ObjectIdentifier
	}

	public void OnLoad() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were reconstructed when loading the game. Key: id, value: ObjectIdentifier
		GameManager.Instance.OnObjectLoaded(this);
	}
}
