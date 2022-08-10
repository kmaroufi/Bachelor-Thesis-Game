using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomTrigger : MonoBehaviour
{
	private GameManager gm;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnCollisionEnter(Collision collision) {
		Sphere sphere = collision.collider.gameObject.GetComponent<Sphere>();
		if (sphere == null) return;
		 
		sphere.touchedBottomTrigger = true;
		gm.DestroySphere(sphere);
	}
}
