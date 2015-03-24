using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AdjustPercentage : Photon.MonoBehaviour {

	private int playerHealth;
	private bool hasValue;
	private Text myText;
	// Use this for initialization
	void Start ()
	{
		playerHealth = 100;
		hasValue = false;
		myText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(photonView.isMine && !hasValue)
		{
			if(gameObject.GetPhotonView ().owner.customProperties ["Health"] != null)
			{
				hasValue = true;
				playerHealth = (int)gameObject.GetPhotonView ().owner.customProperties ["Health"];
				myText.text = playerHealth.ToString ();
			}
		}
	}
	
	void AdjustPercent(int health)
	{
		//playerHealth = (int)gameObject.GetPhotonView ().owner.customProperties ["Health"];
		myText.text = health.ToString ();
	}
}
