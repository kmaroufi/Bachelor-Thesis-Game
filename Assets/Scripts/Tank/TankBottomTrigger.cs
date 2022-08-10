using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankBottomTrigger : MonoBehaviour
{
	public Tank tank;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnTriggerEnter(Collider collider) {
		tank.OnBottomTriggerEnter(collider.gameObject);
	}
}
