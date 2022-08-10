using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kmaroufi;
using Obi;
using UnityStandardAssets.ImageEffects;

public enum GameState {Playing, Retry, Winner}

[System.Serializable]
public class FillingGroup {
	public List<Tank> tanks;
	public List<Action> onFillingActions;
}

[System.Serializable]
public class FillingOrder {
	public Valve valve;
	public List<FillingGroup> fillingGroups;
	[HideInInspector] public int currentGroupToFillIndex = 0;
}

[System.Serializable]
public class CandidateFillingOrders {
	public List<FillingOrder> orders;
}

[System.Serializable]
public class SerializedTransform {
	public Vector3 position;
	public Vector3 localPosition;
	public Quaternion rotation;
	public Vector3 localScale;
	
	public SerializedTransform(Transform t) {
		position = t.position;
		localPosition = t.localPosition;
		rotation = t.rotation;
		localScale = t.localScale;
	}
}

public enum LiquidColor {Green = 0, Blue = 1, Red = 2, Yellow = 3}

public class GameManager : SingletonComponent<GameManager>
{
	[SerializeField] public bool debug = false;
	[SerializeField] public bool isMainScene = false;
	
	[SerializeField] public List<LiquidColor> finishedLiquidColors;
	
	[SerializeField] public List<LiquidColor> liquidColorsArray;
	[SerializeField] public List<Material> liquidColors;
	[SerializeField] public List<Material> wallColors;
	[SerializeField] public Material defaultMaterial;
	[SerializeField] public Material blackMaterial;
	[SerializeField] public Material spriteMaterial;
	
	[SerializeField] public GameObject spherePrefab;
	[SerializeField] public GameObject smokePrefab;
	[SerializeField] public GameObject camerasObject;
	[HideInInspector] public List<Camera> cameras;
	[SerializeField] public List<ParticleSystem> dockedEffects;
	[SerializeField] public GameObject winEffectsObject;
	[HideInInspector] public List<ParticleSystem> winEffects;
	[SerializeField] public Animator guideHandAnimator;
	
	[SerializeField] public AbstractSPHSolver sphSolver;
	
	[SerializeField] public UIController uiController;
	
	[SerializeField] public LevelInfo levelInfo;
	
	
	[SerializeField] public List<Tank> tanks;
	[SerializeField] public List<GameObject> outSideBarriers;
	[SerializeField] public List<Valve> valves;
	[SerializeField] public List<SourceTank> sourceTanks;
	[SerializeField] public List<Hose> hoses;
	[SerializeField] public List<Wire> wires;
	[SerializeField] public ObiSolver obiSolver;
	[SerializeField] public List<Bobble> bobbles;
	[SerializeField] public List<Pin> pins;
	[SerializeField] public List<Passage> passages;
	[SerializeField] public List<Torch> torches;
	[SerializeField] public List<Fan> fans;
	[SerializeField] public List<Turbine> turbines;
	[SerializeField] public List<ColorZone> colorZones;
	[SerializeField] public TankMover tankMover;
	[SerializeField] public NozzleBoundary nozzleBoundary;
	[SerializeField] public GameObject bottomTrigger;
	
	[SerializeField] public List<FillingOrder> fillingOrders;
	[SerializeField] public List<CandidateFillingOrders> allCandidateFillingOrders;
	
	private Dictionary<Tank, int> tankFillingOrderIndex;
	
	//public int candidateFillingOrdersIndex;
	public GameState state;
	public int level;
	private int internalLevel;
	private List<Sphere> toDestorySpheres;
	private List<Sphere> tmpList;
	private bool firstDockFlag;
	
	void Awake() {
		// Cameras
		cameras = new List<Camera>();
		cameras.Add(camerasObject.transform.Find("Front").GetComponent<Camera>());
		cameras.Add(camerasObject.transform.Find("Water Effect Camera").GetComponent<Camera>());
		cameras.Add(camerasObject.transform.Find("Spheres Camera").GetComponent<Camera>());
		cameras.Add(camerasObject.transform.Find("Back").GetComponent<Camera>());
		
		// Confettis
		winEffects = new List<ParticleSystem>();
		winEffects.Add(winEffectsObject.transform.Find("1").GetComponent<ParticleSystem>());
		winEffects.Add(winEffectsObject.transform.Find("2").GetComponent<ParticleSystem>());
		winEffects.Add(winEffectsObject.transform.Find("3").GetComponent<ParticleSystem>());
		winEffects.Add(winEffectsObject.transform.Find("4").GetComponent<ParticleSystem>());
	}
	
