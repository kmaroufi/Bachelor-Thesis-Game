using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetting : MonoBehaviour
{
	private Camera camera;
	
	void Awake() {
		camera = GetComponent<Camera>();
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void Set(float fieldOfView) {
		int deviceHight = Screen.height;
        int deviceWidth = Screen.width;

        float ratio = deviceHight * 1.0f / deviceWidth;

		
        if(ratio >= 2.1f) {
			
			fieldOfView += fieldOfView < 90.1f ? 10 : 5;
			//fieldOfView += fieldOfView < 100.1f ? 10 : 10;
            //fieldOfView = fieldOfView + 10 * 90.0f / fieldOfView;
        } else if(ratio < 2.1f && ratio >= 1.7f) {
            fieldOfView = fieldOfView + Mathf.Max(0, ratio - 1.7f) * 12.5f;
        } else if(ratio < 1.7f) {
            fieldOfView = fieldOfView + Mathf.Max(0, 1.7f - ratio) * 15;
        }
		
		camera.fieldOfView = fieldOfView;
	}
}
