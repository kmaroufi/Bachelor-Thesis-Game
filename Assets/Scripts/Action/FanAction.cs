﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kmaroufi;

public class FanAction : Action
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public override void Do(GameObject caller) {
		GetComponent<Fan>().TurnOn();
	}
}