    // Start is called before the first frame update
    void Start()
    {
		if (PlayerPrefs.GetInt("level", -1) == -1) {
			PlayerPrefs.SetInt("level", 1);
			PlayerPrefs.Save();
		} else {
			//PlayerPrefs.SetInt("level", 5);
			//PlayerPrefs.Save();
		}
		
		level = PlayerPrefs.GetInt("level", 1);
		internalLevel = (level - 1) % 17 + 1;
		
		uiController.Set();
		
        LoadLevel();
		InitSPHSolver();
		state = GameState.Playing;
		
		// AntiAliasing (its heavy for mobile)
		//cameras[0].GetComponent<AntiAliasing>().Set();
		
		// Water Effect
		//cameras[1].GetComponent<WaterEffect>().Set(); 
		
		sphSolver.StartSimulation();
		
		StartCoroutine(LiquidAmountCheck());
		StartCoroutine(LiquidColorFinishedCheck());
    }

    // Update is called once per frame
    void Update()
    {
        if (state == GameState.Playing) {

			
		} else if (state == GameState.Retry) {
			if ((Input.touchCount > 0  || Input.GetMouseButton(0)) && uiController.IsRetryAnimationCompleted()) {
				StartCoroutine(ReloadScene(0)); 
			}
		}
		
		if (toDestorySpheres.Count > 0) {
			tmpList.Clear();
			for (int i = 0; i < toDestorySpheres.Count; i++) {
				Sphere sphere = toDestorySpheres[i];
				sphere.transform.localScale = sphere.transform.localScale * 0.9f;
				if (sphere.transform.localScale.x < 0.1) {
					tmpList.Add(sphere);
				}
			}
			for (int i = 0; i < tmpList.Count; i++) {
				Sphere sphere = tmpList[i];
				if (sphere.tank != null) {
					if (sphere.tank.isFilled) {
						LiquidColor? color = sphere.previousColor == null ? sphere.color : sphere.previousColor;
						foreach (SourceTank sourceTank in sourceTanks) {
							if (sourceTank.liquidColor == color) {
								//Sphere newSphere = SpawnSphere(sourceTank.transform.position, sourceTank.liquidColor);
								//sourceTank.spheres.Add(newSphere);
								break;
							}
						}
					}
					
					sphere.tank.OnTriggerExit(sphere.GetComponent<SphereCollider>());
				}
				if (sphere.gameObject.layer == LayerMask.NameToLayer("BobbleSphere")) {
					LiquidColor? color = sphere.previousColor == null ? sphere.color : sphere.previousColor;
					for (int j = 0; j < sourceTanks.Count; j++) {
						SourceTank sourceTank = sourceTanks[j];
						if (sourceTank.liquidColor == color && !sourceTank.IsFull()) {
							bool flag = false;
							for (int k = 0; k < fillingOrders.Count; k++) {
								FillingOrder order = fillingOrders[k];
								if (order.valve.sourceTank == sourceTank) {
									flag = true;
									break;
								}
							}
							if (!flag) {
								Sphere newSphere = SpawnSphere(sourceTank.transform.position, sourceTank.liquidColor);
								sourceTank.spheres.Add(newSphere);
								break;
							}
						}
					}
				} 
				/*
				if (false && sphere.touchedBottomTrigger) {
					LiquidColor? color = sphere.previousColor == null ? sphere.color : sphere.previousColor;
					foreach (SourceTank sourceTank in sourceTanks) {
						if (sourceTank.liquidColor == color && !sourceTank.IsFull()) {
							Sphere newSphere = SpawnSphere(sourceTank.transform.position, sourceTank.liquidColor);
							sourceTank.spheres.Add(newSphere);
						}
					}
				}*/
				
				LiquidColor? color1 = sphere.previousColor == null ? sphere.color : sphere.previousColor;
				sphere.valve.releasedSpheres[(int) color1].Remove(sphere);
				
				toDestorySpheres.Remove(sphere);
				Destroy(sphere.gameObject);
			}
			tmpList.Clear();
		}
    }
	
