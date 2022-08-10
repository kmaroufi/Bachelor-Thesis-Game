using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionLimit : MonoBehaviour
{
	[SerializeField] public Transform leftDown;
	[SerializeField] public Transform rightTop;
	
	private Rigidbody rb;
	
	 private float minX = 0;
	 private float maxX = 1;
	 private float minY = 0;
	 private float maxY = 1;
	 private float minZ = 0;
	 private float maxZ = 1;
    // Start is called before the first frame update
    void Start()
    {
		Debug.Log("PositionLimit Start");
		rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {	
		if (leftDown == null || rightTop == null) {
			Debug.Log("position limit null");
			return;
		}
		
		rb.position = new Vector3(Mathf.Clamp(gameObject.transform.position.x,leftDown.position.x,rightTop.position.x),
									  Mathf.Clamp(gameObject.transform.position.y,leftDown.position.y,rightTop.position.y),
									  Mathf.Clamp(gameObject.transform.position.z,leftDown.position.z,rightTop.position.z));
    }
}
