using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kmaroufi;

public class TankAction : Action
{
	private Tank tank;
	private GameManager gm;
    // Start is called before the first frame update
    void Start()
    {
        tank = GetComponent<Tank>();
		gm = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public override void Do(GameObject caller) {
		tank.nearTorch = false;
	}
}