	private void LoadLevel() {
		tankFillingOrderIndex = new Dictionary<Tank, int>();
		toDestorySpheres = new List<Sphere>();
		tmpList = new List<Sphere>();
		firstDockFlag = false;
		
		HandleGuide();
		
		// In the MainScene this if will be ignored
		if (!isMainScene) {
			float fieldOfView = cameras[0].fieldOfView;
			foreach (Camera camera in cameras) {
				camera.GetComponent<CameraSetting>().Set(camera.fieldOfView);
			}
			if (cameras[0].fieldOfView >= 110) {
				cameras[1].GetComponent<MetaBalls2>().cutoutMaterial.SetFloat("_Cutoff", (3 * cameras[0].fieldOfView / 110 - 2) * 0.125f);
			} else {
				cameras[1].GetComponent<MetaBalls2>().cutoutMaterial.SetFloat("_Cutoff", (3 - 2 * cameras[0].fieldOfView / 110) * 0.125f);
			}
			
			HandleConfettis(fieldOfView);
			return;
		}
		
		// Here we decides which prefab gonna be load
		string levelName = null;
		if (internalLevel <= 5) {
			levelName = "Prefabs/Levels/lvl" + internalLevel + " object";
		} else if (internalLevel >= 6 && internalLevel <= 9) {
			levelName = "Prefabs/Levels/lvl" + (internalLevel + 3) + " object";
		} else if (internalLevel == 10) {
			levelName = "Prefabs/Levels/lvl19 object";
		} else if (internalLevel >= 11 && internalLevel <= 14) {
			levelName = "Prefabs/Levels/lvl" + (internalLevel + 10) + " object";
		}  else if (internalLevel >= 15 && internalLevel <= 16) {
			levelName = "Prefabs/Levels/lvl" + (internalLevel + 11) + " object";
		} else if (internalLevel == 17) {
			levelName = "Prefabs/Levels/lvl6 object";
		}
		Debug.Log("internalLevel: " + internalLevel);
		Debug.Log("levelName: " + levelName);
		
		GameObject levelPrefab = (GameObject) Resources.Load(levelName);
		Debug.Log(levelPrefab);
		//ClearReferences();
		
		GameObject levelObject = Instantiate(levelPrefab);
		
		levelInfo = levelObject.GetComponentInChildren<LevelInfo>();
		fillingOrders = levelInfo.fillingOrders;
		
		foreach (Camera camera in cameras) {
			camera.GetComponent<CameraSetting>().Set(levelInfo.cameraFieldOfView);
		}
		if (cameras[0].fieldOfView >= 110) {
			cameras[1].GetComponent<WaterEffect>().cutoutMaterial.SetFloat("_Cutoff", (3 * cameras[0].fieldOfView / 110 - 2) * 0.125f);
		} else {
			cameras[1].GetComponent<WaterEffect>().cutoutMaterial.SetFloat("_Cutoff", (3 - 2 * cameras[0].fieldOfView / 110) * 0.125f);
		}
		
		HandleConfettis(levelInfo.cameraFieldOfView);
		
		foreach (Tank tank in levelObject.GetComponentsInChildren<Tank>()) {
			tanks.Add(tank);
		}
		foreach (SourceTank tank in levelObject.GetComponentsInChildren<SourceTank>()) {
			sourceTanks.Add(tank);
		}
		foreach (Valve valve in levelObject.GetComponentsInChildren<Valve>()) {
			valves.Add(valve);
		}
		foreach (Hose hose in levelObject.GetComponentsInChildren<Hose>()) {
			hose.transform.parent = obiSolver.transform;
		}
		foreach (Bobble bobble in levelObject.GetComponentsInChildren<Bobble>()) {
			bobbles.Add(bobble);
		}
		foreach (Torch torch in levelObject.GetComponentsInChildren<Torch>()) {
			torches.Add(torch);
		}
		foreach (Fan fan in levelObject.GetComponentsInChildren<Fan>()) {
			fans.Add(fan);
		}
		foreach (Turbine turbine in levelObject.GetComponentsInChildren<Turbine>()) {
			turbines.Add(turbine);
		}
		foreach (ColorZone colorZone in levelObject.GetComponentsInChildren<ColorZone>()) {
			colorZones.Add(colorZone);
		}
		nozzleBoundary = levelObject.GetComponentInChildren<NozzleBoundary>();
		
		PositionBottomTrigger();
	}
	
	private void InitSPHSolver() {
		List<GameObject> sphereGameObjects = new List<GameObject>();
		foreach (SourceTank tank in sourceTanks) {
			foreach (Sphere sphere in tank.spheres) {
				sphereGameObjects.Add(sphere.gameObject);
			}
		}
		sphSolver.InitSPH(sphereGameObjects);
	}
	
