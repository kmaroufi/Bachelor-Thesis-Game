using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// ~  Fps counter for unity  ~
/// Brief : Calculate the FPS and display it on the screen 
/// HowTo : Create empty object at initial scene and attach this script!!!
/// </summary>
public class FPSCalculator : MonoBehaviour
{
    // for ui.
    private int screenLongSide;
    private Rect boxRect;
    private GUIStyle style = new GUIStyle();

    // for fps calculation.
    public int frameCount;
    public float elapsedTime;
    public double frameRate;
	public double minFrameRate;
	public double maxFrameRate;
	
	private bool stop = false;
	private bool started = false;
	private double currentFrameRate;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Awake()
    {
		minFrameRate = 1000;
		maxFrameRate = 0;
        DontDestroyOnLoad(gameObject);
        UpdateUISize();
    }
	
	private void Start() {
		StartCounting();
	}

    /// <summary>
    /// Monitor changes in resolution and calcurate FPS
    /// </summary>
    private void Update()
    {
		if (!started || stop) return;
		
        // FPS calculation
        frameCount++;
        elapsedTime += Time.deltaTime;
        frameRate = System.Math.Round(frameCount / elapsedTime, 1, System.MidpointRounding.AwayFromZero);
		currentFrameRate = System.Math.Round(1 / Time.deltaTime, 1, System.MidpointRounding.AwayFromZero);
		maxFrameRate = math.max(maxFrameRate, currentFrameRate);
		minFrameRate = math.min(minFrameRate, currentFrameRate);
		
		// Update the UI size if the resolution has changed
		if (screenLongSide != Mathf.Max(Screen.width, Screen.height))
		{
			UpdateUISize();
		}
    }
	
	public void Reset() {
		frameCount = 0;
		elapsedTime = 0;
	}
	
	public void StartCounting() {
		started = true;
	}
	
	public void Stop() {
		stop = true;
	}

    /// <summary>
    /// Resize the UI according to the screen resolution
    /// </summary>
    private void UpdateUISize()
    {
        screenLongSide = Mathf.Max(Screen.width, Screen.height);
        var rectLongSide = screenLongSide / 10;
        boxRect = new Rect(1, 1, rectLongSide * 1.7f, rectLongSide / 3);
        style.fontSize = (int)(screenLongSide / 36.8);
        style.normal.textColor = Color.white;
		//style.alignment = TextAnchor.MiddleCenter;
    }

    /// <summary>
    /// Display FPS
    /// </summary>
    private void OnGUI()
    {
        GUI.Box(boxRect, "");
        GUI.Label(boxRect, " avg fps: " + frameRate, style);
        //GUI.Label(boxRect, " avg fps: " + frameRate + "\n min fps: " + minFrameRate + "\n max fps: " + maxFrameRate, style);
    }
}