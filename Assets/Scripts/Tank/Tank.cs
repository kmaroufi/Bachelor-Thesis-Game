using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[SaveNoMembers]
[ExecuteInEditMode]
public class Tank : MonoBehaviour
{
	[HideInInspector] public int numOfSpheres;
	[SerializeField] public LiquidColor color;
	[SerializeField][SaveMember] public int fillingLimit;
	[SerializeField] public List<GameObject> sides;
	[SerializeField] public List<GameObject> barriers;
	[SerializeField] public Animator animator;
	[SerializeField] public TMP_Text text;
	[SerializeField] public GameObject filledMark;
	[SerializeField][SaveMember] public bool nearTorch = false;
	[SerializeField][SaveMember] public bool toBeFilledWithBobble = false;
	[SerializeField][SaveMember] public Bobble bobble;
	[SerializeField][SaveMember] public float bubblePoppedPeriodThreshold = 0.5f;
	
	private GameManager gm;
	public bool isFilled;
	private List<Sphere> unmatchedSpheres;
	public int numOfUnmatchedSpheres;
	public bool toFill = false;
	public bool unmatchThresholdFlag = false;
	private float timeSinceFilled;
	private float timeSinceLastEnteredSphere;
	private int numOfSmokedSpheres;
	private bool smokeThresholdFlag = false;
	private bool ranOutOfBobblesFlag = false;
	private bool fillingGroupCompletedFlag = false;
	private float timeSinceBobblePopped;
	private int numOfSpheresInBobble;
	[SaveMember] private int colorToInt;
    // Start is called before the first frame update
    void Start()
    {
		if (gm == null) {
			OnValidate();
		}
        gm = GameManager.Instance;
		isFilled = false;
		unmatchedSpheres = new List<Sphere>();
		numOfUnmatchedSpheres = 0;
		numOfSpheres = 0;
		timeSinceLastEnteredSphere = 1000;
		numOfSmokedSpheres = 0;
		
		text.gameObject.SetActive(gm.debug);
		filledMark.SetActive(false);
		
		if (bobble != null) {
			bobble.onBobblePopped.Add((num) => {
				numOfSpheresInBobble = num;
				timeSinceBobblePopped = 0f;
			});
		}
		timeSinceBobblePopped = -1;
    }

    // Update is called once per frame
    void Update()
    {
		if (animator != null) {
			if (isFilled && fillingGroupCompletedFlag) {
				if (animator.GetCurrentAnimatorStateInfo(0).IsName("excited")) {
					animator.Play("Base Layer." + "excited_to_happy");
				} else if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle")) {
					animator.Play("Base Layer." + "idle_to_excited");
				}
			} else {
				if (timeSinceLastEnteredSphere > 1 && animator.GetCurrentAnimatorStateInfo(0).IsName("excited")) {
					animator.Play("Base Layer." + "excited_to_idle");
				}
				if (timeSinceLastEnteredSphere < 1 && animator.GetCurrentAnimatorStateInfo(0).IsName("idle")) {
					animator.Play("Base Layer." + "idle_to_excited");
				}
			}
		}
		timeSinceLastEnteredSphere += Time.deltaTime;
		
		if (isFilled && numOfUnmatchedSpheres < 5 && numOfUnmatchedSpheres > 0) {
			foreach (Sphere sphere in unmatchedSpheres) {
				if (!sphere.toDestroy) {
					gm.DestroySphere(sphere);
					//gm.SpawnSphere(transform.position, sphere.color);	
				}
			}
		}
		
		if (bobble != null && bobble.state == BobbleState.Popped) {
			timeSinceBobblePopped += Time.deltaTime;
			
			if (!ranOutOfBobblesFlag && !isFilled && 
				timeSinceBobblePopped > 0.5f && (numOfSpheresInBobble < fillingLimit)) {
				ranOutOfBobblesFlag = true;
				gm.Retry();
				Debug.Log("3");
			} else if (!ranOutOfBobblesFlag && !isFilled && timeSinceBobblePopped > bubblePoppedPeriodThreshold) {
				if (bobble.liquidColor == color && numOfSpheresInBobble - 2 > fillingLimit) {
					Filled();
				} else {
					ranOutOfBobblesFlag = true;
					Debug.Log("4");
					gm.Retry();
				}
			}
		}
		
		if (!smokeThresholdFlag && numOfSmokedSpheres > 5 && timeSinceLastEnteredSphere > 0.1f) {
			smokeThresholdFlag = true;
			gm.Retry();
			Debug.Log("6");
		}
		
