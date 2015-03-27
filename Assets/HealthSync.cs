using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthSync : Photon.MonoBehaviour {

	public UIManager uiManager;
	private UIManager testForManager;
	private bool dead;
	private bool uiManagerStillNull;
	public float TankShellDamage;
	private PhotonPlayer hurtPlayer;
	private Text myText;
	private RectTransform myRect;
	public GameObject myTextObj;
	public GameObject myRectObj;
	public static int healthAmount;
	private GameObject theBullet;
	void Start()
	{
		dead = false;
		uiManagerStillNull = true;
		uiManager = null;
		testForManager = null;
		hurtPlayer = null;
		myText = myTextObj.GetComponent<Text> ();
		myRect = myRectObj.GetComponent<RectTransform> ();

		hurtPlayer = gameObject.GetPhotonView().owner;
		ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
		hash.Add("Health",100);
		hurtPlayer.SetCustomProperties(hash);
		healthAmount = (int)gameObject.GetPhotonView ().owner.customProperties ["Health"];
	}

    void Update()
    {
		if(uiManagerStillNull && photonView.isOwnerActive && (photonView.ownerId == gameObject.GetPhotonView().ownerId))
		{
			testForManager = FindObjectOfType<UIManager>();
			if(testForManager != null)
			{
				uiManagerStillNull = false;
				uiManager = testForManager;
				uiManager.ChangeHealth(healthAmount);
			}
		}
    }

	void OnCollisionEnter(Collision col)
	{
		if (photonView.isOwnerActive && (photonView.ownerId == gameObject.GetPhotonView().ownerId)) 
		{
			healthAmount = (int)gameObject.GetPhotonView().owner.customProperties["Health"];

			if((healthAmount >= 0) && (!dead) && (col.gameObject.tag == "TankShell"))
			{
				theBullet = new GameObject();
				theBullet = col.gameObject;
				if(theBullet.GetPhotonView().owner.customProperties["Team"].ToString() != gameObject.GetPhotonView().owner.customProperties["Team"].ToString())
				{
					if(((healthAmount-(int)TankShellDamage)) <= 0 && !dead)
					{
						dead = true;
						if(!photonView.isMine){}
						photonView.RPC("ReduceMyHealth",PhotonTargets.Others,gameObject.GetPhotonView().ownerId,1,theBullet.GetPhotonView().ownerId,dead);
					}else if((healthAmount-(int)TankShellDamage) > 0)
					{
						photonView.RPC("ReduceMyHealth",PhotonTargets.Others,gameObject.GetPhotonView().ownerId,2,theBullet.GetPhotonView().ownerId,dead);
					}
				}
			}
		}
	}


	[RPC]
	void ReduceMyHealth(int myViewID,int theCase,int theKiller,bool isDead)
	{
		if(theCase == 1 && (photonView.ownerId == myViewID) && isDead)
		{
			hurtPlayer = gameObject.GetPhotonView().owner;

			ExitGames.Client.Photon.Hashtable hash3 = new ExitGames.Client.Photon.Hashtable();
			hash3.Add("Health",0);
			hurtPlayer.SetCustomProperties(hash3);
			healthAmount = (int)hurtPlayer.customProperties["Health"];

			transform.rigidbody.AddExplosionForce(150000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);
			photonView.RPC ("AdjustHealthBar",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,1);
			photonView.RPC("AdjustPercent",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,healthAmount);

		}else if(theCase == 2 && (photonView.ownerId == myViewID) && !isDead){

			hurtPlayer = gameObject.GetPhotonView().owner;
			ExitGames.Client.Photon.Hashtable hash2 = new ExitGames.Client.Photon.Hashtable();

			healthAmount = (int)hurtPlayer.customProperties["Health"] - (int)TankShellDamage;
			hash2.Add("Health",healthAmount);
			hurtPlayer.SetCustomProperties(hash2);

			//string blah = "HEALTH: " + healthAmount.ToString();
			//Debug.Log (blah);

			transform.rigidbody.AddExplosionForce(15000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);
			
			photonView.RPC("AdjustPercent",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,healthAmount);
			photonView.RPC ("AdjustHealthBar",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,2);

		}else if(true)//if photonView == theKiller && theCase == 1
		{

		}
	}

	[RPC]
	void AdjustPercent(int myViewID, int health)
	{
		if(photonView.ownerId == myViewID)
			myText.text = health.ToString ();
	}

	[RPC]
	void AdjustHealthBar(int myViewID, int bounds)
	{
		if(bounds == 1 && (photonView.ownerId == myViewID))
		{
			if(myRect.tag == "GreenHealthBar")
			{
				Vector2 tempVectTwo = new Vector2(7.01f,0.0f);
				myRect.offsetMin = tempVectTwo;
			}
		}else if(bounds == 2 && (photonView.ownerId == myViewID))
		{
			if(myRect.tag == "GreenHealthBar")
			{
				Vector2 tempVectTwoTwo = new Vector2(((TankShellDamage/100.0f)*7.1f),0.0f);
				myRect.offsetMin += tempVectTwoTwo;
			}
		}
	}
}
