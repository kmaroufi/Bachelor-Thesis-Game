using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
	private ColorZone zone;
	public bool isStayingOnTrigger;
    // Start is called before the first frame update
    void Start()
    {
        zone = transform.parent.GetComponent<ColorZone>();
		isStayingOnTrigger = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnTriggerEnter(Collider collider) {
		if (collider.gameObject.layer == LayerMask.NameToLayer("Nozzle") && collider.gameObject.tag == "NozzleTrigger") {
			isStayingOnTrigger = true;
			zone.OnZoneTriggerEnter(this, collider.gameObject.transform.parent.transform.GetComponent<Nozzle>());
		}
	}
	
	void OnTriggerExit(Collider collider) {
		if (collider.gameObject.layer == LayerMask.NameToLayer("Nozzle") && collider.gameObject.tag == "NozzleTrigger") {
			isStayingOnTrigger = false;
			zone.OnZoneTriggerExit(this, collider.gameObject.transform.parent.transform.GetComponent<Nozzle>());
		} else if (collider.gameObject.GetComponent<Sphere>() != null) {
			zone.OnZoneTriggerExit(this, collider.gameObject.GetComponent<Sphere>());
		}
	}
}
