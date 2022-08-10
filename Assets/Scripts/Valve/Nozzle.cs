using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NozzleState {Free, Docking, Docked, UnDocking}

public class Nozzle : MonoBehaviour
{	
	[SerializeField] public Valve valve;
	[SerializeField] public GameObject body;
	[SerializeField] public GameObject trigger;
	[SerializeField] public GameObject obj;	
	private GameManager gm;
	private Camera camera;
	private Plane plane;
	private SourceTank sourceTank;
	
	private NozzleState state;
	public Rigidbody rb;
	public bool touched;
	private Vector3 lastPosition;
	private bool isLastPositonValid;
	private float maxY;
	public LiquidColor zoneColor;
	public List<ZoneData> zones;
	public bool isInZone;
	private Vector3 futurePosition;
	private PositionLimit pl;
	private Vector3 lastRbPosition;
	private Vector3 lastRbVelocity;
	private SourceTank previousSourceTank;
	
	public ZoneData currentZone {
		get { return isInZone ? zones[zones.Count - 1] : null; }
	}
    // Start is called before the first frame update
    void Start()
    {
       	gm = GameManager.Instance;
		rb = GetComponent<Rigidbody>();
		transform.Find("Head").GetComponent<MeshRenderer>().sharedMaterial = gm.defaultMaterial;
		body.GetComponent<MeshRenderer>().sharedMaterial = gm.blackMaterial;
		camera = gm.cameras[0];
		Vector3 distance = new Vector3(0, 0, transform.position.z);
		plane  = new Plane(Vector3.back, distance);
		state = NozzleState.Free;
		touched = false;
		isLastPositonValid = false;
		maxY = gm.sourceTanks[0].nozzle.transform.position.y;
		zones = new List<ZoneData>();
		
		pl = GetComponent<PositionLimit>();
		pl.leftDown = gm.nozzleBoundary.leftDown;
		pl.rightTop = gm.nozzleBoundary.rightTop;
		pl.enabled = false;
		
		//Debug.Log("Nozzle Start");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		if (!touched) {
			touched = IsNozzleTouched() && (gm.tankMover == null || gm.tankMover.state != TankMoverState.Moving);
			isLastPositonValid = false;
		} else {
			touched = Input.touchCount == 1 || ((Input.touchCount == 0) && Input.GetMouseButton(0));
			/*
			if (Input.touchCount == 1) {
				Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
				//Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);
				
				float enter = 0.0f;
				if (plane.Raycast(ray, out enter)) {
					//Debug.Log("salam");
					Vector3 hitPoint = ray.GetPoint(enter);
					Vector3 position = new Vector3(hitPoint.x, hitPoint.y, 0);
					if (position.y > maxY) {
						touched = false;
					}
				}
			} else {
				touched = false;
			}
			*/
		}
		
		if (Input.touchCount == 0 || !Input.GetMouseButton(0)) obj.transform.localPosition = new Vector3(0, -3, 0);
		