	public void OnTankFilled(Tank tank, bool isColorMatched) {
		if (!isColorMatched) {
			Retry();
			Debug.Log("!");
			return;
		}
		
		FillingOrder fillingOrder = fillingOrders[tankFillingOrderIndex[tank]];
		bool isGroupFilled = true;
		foreach (Tank tank1 in fillingOrder.fillingGroups[fillingOrder.currentGroupToFillIndex].tanks) {
			if (!tank1.isFilled) {
				isGroupFilled = false;
				break;
			}
		}
		if (isGroupFilled) {
			Debug.Log("Order " + tankFillingOrderIndex[tank] + ", Group " + (fillingOrder.currentGroupToFillIndex).ToString() + " Filled");
			StartCoroutine(RemoveExtraSpheres(0.1f, fillingOrder));
			
			foreach (Tank tank1 in fillingOrder.fillingGroups[fillingOrder.currentGroupToFillIndex].tanks) {
				tank1.OnFillingGroupCompleted();
			}
			
			foreach (Action action in fillingOrder.fillingGroups[fillingOrder.currentGroupToFillIndex].onFillingActions) {
				action.Do(gameObject);
			}
			fillingOrder.currentGroupToFillIndex++;
			
			bool isOrdersCompleted = true;
			foreach (FillingOrder order1 in fillingOrders) {
				if(order1.currentGroupToFillIndex != order1.fillingGroups.Count) {
					isOrdersCompleted = false;
					break;
				}
			}
			if (isOrdersCompleted) {
				Invoke("Win", 0.25f);
				return;
			}
			
			if (fillingOrder.fillingGroups.Count > fillingOrder.currentGroupToFillIndex) {
				foreach (Tank tank1 in fillingOrder.fillingGroups[fillingOrder.currentGroupToFillIndex].tanks) {
					tank1.toFill = true;
				}
			}
		}
	}
	
	public void OnBobbleFilled(Bobble bobble) {
		FillingOrder fillingOrder = fillingOrders[0];
		bool isGroupFilled = true;
		foreach (Tank tank1 in fillingOrder.fillingGroups[fillingOrder.currentGroupToFillIndex].tanks) {
			if (!tank1.isFilled) {
				isGroupFilled = false;
				break;
			}
		}
		Tank tank = fillingOrder.fillingGroups[fillingOrder.currentGroupToFillIndex].tanks[0];
		if (tank.color == bobble.liquidColor) {
			foreach (Action action in fillingOrder.fillingGroups[fillingOrder.currentGroupToFillIndex].onFillingActions) {
				action.Do(gameObject);
			}
			fillingOrder.currentGroupToFillIndex++;
			
			if (fillingOrder.fillingGroups.Count > fillingOrder.currentGroupToFillIndex) {
				foreach (Tank tank1 in fillingOrder.fillingGroups[fillingOrder.currentGroupToFillIndex].tanks) {
					tank1.toFill = true;
				}
			}
		}
	}
	
	private IEnumerator RemoveExtraSpheres(float delay, FillingOrder order) {
		yield return new WaitForSeconds(delay);
		
		List<Tank> tanks = order.fillingGroups[order.currentGroupToFillIndex - 1].tanks;
		foreach (List<Sphere> spheres in order.valve.releasedSpheres) {
			foreach (Sphere sphere in spheres) {
				if (sphere.tank == null || sphere.tank.color != sphere.color) {
					//sphere.transform.position = fillingGroups[currentGroupToFillIndex - 1].tanks[0].transform.position;
					DestroySphere(sphere);
				}
			}
		}
		/*
		List<Sphere> releasedSpheres = order.valve.releasedSpheres[(int)tanks[0].color];
		foreach (Sphere sphere in releasedSpheres) {
			if (sphere.tank == null || sphere.tank.color != tanks[0].color) {
				//sphere.transform.position = fillingGroups[currentGroupToFillIndex - 1].tanks[0].transform.position;
				DestroySphere(sphere);
			}
		}*/
		
		foreach (Tank tank in tanks) {
			int numOfSpheresInTank = tank.numOfSpheres - tank.numOfUnmatchedSpheres;
			Debug.Log("numOfSpheresInTank " + tank.ToString() + " " + numOfSpheresInTank);
			Vector3 position = tank.GetComponent<BoxCollider>().bounds.center;
			while (numOfSpheresInTank < 16) {
				Sphere sphere = SpawnSphere(position, tank.color);
				//Debug.Log("Spw pos: " + tank.transform.position);
				sphere.valve = order.valve;
				numOfSpheresInTank++;
			}
		}
	}
	
	public Sphere SpawnSphere(Vector3 position, LiquidColor color) {
		Sphere sphere = Instantiate(spherePrefab).GetComponent<Sphere>();
		sphere.targetSize = Random.Range(0.3f, 0.4f);
		float sphereSize = 0.02f;
		sphere.transform.position = position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
		sphere.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
		
		sphere.SetColor(color);
		
		return sphere;
	}
	
	public void DestroySphere(Sphere sphere) {
		if (sphere.toDestroy) return;
		
		sphere.toDestroy = true;
		toDestorySpheres.Add(sphere);
	}
	
	public void SpawnSmoke(Sphere sphere) {
		GameObject smokeObj = Instantiate(smokePrefab);
		smokeObj.transform.position = sphere.transform.transform.position;
		smokeObj.GetComponent<ParticleSystem>().Play();
	}
	
