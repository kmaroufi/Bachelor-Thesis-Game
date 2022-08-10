using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneData {
	public bool upDirection;
	public ColorZone zone;
	
	public ZoneData(bool upDirection, ColorZone zone) {
		this.upDirection = upDirection;
		this.zone = zone;
	}
}

public class ColorZone : MonoBehaviour
{
	[SerializeField] public LiquidColor color;
	[SerializeField] public List<ParticleSystem> effects;
	[SerializeField] public GameObject cube;
	[SerializeField] public ZoneTrigger bottomTrigger;
	[SerializeField] public ZoneTrigger topTrigger;
	[SerializeField] public ZoneTrigger externalTrigger;
	[SerializeField] public ZoneTrigger border;
	private GameManager gm;
	private GameObject limitObj;
	private List<Nozzle> nozzles;
	private Dictionary<Nozzle, Vector3> lastPoses;
	private bool exitedBorder;
	private bool enteredBorder;
	private bool? enteredFromBottom;
	private bool borderFlag;
	//private Dictionary<Nozzle, Vector3> lastPoses;
    // Start is called before the first frame update
    void Start()
    {
		if (gm == null) {
			OnValidate();
		}
        gm = GameManager.Instance;
		limitObj = new GameObject();
		
		nozzles = new List<Nozzle>();
		foreach (FillingOrder order in gm.fillingOrders) {
			nozzles.Add(order.valve.nozzle);
		}
		
		
		lastPoses = new Dictionary<Nozzle, Vector3>();
		for (int i = 0; i < nozzles.Count; i++) {
			lastPoses[nozzles[i]] = nozzles[i].trigger.transform.position;
		}
		//StartCoroutine(UpdatePositions());
		StartCoroutine(Check());
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }
	
	public void OnZoneTriggerEnter(ZoneTrigger trigger, Nozzle nozzle) {
		/*
		if (trigger == bottomTrigger) {
			
		} else if (trigger == topTrigger) {
			if (bottomTrigger.isStayingOnTrigger || BorderPassedCheck(nozzle, true)) {
				nozzle.SetZone(this);
				Debug.Log("zone set");
			}
		}
		*/
		if (trigger == externalTrigger) {
			enteredFromBottom = nozzle.trigger.transform.position.y < trigger.transform.position.y;
		}
		if (trigger == border) {
			enteredBorder = true;
			borderFlag = false;
			Debug.Log("enteredBorder");
		}
	}
	
	public void OnZoneTriggerExit(ZoneTrigger trigger, Nozzle nozzle) {
		/*
		if (trigger == bottomTrigger) {
			
		} else if (trigger == topTrigger) {
			if (nozzle.isInZone) {
				if (nozzle.currentZone == this) {
					Collider[] colls = Physics.OverlapSphere(nozzle.trigger.transform.position, 0.5f, 
					LayerMask.GetMask("ColorZone"), QueryTriggerInteraction.Collide);
					Debug.Log("collider size: " + colls.Length);
						
					bool flag = false;
					for (int i = 0; i < colls.Length; i++) {
						if (colls[i].gameObject.tag == "ColorZoneBottomTrigger") {
							flag = true;
							break;
						}
					}
					
					if (flag || BorderPassedCheck(nozzle, false)) {
						nozzle.SetZone(null);
						Debug.Log("zone null");
					}
				}
			}
		}
		*/
		/*
		if (nozzle.isInZone && ((nozzle.currentZone.upDirection && BorderPassedCheck(nozzle, false)) || 
			(!nozzle.currentZone.upDirection && BorderPassedCheck(nozzle, true)))) {
			nozzle.SetZone(null);
			Debug.Log("set null");
		} else {
			Vector3 path = nozzle.trigger.transform.position - lastPoses[nozzle];
			nozzle.SetZone(new ZoneData(path.y > 0, this));
			Debug.Log("set zone");
		}
		*/

		if (trigger == border) {
			exitedBorder = true;
			Debug.Log("exitedBorder");
		}
	}
	
