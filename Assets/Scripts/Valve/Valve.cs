using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SaveNoMembers]
public class Valve : MonoBehaviour
{
	[SerializeField] public Nozzle nozzle;
	[SerializeField][SaveMember] public Hose hose;
	[SerializeField] public GameObject handle;
	[SerializeField] public GameObject pipe;
	[SerializeField] public Animator pipeAnimator;
	[SerializeField] public Transform deploy;
	[SerializeField] public Transform start;
	
	private GameManager gm;
	private bool isOpened;
	public int numOfSpheres;
	public SourceTank sourceTank;
	
	private float remainingTimeToNextSpawn;
	
	public List<List<Sphere>> releasedSpheres;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.Instance;
		numOfSpheres = 0;
		releasedSpheres = new List<List<Sphere>>();
		foreach (LiquidColor color in gm.liquidColorsArray) {
			releasedSpheres.Add(new List<Sphere>());
		}
    }
	
	void Update() {
		/*
		if (gm.hose.IsLengthChanged()) {
			handle.transform.Rotate(0, 0, 20, Space.World);
		}*/
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		if (sourceTank != null && sourceTank.spheres.Count > 0) {
			if (remainingTimeToNextSpawn <= 0) {
				for (int i = 0; i < 1 && (sourceTank.spheres.Count != 0); i++) {
					Sphere sphere = sourceTank.spheres[0];
					sphere.transform.position = deploy.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
					sphere.GetComponent<Rigidbody>().velocity = new Vector3(0, -10f, 0);
					sphere.valve = this;
					if (nozzle.isInZone) {
						sphere.SetColor(nozzle.zoneColor);
						//Debug.Log("In zone, color: " + nozzle.zoneColor);
					} else {
						//Debug.Log("Color: " + sphere.color);
					}
					
					sourceTank.spheres.RemoveAt(0);
					
					releasedSpheres[(int) sphere.color].Add(sphere);
					
					if (gm.finishedLiquidColors.Contains(sphere.color.Value)) {
						gm.DestroySphere(sphere);
					}
				}
				remainingTimeToNextSpawn = 0.01f;
			} else {
				remainingTimeToNextSpawn -= Time.deltaTime;
			}
			handle.transform.Rotate(0, 0, 20, Space.World);
			if (pipeAnimator.GetCurrentAnimatorStateInfo(0).IsName("idle")) {
				pipeAnimator.Play("Base Layer." + "busy");
				//Debug.Log("Pipe busy");
			}
		} else {
			if (pipeAnimator.GetCurrentAnimatorStateInfo(0).IsName("busy")) {
				pipeAnimator.Play("Base Layer." + "idle");
			}
		}
    }
	
	public void Open() {
		if (isOpened) return;
		
		isOpened = true;
		gm.OnValveOpened();
	}
	
	public void SetSourceTank(SourceTank sourceTank) {
		this.sourceTank = sourceTank;
		remainingTimeToNextSpawn = 0;
		if (sourceTank != null) {
			if (nozzle.isInZone) {
				pipe.GetComponent<MeshRenderer> ().sharedMaterial = gm.wallColors[(int)nozzle.zoneColor];
			} else {
				pipe.GetComponent<MeshRenderer> ().sharedMaterial = gm.wallColors[(int)sourceTank.liquidColor];
			}
		} else {
			pipe.GetComponent<MeshRenderer> ().sharedMaterial = gm.defaultMaterial;
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