	public void OnValveOpened() {

	}
	
	public IEnumerator LiquidColorFinishedCheck() {
		yield break;
		
		yield return new WaitForSeconds(0.2f);
		
		finishedLiquidColors = new List<LiquidColor>();
		Dictionary<LiquidColor, bool> dict = new Dictionary<LiquidColor, bool>();
		
		while (true) {
			dict[LiquidColor.Green] = false;
			dict[LiquidColor.Blue] = false;
			dict[LiquidColor.Yellow] = false;
			dict[LiquidColor.Red] = false;
			
			foreach (Tank tank in tanks) {
				if (!tank.isFilled) {
					dict[tank.color] = true;
				}
			}
			
			foreach (LiquidColor color in liquidColorsArray) {
				if (!finishedLiquidColors.Contains(color) && !dict[color]) {
					finishedLiquidColors.Add(color);
				}
			}
			
			yield return new WaitForSeconds(0.3f);
		}
		
	}
	
	
	public int GetLiquidLayer(LiquidColor? color) {
		if (color == null) return -1;
		return LayerMask.NameToLayer("Liquid" + (((int)color) + 1).ToString());
	}
	
	
	public void OnDockingListener(Nozzle nozzle, SourceTank sourceTank) {
		Animator animator = sourceTank.nozzle.GetComponent<Animator>();
		animator.Play("Base Layer." + "nozzle_docked");
		nozzle.SetSourceTank(sourceTank);
	}
	
	public void OnDockedListener(Nozzle nozzle, SourceTank sourceTank) {
		foreach (ParticleSystem effect in dockedEffects) {
			effect.transform.position = nozzle.body.transform.position;
			effect.Play();			
		}
		nozzle.valve.SetSourceTank(sourceTank);
		if (!nozzle.isInZone) {
			nozzle.valve.hose.GetComponent<MeshRenderer> ().sharedMaterial = wallColors[(int)sourceTank.liquidColor];
		}
		foreach (FillingOrder order in fillingOrders) {
			if (order.valve == nozzle.valve && order.fillingGroups.Count == order.currentGroupToFillIndex) {
				Retry();
				Debug.Log("2");
			}
		}
		if (!firstDockFlag) {
			firstDockFlag = true;
			if (tankMover != null) {
				tankMover.Hide();
			}
			if (allCandidateFillingOrders.Count != 0) {
				fillingOrders = allCandidateFillingOrders[tankMover.currentPositionIndex].orders;
			}
			tankFillingOrderIndex.Clear();
			int i = 0;
			foreach (FillingOrder order in fillingOrders) {
				order.currentGroupToFillIndex = 0;
				foreach (FillingGroup fillingGroup in order.fillingGroups) {
					foreach (Tank tank in fillingGroup.tanks) {
						tankFillingOrderIndex[tank] = i;
					}
				}
				i++;
			}
			foreach (FillingOrder order in fillingOrders) {
				foreach (Tank tank in order.fillingGroups[0].tanks) {
					tank.toFill = true;
				}
			}
		}
		/*
		Animator animator = sourceTank.nozzle.GetComponent<Animator>();
		animator.Play("Base Layer." + "nozzle_docked");
		*/
	}
	
	public void OnUnDockedListener(Nozzle nozzle, SourceTank sourceTank) {
		nozzle.valve.SetSourceTank(null);
		if (!nozzle.isInZone) {
			nozzle.valve.hose.GetComponent<MeshRenderer> ().sharedMaterial = defaultMaterial;
		}
		Animator animator = sourceTank.nozzle.GetComponent<Animator>();
		animator.Play("Base Layer." + "nozzle_undocked");
	}
	
	public int NumOfUnPoppedBobbles() {
		int num = 0;
		foreach (Bobble bobble in bobbles) {
			if (bobble.state != BobbleState.Popped) {
				num++;
			}
		}
		return num;
	}
	
	public bool IsNozzleTouched() {
		for (int i = 0; i < valves.Count; i++) {
			if (valves[i].nozzle.touched) {
				return true;
			}
		}
		return false;
	}
	
