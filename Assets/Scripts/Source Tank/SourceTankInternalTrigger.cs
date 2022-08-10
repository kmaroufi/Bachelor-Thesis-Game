using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceTankInternalTrigger : MonoBehaviour
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
		if (collider.gameObject.layer == LayerMask.NameToLayer("Nozzle") && collider.gameObject.tag == "NozzleTrigger" 
			&& !collider.transform.parent.GetComponent<Rigidbody>().isKinematic) {
			gm.OnDockingListener(collider.transform.parent.gameObject.GetComponent<Nozzle>(), transform.parent.GetComponent<SourceTank>());
		}
	}
}
