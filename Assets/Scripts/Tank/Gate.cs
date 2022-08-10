using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GateState {Opened, Closed, Closing}

[SaveNoMembers]
public class Gate : MonoBehaviour
{
	[SerializeField][SaveMember] public float period = 0.5f;
	[SerializeField] public GameObject leftDoor;
	[SerializeField] public GameObject rightDoor;
	[SerializeField] public GameObject leftBarrier;
	[SerializeField] public GameObject rightBarrier;
	
	private GateState m_state;
	public GateState state { get { return m_state; } set {m_state = value;} }
	
	private float doorOmega;
	private float barrierSpeed;
	private float remainingTime;
	
	private Vector3 leftBarrierFirstPosition;
	private Vector3 rightBarrierFirstPosition;
	
	void Awake() {
		/*
		leftDoor = transform.Find("Left Door").gameObject;
		rightDoor = transform.Find("Right Door").gameObject;
		leftBarrier = transform.Find("Left Barrier").gameObject;
		rightBarrier = transform.Find("Right Barrier").gameObject;
		
		Debug.Log("Gate Awake:, " + leftDoor + ", " + rightDoor + ", " + leftBarrier + ", " + rightBarrier);
		*/
	}
	
	
    // Start is called before the first frame update
    void Start()
    {
		state = GateState.Opened;
		
        doorOmega = 90 / period;
		remainingTime = period;
		barrierSpeed = 0.45f / period;
		
		leftBarrierFirstPosition = leftBarrier.transform.position;
		rightBarrierFirstPosition = rightBarrier.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == GateState.Closing) {
			leftDoor.transform.Rotate(0, 0, doorOmega * Time.deltaTime, Space.World);
			rightDoor.transform.Rotate(0, 0, -doorOmega * Time.deltaTime, Space.World);
			
			leftBarrier.transform.Translate(new Vector3(barrierSpeed * Time.deltaTime, 0, 0));
			rightBarrier.transform.Translate(new Vector3(-barrierSpeed * Time.deltaTime, 0, 0));
			
			remainingTime -= Time.deltaTime;
			
			if (remainingTime < 0) {
				leftDoor.transform.eulerAngles = new Vector3(0, 0, 90);
				rightDoor.transform.eulerAngles = new Vector3(0, 0, -90);
				
				leftBarrier.transform.position = leftBarrierFirstPosition + new Vector3(0.45f, 0, 0);
				rightBarrier.transform.position = rightBarrierFirstPosition - new Vector3(0.45f, 0, 0);
				
				state = GateState.Closed;
			}
		}
    }
	
	public void Close() {
		state = GateState.Closing;
		
		leftDoor.GetComponentInChildren<BoxCollider>().size = new Vector3(1, 2, 1);
		leftDoor.GetComponentInChildren<BoxCollider>().center = new Vector3(0, -0.5f, 0);
		
		rightDoor.GetComponentInChildren<BoxCollider>().size = new Vector3(1, 2, 1);
		rightDoor.GetComponentInChildren<BoxCollider>().center = new Vector3(0, 0.5f, 0);
	}
	
	public void OnSave() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were found in the scene when saving the game. Key: id, value: ObjectIdentifier
	}

	public void OnLoad() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were reconstructed when loading the game. Key: id, value: ObjectIdentifier
		
	}
}