	// We don't use this function for now
	public void OnObjectLoaded(object comp) {
		if (comp is LevelInfo) {
			levelInfo = (LevelInfo) comp;
			fillingOrders = levelInfo.fillingOrders;
		} else if (comp is Tank) {
			tanks.Add((Tank) comp);
		} else if (comp is OutSideBarrier) {
			outSideBarriers.Add(((OutSideBarrier) comp).gameObject);
		} else if (comp is Valve) {
			valves.Add((Valve) comp);
		} else if (comp is SourceTank) {
			sourceTanks.Add((SourceTank) comp);
		} else if (comp is Hose) {
			hoses.Add((Hose) comp);
		} else if (comp is Bobble) {
			bobbles.Add((Bobble) comp);
		} else if (comp is NozzleBoundary) {
			nozzleBoundary = (NozzleBoundary) comp;
		} else if (comp is Pin) {
			pins.Add((Pin) comp);
		} else if (comp is Passage) {
			passages.Add((Passage) comp);
		} else if (comp is Torch) {
			torches.Add((Torch) comp);
		} else if (comp is Fan) {
			fans.Add((Fan) comp);
		} else if (comp is Turbine) {
			turbines.Add((Turbine) comp);
		} else if (comp is Wire) {
			wires.Add((Wire) comp);
		} 
	}
	
	// We don't use this function for now
	public void ClearReferences() {
		levelInfo = null;
		
		tanks.Clear();

		outSideBarriers.Clear();

		valves.Clear();

		hoses.Clear();
		
		foreach (SourceTank tank in sourceTanks) {
			foreach (Sphere sphere in tank.spheres) {
				Destroy(sphere.gameObject);
			}
		}
		sourceTanks.Clear();
		
		bobbles.Clear();

		pins.Clear();

		passages.Clear();
		
		torches.Clear();
		
		fans.Clear();
		
		turbines.Clear();
		
		wires.Clear();
		
		tankMover = null;
	
		nozzleBoundary = null;
	}
	
	// We don't use this function for now
	public void BeforeLoading() {
		if (levelInfo != null) {
			Destroy(levelInfo.gameObject);
			levelInfo = null;
		}
		
		foreach (Sphere sphere in toDestorySpheres) {
			Destroy(sphere.gameObject);
		}
		toDestorySpheres.Clear();
		firstDockFlag = false;
		
		foreach (Tank tank in tanks) {
			Destroy(tank.gameObject);
		}
		tanks.Clear();
		
		foreach (GameObject barrier in outSideBarriers) {
			Destroy(barrier);
		}
		outSideBarriers.Clear();
		
		foreach (Valve valve in valves) {
			Destroy(valve.gameObject);
		}
		valves.Clear();
		
		foreach(Hose hose in hoses) {
			Destroy(hose.gameObject);
		}
		hoses.Clear();
		
		foreach (SourceTank tank in sourceTanks) {
			foreach (Sphere sphere in tank.spheres) {
				Destroy(sphere.gameObject);
			}
			Destroy(tank.gameObject);
		}
		sourceTanks.Clear();
		
		foreach (Bobble bobble in bobbles) {
			Destroy(bobble.gameObject);
		}
		bobbles.Clear();
		
		foreach (Pin pin in pins) {
			Destroy(pin.gameObject);
		}
		pins.Clear();
		
		foreach (Passage passage in passages) {
			Destroy(passage.gameObject);
		}
		passages.Clear();
		
		foreach (Torch torch in torches) {
			Destroy(torch.gameObject);
		}
		torches.Clear();
		
		foreach (Fan fan in fans) {
			Destroy(fan.gameObject);
		}
		fans.Clear();
		
		foreach (Turbine turbine in turbines) {
			Destroy(turbine.gameObject);
		}
		turbines.Clear();
		
		foreach(Wire wire in wires) {
			Destroy(wire.gameObject);
		}
		wires.Clear();
		
		if (tankMover != null) {
			Destroy(tankMover.gameObject);
			tankMover = null;
		}
		
		if (nozzleBoundary != null) {
			Destroy(nozzleBoundary.gameObject);
			nozzleBoundary = null;
		}
	}
	
	// We don't use this function for now
	public void AfterLoading(Dictionary<string, object> settings) {
		foreach (Valve valve in valves) {
			valve.hose.SetValve(valve);
		}
		state = GameState.Playing;
		
		for (int i = 0; i < winEffects.Count; i++) {
			SerializedTransform t = (SerializedTransform) ((List<SerializedTransform>) settings["Win Effects"])[i];
			winEffects[i].gameObject.transform.position = t.position;
			winEffects[i].gameObject.transform.rotation = t.rotation;
			winEffects[i].gameObject.transform.localScale = t.localScale;
			winEffects[i].gameObject.transform.localPosition = t.localPosition;
		}
		
		float fieldOfView = (float) settings["Camera Filed Of View"];
		foreach (Camera camera in cameras) {
			camera.fieldOfView = fieldOfView;
		}
	}
	
