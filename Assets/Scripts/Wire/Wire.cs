using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using Kmaroufi;

[SaveNoMembers]
public class Wire : MonoBehaviour
{
	public enum State {Idle, Transfering}
	
	[SerializeField] private ObiSolver solver;
	[SerializeField][SaveMember] private float movementPeriod = 0.05f;
	[SerializeField] private GameObject electroObj;
	[SerializeField][SaveMember] private Action onTransferingEndAction;
	[SerializeField][SaveMember] public Transform leftAttachment;
	[SerializeField][SaveMember] public Transform rightAttachment;
	[SerializeField][SaveMember] public bool leftToRight = false;

	private GameManager gm;
	private ObiActor actor;
	private ObiRope rope;
	private State state;
	private int currentParticleIndex;
	private float movementTime;
    // Start is called before the first frame update
    void Start()
    {
		gm = GameManager.Instance;
		state = State.Idle;
		solver = GameManager.Instance.obiSolver;
		transform.parent = solver.transform;
		actor = gameObject.GetComponent<ObiActor>();
		rope = GetComponent<ObiRope>();
		electroObj.SetActive(false);
		electroObj.GetComponent<MeshRenderer>().sharedMaterial = gm.wallColors[1];
		
		ObiParticleAttachment[] attachments = rope.GetComponents<ObiParticleAttachment>();

		foreach (ObiParticleAttachment attachment in attachments) {
			if (attachment.particleGroup.name == "Left") {
				attachment.target = leftAttachment;
			} else if (attachment.particleGroup.name == "Right") {
				attachment.target = rightAttachment;
			} 
		}
    }

    // Update is called once per frame
    void Update()
    {
		/*
		Vector3 pos = solver.positions[actor.solverIndices[5]];
		electroObj.transform.position = solver.transform.position + pos;
		Debug.Log("Count " + actor.activeParticleCount);
		*/
		if (state == State.Transfering) {
			Vector3 pos = Vector3.zero;
			if (leftToRight) {
				pos = solver.positions[actor.solverIndices[currentParticleIndex]];
			} else {
				pos = solver.positions[actor.solverIndices[actor.activeParticleCount - 1 - currentParticleIndex]];
			}
			electroObj.transform.position = Vector3.Lerp(electroObj.transform.position, solver.transform.position + pos, 0.2f);
			
			movementTime -= Time.deltaTime;
			while (movementTime < 0) {
				moveToNextParticle();
				if (state == State.Idle) break;
			}
		}
    }
	
	public void StartTransfering() {
		state = State.Transfering;
		currentParticleIndex = -1;
		
		electroObj.SetActive(true);
		Vector3 pos = solver.positions[actor.solverIndices[leftToRight ? 0 : actor.activeParticleCount - 1]];
		electroObj.transform.position = solver.transform.position + pos;
		
		movementTime = 0f;
		moveToNextParticle();
	}
	
	public void moveToNextParticle() {
		/*
		if (currentShiningComponentIndex > -1) {
			components[currentShiningComponentIndex].DoNotShine();
		}
		*/
		currentParticleIndex++;
		if (actor.activeParticleCount == currentParticleIndex) {
			electroObj.SetActive(false);
			state = State.Idle;
			if (onTransferingEndAction != null) {
				onTransferingEndAction.Do(gameObject);
			}
		} else {
			movementTime += movementPeriod;
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