		switch (state) {
			case NozzleState.Docking:
				rb.velocity = new Vector3(0, 0, 0);
				transform.position = Vector3.Lerp(transform.position, sourceTank.nozzle.transform.position, 0.5f);
				if (Vector3.Distance(transform.position, sourceTank.nozzle.transform.position) < 0.1) {
					state = NozzleState.Docked;
					isLastPositonValid = false;
					
					transform.position = sourceTank.nozzle.transform.position;
					
					touched = false;
					
					gm.OnDockedListener(this, sourceTank);
				} else if (touched) {
					/*
					Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
					//Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);
					
					float enter = 0.0f;
					if (plane.Raycast(ray, out enter)) {
						//Debug.Log("salam");
						Vector3 hitPoint = ray.GetPoint(enter);
						Vector3 position = new Vector3(hitPoint.x, hitPoint.y, 0);
						
						if (isLastPositonValid) {
							transform.position = transform.position + position - lastPosition;
						} else {
							isLastPositonValid = true;
						}
						lastPosition = position;
						
					}
					SourceTank tank = GetSourceTankNozzle();
					if (tank == null) {
						this.sourceTank = null;
						state = NozzleState.Free;
					}
					*/
				}
				break;
			case NozzleState.Docked:
				rb.velocity = new Vector3(0, 0, 0);
				if (Input.touchCount == 1 || ((Input.touchCount == 0) && Input.GetMouseButton(0))) {
					Ray ray = gm.cameras[0].ScreenPointToRay(Input.touchCount == 1 ? Input.GetTouch(0).position : 
						new Vector2(Input.mousePosition.x, Input.mousePosition.y));
					//Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);
					
					float enter = 0.0f;
					if (plane.Raycast(ray, out enter)) {
						//Debug.Log("salam");
						Vector3 hitPoint = ray.GetPoint(enter);
						Vector3 position = new Vector3(hitPoint.x, hitPoint.y, 0);
						
						if (!isLastPositonValid) {
							isLastPositonValid = true;
							lastPosition = position;
							
						}
						position.y = Mathf.Min(maxY, position.y);
						Vector3 point = position;
						
						obj.transform.position = point;
						
						lastPosition = point;
						
						/*
						Collider[] colliders = Physics.OverlapSphere(point, 0.5f, LayerMask.GetMask("SourceTank"), QueryTriggerInteraction.Collide);
						bool flag = false;
						foreach (Collider collider in colliders) {
							if (collider.gameObject.tag == "SourceTankExternalTrigger" && 
								collider.transform.parent.gameObject.GetComponent<SourceTank>() == sourceTank) {
								flag = true;
								break;
							}	
						}
						if ((colliders.Length == 0) || !flag) {
							lastPosition = point;
							
							gm.OnUnDockedListener(this, sourceTank);
							this.sourceTank = null;
							state = NozzleState.UnDocking;
							isLastPositonValid = false;
						}
						*/
					}
				}
				break;
			case NozzleState.UnDocking:
				rb.velocity = new Vector3(0, 0, 0);
				if (touched) {
					Ray ray = gm.cameras[0].ScreenPointToRay(Input.touchCount == 1 ? Input.GetTouch(0).position : 
						new Vector2(Input.mousePosition.x, Input.mousePosition.y));
					//Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);
					
					float enter = 0.0f;
					if (plane.Raycast(ray, out enter)) {
						//Debug.Log("salam");
						Vector3 hitPoint = ray.GetPoint(enter);
						Vector3 position = new Vector3(hitPoint.x, hitPoint.y, 0);
						position.y = Mathf.Min(maxY, position.y);
						lastPosition = position;
					}
				}
				transform.position = Vector3.Lerp(transform.position, lastPosition, 0.3f);
				if (Vector3.Distance(transform.position, lastPosition) < 0.1f) {
					state = NozzleState.Free;
					rb.isKinematic = false;
					sourceTank.externalTrigger.GetComponent<BoxCollider>().isTrigger = true;
					sourceTank = null;
					isLastPositonValid = false;
					
					transform.position = lastPosition;
					
					
					Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("SourceTank"), QueryTriggerInteraction.Collide);
					//Debug.Log("collider.Count: " + colliders.Length);
					foreach (Collider collider in colliders) {
						//Debug.Log("Tag: " + collider.gameObject.tag);
						if (collider.gameObject.tag == "SourceTankInternalTrigger") {
							//Debug.Log("WOW");
							gm.OnDockingListener(this, collider.transform.parent.gameObject.GetComponent<SourceTank>());
						}
					}
				} 
				break;
			case NozzleState.Free:
				if (touched) {
					Ray ray = gm.cameras[0].ScreenPointToRay(Input.touchCount == 1 ? Input.GetTouch(0).position : 
						new Vector2(Input.mousePosition.x, Input.mousePosition.y));
					//Debug.Log(Input.GetTouch(0).position);
					//Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);
					//Debug.Log("Free touched");
					float enter = 0.0f;
					if (plane.Raycast(ray, out enter)) {
						//Debug.Log("salam");
						Vector3 hitPoint = ray.GetPoint(enter);
						Vector3 position = new Vector3(hitPoint.x, hitPoint.y, 0);
						
						/*
						if (Vector3.Distance(transform.position, position) > 0.1f) {
							if (valve.hose.IsLengthIncreased()) {
								valve.handle.transform.Rotate(0, 0, 20, Space.World);
							} else {
								valve.handle.transform.Rotate(0, 0, -20, Space.World);
							}
						}
						*/
						position.y = Mathf.Min(position.y, maxY);
						
						if (isLastPositonValid) {
							futurePosition = futurePosition + position - lastPosition;
							//transform.position = transform.position + position - lastPosition;
							//rb.position = position;
							//Debug.Log("rb.position: " + rb.position);
						} else {
							isLastPositonValid = true;
							futurePosition = transform.position;
						}
						lastPosition = position;
						
						
						if ((lastRbPosition - rb.position).magnitude < 0.01 && lastRbVelocity.magnitude > 5) {
							futurePosition = rb.position;
							touched = false;
						}
						
						//Debug.Log("lastPosition: " + lastPosition);
						//transform.position = Vector3.Lerp(transform.position, futurePosition, 1f);
						Vector3 deff = futurePosition - transform.position;
						float deffMagnitude = deff.magnitude;
						if (deffMagnitude < 0.1) {
							rb.velocity = new Vector3(0, 0, 0);
						} else {
							rb.velocity = Vector3.ClampMagnitude(deff * 10 * (deffMagnitude > 0.5 ? Mathf.Pow(deffMagnitude * 2, 3) : 1), 50);
							if (rb.velocity.magnitude < 2) {
								rb.velocity = Vector3.Normalize(rb.velocity) * 2;
							}
						}
						/*
						if (deffMagnitude < 0.1) {
							rb.velocity = new Vector3(0, 0, 0);
						} else {
							//rb.velocity = Vector3.Normalize(deff) * Mathf.Clamp(deff.magnitude * 50, 10, 100);
							if (true) {
								
								
							} else {
								rb.velocity = Vector3.ClampMagnitude(deff * 50, 50);
								/*
								Vector3 v = rb.velocity;
								v += Vector3.ClampMagnitude(deff, 50);
								v = Vector3.Normalize(deff) * v.magnitude;
								rb.velocity = v;
								*/
								/*
							}
							
							//Debug.Log("rb.velocity: " + rb.velocity);
						}
						*/
					}
					
					obj.transform.localPosition = new Vector3(0, -3, 0);
				} else {
					rb.velocity = new Vector3(0, 0, 0);
				}
				break;
		}
		//rb.velocity = new Vector3(0, 0, 0);
		//Debug.Log("touchs: " + Input.touchCount);
		if (state != NozzleState.Docking) {
				Vector3 velocity = rb.velocity;
			Vector3 pos = rb.position;
			if (gameObject.transform.position.x + rb.velocity.x * Time.fixedDeltaTime < pl.leftDown.position.x) {
				velocity.x = 0;
				pos.x = pl.leftDown.position.x;
			}
			if (gameObject.transform.position.x + rb.velocity.x * Time.fixedDeltaTime > pl.rightTop.position.x) {
				velocity.x = 0;
				pos.x = pl.rightTop.position.x;
			}
			if (gameObject.transform.position.y + rb.velocity.y * Time.fixedDeltaTime < pl.leftDown.position.y) {
				velocity.y = 0;
				pos.y = pl.leftDown.position.y;
			}
			if (gameObject.transform.position.y + rb.velocity.y * Time.fixedDeltaTime > pl.rightTop.position.y) {
				velocity.y = 0;
				pos.y = pl.rightTop.position.y;
			}
			if (gameObject.transform.position.z + rb.velocity.z * Time.fixedDeltaTime < pl.leftDown.position.z) {
				velocity.z = 0;
				pos.z = pl.leftDown.position.z;
			}
			if (gameObject.transform.position.z + rb.velocity.z * Time.fixedDeltaTime > pl.rightTop.position.z) {
				velocity.z = 0;
				pos.z = pl.rightTop.position.z;
			}
			rb.velocity = velocity;
			rb.position = pos;
			
			lastRbPosition = rb.position;
			lastRbVelocity = rb.velocity;
		}
		
		
		return;
		
		rb.position = new Vector3(Mathf.Clamp(gameObject.transform.position.x + rb.velocity.x * Time.fixedDeltaTime ,pl.leftDown.position.x,pl.rightTop.position.x),
									  Mathf.Clamp(gameObject.transform.position.y + rb.velocity.y * Time.fixedDeltaTime,pl.leftDown.position.y,pl.rightTop.position.y),
									  Mathf.Clamp(gameObject.transform.position.z + rb.velocity.z * Time.fixedDeltaTime,pl.leftDown.position.z,pl.rightTop.position.z));
    }
	
	private SourceTank GetSourceTankNozzle() {
		if(Input.touchCount == 1) {
			Touch touch = Input.GetTouch(0);
			Ray ray = gm.cameras[0].ScreenPointToRay(Input.GetTouch(0).position);
			RaycastHit hit;
			//Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);
			
			if(Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("SourceTank"), QueryTriggerInteraction.Collide)) {
				//Debug.Log(hit.transform.gameObject);
				if (hit.collider != null && hit.collider.gameObject.tag == "SourceTankInternalTrigger") {
					return hit.collider.transform.parent.gameObject.GetComponent<SourceTank>();
				}
			}
		}
		return null;
	}
	
	private bool IsNozzleTouched() {
		//Debug.Log("nozzleTouched: 1");
		if (!touched && gm.IsNozzleTouched()) return false;
		//Debug.Log("nozzleTouched: 2");
		
		if(Input.touchCount == 1 || ((Input.touchCount == 0) && Input.GetMouseButton(0))) {
			Ray ray = gm.cameras[0].ScreenPointToRay(Input.touchCount == 1 ? Input.GetTouch(0).position : 
						new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			RaycastHit hit;
			//Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);
			//Debug.Log("ray: " + ray);
			if(Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Nozzle"), QueryTriggerInteraction.Collide)) {
				//Debug.Log(hit.transform.gameObject);
				return hit.collider != null && hit.collider.gameObject == gameObject;
			}
		}
		return false;
	}
	
	public void SetSourceTank(SourceTank tank) {
		Debug.Log("Set Source Tank: " + tank);
		this.sourceTank = tank;
		if (tank != null) {
			state = NozzleState.Docking;
			rb.isKinematic = true;
			sourceTank.externalTrigger.GetComponent<BoxCollider>().isTrigger = false;
		}
		/*
		GameObject obj = tank.transform.Find("External Trigger").transform.gameObject;
		BoxCollider collider = obj.GetComponent<BoxCollider>();
		*/
	}
	
	public void SetZone(ZoneData zoneData) {
		if (zoneData != null) {
			zones.Add(zoneData);
			if (!isInZone) {
				isInZone = true;
			}
			zoneColor = zoneData.zone.color;
			valve.hose.GetComponent<MeshRenderer> ().sharedMaterial = gm.wallColors[(int)zoneColor];
		} else {
			zones.RemoveAt(zones.Count - 1);
			if (zones.Count > 0) {
				zoneColor = zones[zones.Count - 1].zone.color;
				valve.hose.GetComponent<MeshRenderer> ().sharedMaterial = gm.wallColors[(int)zoneColor];
			} else {
				isInZone = false;
				valve.hose.GetComponent<MeshRenderer> ().sharedMaterial = gm.defaultMaterial;
			}
		}
	}
	
	
	public void OnTouchObjEnteredSourceTankExternalTrigger(SourceTank sourceTank) {
		if (this.sourceTank != sourceTank) return;
		
		touched = state == NozzleState.Docked && (Input.touchCount == 1 || ((Input.touchCount == 0) && Input.GetMouseButton(0)));
	}
	
	public void OnTouchObjExitedSourceTankExternalTrigger(SourceTank sourceTank) {
		if (touched && state == NozzleState.Docked && this.sourceTank == sourceTank) {
			gm.OnUnDockedListener(this, sourceTank);
			
			state = NozzleState.UnDocking;
			isLastPositonValid = false;
			obj.transform.localPosition = new Vector3(0, -3, 0);
		}
	}
}
