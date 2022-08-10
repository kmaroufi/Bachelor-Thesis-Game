using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kmaroufi;
using DentedPixel;


public enum BobbleState {Idle, Moving, Popped}

[SaveNoMembers]
public class Bobble : MonoBehaviour
{
	[SerializeField][SaveMember] public float sphereScale = 1;
	[SerializeField] public GameObject fakeObject;
	[SerializeField] public List<ParticleSystem> poppedEffects;
	[SerializeField] public GameObject bobble;
	[SerializeField] public Action onBobbleMovingAction;
	[SerializeField] public List<System.Action<int>> onBobblePopped = new List<System.Action<int>>();
	
	private GameManager gm;
	private Animator animator;
	private List<Sphere> spheres;
	public BobbleState state;
	private Vector3 velocity;
	public LiquidColor? liquidColor;
	private float timeSinceLastSphereEntered;
	private int idleEffectId;
    // Start is called before the first frame update
    void Start()
    {
		gm = GameManager.Instance;
		animator = GetComponent<Animator>();
		
        spheres = new List<Sphere>();
		state = BobbleState.Idle;
		fakeObject.SetActive(false);
		
		timeSinceLastSphereEntered = 1000;
		idleEffectId = -1;
    }

    // Update is called once per frame
    void Update()
    {
		if (state == BobbleState.Moving) {
			velocity = new Vector3(0, Mathf.Min(36, velocity.y + 30 * Time.deltaTime), 0);
			transform.Translate(velocity * Time.deltaTime);
		}
    }
	
	void OnTriggerEnter(Collider collider) {
		Sphere sphere = collider.gameObject.GetComponent<Sphere>();
		if (sphere == null || sphere.IsInSourceTank() || sphere.tank != null) return;
		
		if (spheres.Count == 0) {
			
			//if (onBobbleMovingAction != null) onBobbleMovingAction.Do(gameObject);
			
			liquidColor = sphere.color;
			fakeObject.SetActive(true);
			fakeObject.transform.localScale = new Vector3(0.1f, 0.1f, 1);
			fakeObject.GetComponent<MeshRenderer>().sharedMaterial = gm.liquidColors[(int)liquidColor];
		} else {
			if (liquidColor != sphere.color) {
				if (state != BobbleState.Moving && spheres.Count <= 100) {
					liquidColor = sphere.color;
					fakeObject.transform.localScale = new Vector3(0.1f, 0.1f, 1);
					fakeObject.GetComponent<MeshRenderer>().sharedMaterial = gm.liquidColors[(int)liquidColor];
					
					foreach (Sphere sphere1 in spheres) {
						gm.DestroySphere(sphere1);
					}
					spheres.Clear();
				} else {
					return;
				}
			}
			
			float size = Mathf.Min((spheres.Count + 1) / 16.0f, 1);
			fakeObject.transform.localScale = new Vector3(size, size, 1);
			
			if (spheres.Count > 10) {
				state = BobbleState.Moving;
				animator.Play("Base Layer." + "idle_to_moving");
			}
		}
		timeSinceLastSphereEntered = 0f;
		
		if (state == BobbleState.Moving) {
			if (animator.enabled == false) {
				animator.enabled = true;
			}
		} else {
			if (idleEffectId == -1) {
				idleEffectId = LeanTween.scale(gameObject, new Vector3(0.9f, 0.9f, 0.9f), 0.05f).setOnComplete(() => {
					idleEffectId = LeanTween.scale(gameObject, new Vector3(1.1f, 1.1f, 1.1f), 0.05f).setOnComplete(() => {
						idleEffectId = -1;
						animator.enabled = true;
					}).id;
				}).id;
				animator.enabled = false;
			}
		}

		
		/*
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle")) {
			animator.Play("Base Layer." + "idle_to_moving");
		}*/
		
		/*
		if (state == BobbleState.Moving) {
			velocity = new Vector3(0, -1, 0);
		}*/
		
		sphere.transform.parent = transform;
		sphere.transform.localPosition = Vector3.zero;
		sphere.transform.localScale = sphere.transform.localScale * sphereScale;
		sphere.gameObject.layer = LayerMask.NameToLayer("BobbleSphere");
		spheres.Add(sphere);
	}
	
	public void Pop() {
		if (state == BobbleState.Popped) return;
		
		Debug.Log("Bobble Popped with " + spheres.Count + " spheres");
		int numOfSpheres = spheres.Count;
		state = BobbleState.Popped;
		GetComponent<BoxCollider>().enabled = false;
		animator.Play("Base Layer." + "idle");
		transform.localScale = new Vector3(1, 1, 1);
		foreach (Sphere sphere in spheres) {
			//Debug.Log("bobble " + transform.localScale);
			float scale = sphere.transform.localScale.x / sphereScale;
			sphere.transform.localScale = new Vector3(scale, scale, scale);
			sphere.transform.parent = null;
			sphere.gameObject.layer = gm.GetLiquidLayer(sphere.color);
			Vector3 position = sphere.transform.position;
			position.z = 0;
			sphere.transform.position = position;
		}
		spheres.Clear();
		bobble.SetActive(false);
		fakeObject.SetActive(false);
		foreach (ParticleSystem effect in poppedEffects) {
			effect.Play();
		}
		
		foreach (System.Action<int> action in onBobblePopped) {
			action(numOfSpheres);
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
