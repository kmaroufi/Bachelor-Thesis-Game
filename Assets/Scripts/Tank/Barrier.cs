using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DentedPixel;

public class Barrier : MonoBehaviour
{
	public float finalY;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void Move() {
		LeanTween.moveY(gameObject, finalY, 0.3f);
	}
}
