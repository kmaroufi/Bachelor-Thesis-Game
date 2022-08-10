using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceTankExternalTrigger : MonoBehaviour
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
	
	private void OnTriggerEnter(Collider collider) {
		if (collider.gameObject.layer == LayerMask.NameToLayer("Nozzle") && collider.gameObject.tag == "NozzleTouchObject") {
			//Debug.Log("enter");
			Nozzle nozzle = collider.transform.parent.gameObject.GetComponent<Nozzle>();
			nozzle.OnTouchObjEnteredSourceTankExternalTrigger(transform.parent.GetComponent<SourceTank>());
		}
	}
	
	private void OnTriggerExit(Collider collider) {
		if (collider.gameObject.layer == LayerMask.NameToLayer("Nozzle") && collider.gameObject.tag == "NozzleTouchObject") {
			//Debug.Log("exit");
			Nozzle nozzle = collider.transform.parent.gameObject.GetComponent<Nozzle>();
			nozzle.OnTouchObjExitedSourceTankExternalTrigger(transform.parent.GetComponent<SourceTank>());
		}
	}
}