	public void OnZoneTriggerExit(ZoneTrigger trigger, Sphere sphere) {
		if (trigger == topTrigger) {
			sphere.SetColor(color);
		}
	}
	
	
	private bool BorderPassedCheck(Nozzle nozzle, bool upDirection, bool multiply) {
		Vector3 path = nozzle.trigger.transform.position - lastPoses[nozzle];
		path.x = 0;
		path.z = 0;
		Ray ray = new Ray(lastPoses[nozzle] - Vector3.Normalize(path) * (multiply ? 5 : 1), path);
		
		RaycastHit[] hits = Physics.RaycastAll(ray, (multiply ? 10 : 2), 
			LayerMask.GetMask("ColorZone"), QueryTriggerInteraction.Collide);
		//Debug.Log("hits size: " + hits.Length);
		
		bool flag = false;
		for (int i = 0; i < hits.Length; i++) {
			if (hits[i].collider.gameObject.tag == "ColorZoneBorder" && hits[i].transform.parent.gameObject == gameObject) {
				flag = true;
				break;
			}
		}
		
		if (!flag) {
			Collider[] colls = Physics.OverlapSphere(nozzle.trigger.transform.position, 0.5f, 
				LayerMask.GetMask("ColorZone"), QueryTriggerInteraction.Collide);
			//Debug.Log("collider size: " + colls.Length);
			
			for (int i = 0; i < colls.Length; i++) {
				if (colls[i].gameObject.tag == "ColorZoneBorder" && colls[i].transform.parent.gameObject == gameObject) {
					flag = true;
					break;
				}
			}
		}
		
		
		//Debug.Log("BorderPassedCheck: " + (upDirection ? "up " : "down ") + flag + ", " + (flag && (upDirection ? path.y > 0 : path.y < 0)) + ", " + path.y);
		
		return flag && (upDirection ? path.y > 0 : path.y < 0);
	}
	
	/*
	private IEnumerator UpdatePositions() {
		while (gm.state == GameState.Playing) {
			for (int i = 0; i < nozzles.Count; i++) {
				lastPoses[nozzles[i]] = nozzles[i].trigger.transform.position;
			}
			
			yield return new WaitForFixedUpdate();
		}
		
	}
	*/
	
	private IEnumerator Check() {
		yield return new WaitForSeconds(0.2f);
		enteredBorder = false;
		/*
		Dictionary<Nozzle, Vector3> lastPoses = new Dictionary<Nozzle, Vector3>();
		for (int i = 0; i < nozzles.Count; i++) {
			lastPoses[nozzles[i]] = nozzles[i].transform.position;
		}*/
		while (gm.state == GameState.Playing) {
			/*
			if (!exitedTrigger) {
				for (int i = 0; i < nozzles.Count; i++) {
					Nozzle nozzle = nozzles[i];
					
					Vector3 path = nozzle.trigger.transform.position - lastPoses[nozzle];
					
					if (BorderPassedCheck(nozzle, true, false)) {
						enteredFromBottom = true;
						exitedTrigger = true;
					} else if (BorderPassedCheck(nozzle, false, false)) {
						enteredFromBottom = false;
						exitedTrigger = true;
					} 
				}
			}
			*/
			if (enteredBorder) {
				for (int i = 0; i < nozzles.Count; i++) {
					Nozzle nozzle = nozzles[i];
					if (enteredFromBottom == null) {
						enteredFromBottom = nozzle.trigger.transform.position.y < border.transform.position.y;
					}
					Vector3 path = nozzle.trigger.transform.position - lastPoses[nozzle];
					if (nozzle.isInZone && nozzle.currentZone.zone == this && 
						((nozzle.currentZone.upDirection && BorderPassedCheck(nozzle, false, true)) || 
						(!nozzle.currentZone.upDirection && BorderPassedCheck(nozzle, true, true)))) {
						nozzle.SetZone(null);
						Debug.Log("set null");
						borderFlag = true;
					} else if (!nozzle.isInZone || (((bool) enteredFromBottom) && path.y > 0) || (((bool) !enteredFromBottom)  && path.y < 0)) {
						nozzle.SetZone(new ZoneData(path.y > 0, this));
						Debug.Log("set zone " + path.y);
					}
				}
				enteredBorder = false;
				enteredFromBottom = null;
			}
			
			if (exitedBorder) {
				for (int i = 0; i < nozzles.Count; i++) {
					Nozzle nozzle = nozzles[i];
					Vector3 path = nozzle.trigger.transform.position - lastPoses[nozzle];
					if (nozzle.isInZone && nozzle.currentZone.zone == this && 
						((nozzle.currentZone.upDirection && nozzle.trigger.transform.position.y < border.transform.position.y) || 
						(!nozzle.currentZone.upDirection && nozzle.trigger.transform.position.y > border.transform.position.y))) {
						if (!borderFlag) {
							nozzle.SetZone(null);
							Debug.Log("set null");
						}
					}
				}
				exitedBorder = false;
				borderFlag = false;
			}
			
			for (int i = 0; i < nozzles.Count; i++) {
				lastPoses[nozzles[i]] = nozzles[i].trigger.transform.position;
			}
			
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	void OnValidate() {
		if (gm == null) gm = GameManager.Instance;
		
		if (gm == null) return;
		
		if (color != null) {
			Color relatedColor = gm.wallColors[(int)color].color;
			Color c = new Color(relatedColor.r, relatedColor.g, relatedColor.b, relatedColor.a);
			c.a = 30.0f / 255;
			effects[0].startColor = c;
			
			//effects[1].startColor = relatedColor;
			
			cube.GetComponent<MeshRenderer> ().sharedMaterial = gm.wallColors[(int)color];
		}
	}
}
