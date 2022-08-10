using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideHand : MonoBehaviour
{
	private Animator animator;
	private int num;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
		num = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void OnGuideHandAnimationCompleted() {
		num++;
		if (num == 2) {
			GameManager.Instance.OnGuideHandAnimationCompleted();
		}
	}
}
