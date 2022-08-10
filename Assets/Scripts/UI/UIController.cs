using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
	[SerializeField] public GameObject retry;
	[SerializeField] public GameObject retryHalo;
	[SerializeField] public GameObject retryVisual;
	
	[SerializeField] public GameObject retryButton;
	
	[SerializeField] public GameObject level;
	[SerializeField] public TMP_Text levelText;
	
	[SerializeField] public GameObject levelCompleted;
	[SerializeField] public GameObject levelCompletedImage;

	
	private GameManager gm;
    // Start is called before the first frame update
    void Start()
    {
		
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void Set() {
		gm = GameManager.Instance;
		retryButton.SetActive(true);
		retry.SetActive(false);
		level.SetActive(true);
		levelText.text = "Level " + gm.level.ToString();
		//levelText.text = "Level 5";
		levelCompleted.SetActive(false);
	}
	
	public void ShowWin() {
		levelCompleted.SetActive(true);
		levelCompletedImage.GetComponent<Animator>().Play("show");
	}
	
	public void ShowRetry() {
		retryButton.SetActive(false);
		retry.SetActive(true);
		retryVisual.GetComponent<Animator>().Play("show");
	}
	
	public bool IsRetryAnimationCompleted() {
		return retryVisual.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f;
	}
}