	// We don't use this function for now
	public void OnSave(SaveGame saveGame) {
		Dictionary<string, object> settings = new Dictionary<string, object>();
		
		List<SerializedTransform> winEffectsTransforms = new List<SerializedTransform>();
		foreach (ParticleSystem effect in winEffects) {
			winEffectsTransforms.Add(new SerializedTransform(effect.transform));
		}
		settings["Win Effects"] = winEffectsTransforms;
		
		settings["Camera Filed Of View"] = cameras[0].fieldOfView;
		saveGame.settings = settings;
	}
	
	private IEnumerator LiquidAmountCheck() {
		//yield break;
		
		yield return new WaitForSeconds(0.2f);
		
		Dictionary<LiquidColor, int> currentNum = new Dictionary<LiquidColor, int>();
		Dictionary<LiquidColor, int> requiredNum = new Dictionary<LiquidColor, int>();
		
		Dictionary<LiquidColor, bool> hasColorZone = new Dictionary<LiquidColor, bool>();
		for (int i = 0; i < colorZones.Count; i++) {
			hasColorZone[colorZones[i].color] = true;
		}
		
		while (state == GameState.Playing) {
			currentNum[LiquidColor.Green] = 0;
			currentNum[LiquidColor.Blue] = 0;
			currentNum[LiquidColor.Yellow] = 0;
			currentNum[LiquidColor.Red] = 0;
			
			for (int i = 0; i < sourceTanks.Count; i++) {
				SourceTank sourceTank = sourceTanks[i];
				currentNum[sourceTank.liquidColor] += sourceTank.spheres.Count;
			}
			
			//LiquidColor color1 = LiquidColor.Blue;
			//int x = currentNum[color1];
			//Debug.Log(color1 + ": " + currentNum[color1] + " in SourceTank");
			
			for (int i = 0; i < fillingOrders.Count; i++) {
				FillingOrder order = fillingOrders[i];
				for (int j = 0; j < liquidColorsArray.Count; j++) {
					LiquidColor color = liquidColorsArray[j];
					currentNum[color] += order.valve.releasedSpheres[(int) color].Count;
				}
			}
			
			//Debug.Log(color1 + ": " + (currentNum[color1] - x) + " in Valve");
			
			requiredNum[LiquidColor.Green] = 0;
			requiredNum[LiquidColor.Blue] = 0;
			requiredNum[LiquidColor.Yellow] = 0;
			requiredNum[LiquidColor.Red] = 0;
			
			for (int i = 0; i < tanks.Count; i++) {
				Tank tank = tanks[i];
				if (!tank.isFilled) {
					requiredNum[tank.color] += tank.fillingLimit;
				}
			}
			
			
			/*
			foreach (LiquidColor color in liquidColorsArray) {
				Debug.Log(color + ": " + requiredNum[color] + ", " + currentNum[color]);
			}
			*/
			
			for (int i = 0; i < liquidColorsArray.Count; i++) {
				LiquidColor color = liquidColorsArray[i];
				if (requiredNum[color] > currentNum[color] && !hasColorZone.ContainsKey(color)) {
					Retry();
					yield break;
				}
			}
			
			yield return new WaitForSeconds(0.3f);
		}
	}
	
	private void HandleGuide() {
		bool hasGuide = DoesThisLevelHaveGuide();
		Debug.Log("DoesThisLevelHaveGuide: " + hasGuide);
		//Debug.Log(guideHandAnimator.isInitialized);
		if (hasGuide) {
			if (PlayerPrefs.HasKey("Show Guide")) {
				StartCoroutine(ShowGuide(0.5f));
			} else {
				StartCoroutine(ShowGuide(1.5f));
			}
			
		}
		if (guideHandAnimator != null) {
			guideHandAnimator.gameObject.SetActive(false);	
		}
	}
	