		/*
		if (isFilled) {
			if (timeSinceFilled < 0.5f) {
				Color tmp = filledMark.GetComponent<SpriteRenderer>().color;
				tmp.a = Mathf.Min(timeSinceFilled * 2, 1);
				Debug.Log(tmp.a);
				filledMark.GetComponent<SpriteRenderer>().color = tmp;
			}
			timeSinceFilled += Time.deltaTime;
		}*/
    }
	
	public void ToggleBarriers() {
		foreach (GameObject barrier in barriers) {
			barrier.SetActive(!barrier.activeSelf);
		}
	}
	
	private void OnTriggerEnter(Collider collider) {
		//if (collider.gameObject.GetComponent<Sphere>() == null) return;
		
		numOfSpheres++;
		text.text = numOfSpheres.ToString();

		
		if (collider.gameObject.GetComponent<Sphere> ().color != color) {
			numOfUnmatchedSpheres++;
			unmatchedSpheres.Add(collider.gameObject.GetComponent<Sphere>());
		}
		text.text = numOfSpheres.ToString() + ", " + numOfUnmatchedSpheres.ToString();
		
		collider.gameObject.GetComponent<Sphere>().tank = this;
		
		
		if (isFilled || unmatchThresholdFlag) return;
		
		timeSinceLastEnteredSphere = 0;
		
		
		if (toFill && !unmatchThresholdFlag && numOfUnmatchedSpheres >= 5) {
			unmatchThresholdFlag = true;
			gm.Retry();
			Debug.Log("5");
		}
		
		if (toFill && numOfSpheres >= fillingLimit && !isFilled && !nearTorch) {
			if (collider.gameObject.GetComponent<Sphere> ().color == color) {
				Filled();
			} else if (numOfUnmatchedSpheres >= 3) {
				unmatchThresholdFlag = true;
				gm.Retry();
			}
			
		}
	}
	
	public void OnTriggerExit(Collider collider) {
		//if (collider.gameObject.GetComponent<Sphere>() == null) return;
		
		numOfSpheres--;
		text.text = numOfSpheres.ToString();
		
		
		if (collider.gameObject.GetComponent<Sphere> ().color != color) {
			numOfUnmatchedSpheres--;
			unmatchedSpheres.Remove(collider.gameObject.GetComponent<Sphere>());
		}
		text.text = numOfSpheres.ToString() + ", " + numOfUnmatchedSpheres.ToString();
		
		collider.gameObject.GetComponent<Sphere>().tank = null;
		
		/*
		if (isFilled) {
			gm.DestroySphere(collider.gameObject.GetComponent<Sphere>());
		}*/
		
		/*
		if (isFilled && !collider.gameObject.GetComponent<Sphere>().toDestroy) {
			gm.SpawnSphere(transform.position, color);	
		}*/
	}
	
	private void OnTriggerStay(Collider collider) {

	}
	
	private void Filled() {
		isFilled = true;
		gm.OnTankFilled(this, numOfUnmatchedSpheres < 5);
	}
	
	void OnValidate() {
		if (gm == null) gm = GameManager.Instance;
		
		if (gm == null) return;
		
		if (color != null) {
			foreach (GameObject obj in sides) {
				obj.GetComponent<MeshRenderer>().sharedMaterial = gm.wallColors[(int)color];
			}
		}
	}
	
	public void OnBottomTriggerEnter(GameObject obj) {
		if (nearTorch) {
			Sphere sphere = obj.GetComponent<Sphere>();
			if (!sphere.toDestroy) {
				sphere.toSmoke = true;
				gm.SpawnSmoke(sphere);
				gm.DestroySphere(sphere);
				
				numOfSmokedSpheres++;
			}
		}
	}
	
	public void OnFillingGroupCompleted() {
		fillingGroupCompletedFlag = true;
		ToggleBarriers();
		/*
		filledMark.SetActive(true);
		Color tmp = filledMark.GetComponent<SpriteRenderer>().color;
		tmp.a = 0f;
		filledMark.GetComponent<SpriteRenderer>().color = tmp;
		*/
		timeSinceFilled = 0f;
	}
	
	public void OnSave() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were found in the scene when saving the game. Key: id, value: ObjectIdentifier
		colorToInt = (int) color;
	}

	public void OnLoad() {
		//Use SaveLoad.objectIdentifierDict to access all ObjectIdentifers that were reconstructed when loading the game. Key: id, value: ObjectIdentifier
		color = Util.GetEnumValue<LiquidColor>(colorToInt); 
		if (transform.Find("Gate") != null) {
			sides.RemoveAt(sides.Count - 1);
			sides.RemoveAt(sides.Count - 1);
			
			Gate gate = transform.Find("Gate").GetComponent<Gate>();
			//Debug.Log("Gate Finded: " + gate.leftDoor.transform.GetChild(0).gameObject + ", " + gate.rightDoor.transform.GetChild(0).gameObject);
			sides.Add(gate.leftDoor.transform.GetChild(0).gameObject);
			sides.Add(gate.rightDoor.transform.GetChild(0).gameObject);
		}
		
		OnValidate();
		
		gm.OnObjectLoaded(this);
	}
}