	private void HandleConfettis(float fieldOfView) {
		if (Mathf.Abs(fieldOfView - 90) < 0.1f) {
			winEffects[0].transform.localScale = new Vector3(2, 2, 2.2f);
			winEffects[1].transform.localScale = new Vector3(2, 2, 2.2f);
			winEffects[2].transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
			winEffects[3].transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
		} else if (Mathf.Abs(fieldOfView - 100) < 0.1f) {
			winEffects[0].transform.localScale = new Vector3(2.5f, 2.5f, 2.2f);
			winEffects[1].transform.localScale = new Vector3(2.5f, 2.5f, 2.2f);
			winEffects[2].transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
			winEffects[3].transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
		} else {
			winEffects[0].transform.localScale = new Vector3(3, 3, 2.5f);
			winEffects[1].transform.localScale = new Vector3(3, 3, 2.5f);
			winEffects[2].transform.localScale = new Vector3(3, 3, 3);
			winEffects[3].transform.localScale = new Vector3(3, 3, 3);
		}
		int deviceHight = Screen.height;
        int deviceWidth = Screen.width;
        float ratio = deviceHight * 1.0f / deviceWidth;
		
		Vector3 pos = cameras[0].ScreenToWorldPoint (new Vector3 (Screen.width * 0.3f, 0, -cameras[0].transform.position.z - 2));
		pos.y -= 0.2f;
		Debug.Log(pos);

		winEffects[0].transform.position = pos;
		Vector3 tmp = winEffects[0].transform.localScale;
		tmp.z += Mathf.Max(ratio - 1.77f, 0) * 1;
		winEffects[0].transform.localScale = tmp;
		
		tmp = winEffects[1].transform.position;
		tmp.y = pos.y;
		tmp.x = -pos.x;
		tmp.z = pos.z;
		winEffects[1].transform.position = tmp;
		tmp = winEffects[1].transform.localScale;
		tmp.z += Mathf.Max(ratio - 1.77f, 0) * 1;
		winEffects[1].transform.localScale = tmp;
		
		pos = cameras[0].ScreenToWorldPoint (new Vector3 (Screen.width * 0.3f, Screen.height * 0.7f, -cameras[0].transform.position.z - 2));
		
		winEffects[2].transform.position = pos;
		
		tmp = winEffects[3].transform.position;
		tmp.y = pos.y;
		tmp.x = -pos.x;
		tmp.z = pos.z;
		winEffects[3].transform.position = tmp;
	}
	
	private void PositionBottomTrigger() {
		Vector3 pos = cameras[0].ScreenToWorldPoint (new Vector3 (Screen.width / 2, 0, -cameras[0].transform.position.z));
		pos.y -= 2f;
		pos.y -= bottomTrigger.GetComponent<BoxCollider>().bounds.size.y / 2;
		bottomTrigger.transform.position = pos;
	}
	
	private IEnumerator ShowGuide(float delay) {
		yield return new WaitForSeconds(delay);
		
		guideHandAnimator.gameObject.SetActive(true);
		
		//yield return null;
		int lvl = level;
		if (level == 9) {
			lvl = 12;
		} else if (level == 12) {
			lvl = 22;
		} else if (level != 1) {
			lvl = -1;
		}
		guideHandAnimator.Play("lvl" + lvl);
	}
	
	private bool DoesThisLevelHaveGuide() {
		int lvl = level;
		if (level == 9) {
			lvl = 12;
		} else if (level == 12) {
			lvl = 22;
		} else if (level != 1) {
			lvl = -1;
		}
		return guideHandAnimator != null 
			&& guideHandAnimator.HasState(guideHandAnimator.GetLayerIndex("Base Layer"), Animator.StringToHash("Base Layer.lvl" + lvl));
	}
	
	public void OnGuideHandAnimationCompleted() {
		guideHandAnimator.gameObject.SetActive(false);
	}
	
	// Will be called if player completes the level
	public void Win() {
		state = GameState.Winner;
		
		uiController.ShowWin();
		foreach (ParticleSystem ps in winEffects) {
			ps.Play();
		}
		
		PlayerPrefs.SetInt("level", level + 1);
		PlayerPrefs.DeleteKey("Show Guide");
		PlayerPrefs.Save();
		
		StartCoroutine(ReloadScene(2.5f));
	}
	
	// Will be called if player failes
	public void Retry() {
		if (state == GameState.Retry) return;
		
		if (DoesThisLevelHaveGuide()) {
			PlayerPrefs.SetString("Show Guide", "");
			PlayerPrefs.Save();
		}
		
		StartCoroutine(ShowRetry(1f));
	}
	
	// Will be called when top left button (retry button) pressed. This will load the level again
	public void ReloadLevel() {
		if(state == GameState.Retry) return;
		
		if (DoesThisLevelHaveGuide()) {
			PlayerPrefs.SetString("Show Guide", "");
			PlayerPrefs.Save();
		}
		
		Scene scene = SceneManager.GetActiveScene(); 
		SceneManager.LoadScene(scene.name);
	}
	
	// Shows the "tap to retry" screen
	private IEnumerator ShowRetry(float delay) {
		yield return new WaitForSeconds(delay);
		
		if (state == GameState.Retry) yield break;
		
		state = GameState.Retry;
		uiController.ShowRetry();
	}
	
	
	// Reload the scene
	private IEnumerator ReloadScene(float delay) {
		yield return new WaitForSeconds(delay);
		
		if (true || state == GameState.Retry || isMainScene) {
			Scene scene = SceneManager.GetActiveScene(); 
			SceneManager.LoadScene(scene.name);
		} else {
			SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % 3);
		}
		
		
		/*
		Scene scene = SceneManager.GetActiveScene(); 
		SceneManager.LoadScene(scene.name);
		*/
	}
}
