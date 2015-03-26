using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthSync : Photon.MonoBehaviour {

	////////////////////////////
	//I ADDED THIS JACOB ---Adam
	////////////////////////////
	private int health;
	public UIManager uiManager;
	private UIManager testForManager;
	private bool dead;
	private bool uiManagerStillNull;
	public float TankShellDamage;
	private PhotonPlayer hurtPlayer;
	private Transform hurtPlayersTransform;
	private PhotonPlayer theOwner;
	private Text myText;
	private RectTransform myRect;
	public GameObject myTextObj;
	public GameObject myRectObj;
	void Start()
	{
		dead = false;
		uiManagerStillNull = true;
		uiManager = null;
		testForManager = null;
		hurtPlayer = null;
		hurtPlayersTransform = null;
		theOwner = null;
		//TankShellDamage = 30.0f;
		//health = 100;
		myText = myTextObj.GetComponent<Text> ();
		myRect = myRectObj.GetComponent<RectTransform> ();

		hurtPlayer = gameObject.GetPhotonView().owner;
		ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
		hash.Add("Health",100);
		hurtPlayer.SetCustomProperties(hash);
		health = (int)gameObject.GetPhotonView ().owner.customProperties ["Health"];
	}
	////////////////////////////
	////////////////////////////
    void Update()
    {
		if(uiManagerStillNull && photonView.isOwnerActive && (photonView.ownerId == gameObject.GetPhotonView().ownerId))
		{
			testForManager = FindObjectOfType<UIManager>();
			if(testForManager != null)
			{
				uiManagerStillNull = false;
				uiManager = testForManager;
				//Debug.Log("UIMANAGER WAS SET");
				uiManager.ChangeHealth(health);
			}
		}
    }

	////////////////////////////
	//I ADDED THIS JACOB ---Adam
	////////////////////////////
	void OnCollisionEnter(Collision col)
	{
		if (photonView.isOwnerActive && (photonView.ownerId == gameObject.GetPhotonView().ownerId)) 
		{
//			theOwner = gameObject.GetPhotonView().owner;
//			health = (int)theOwner.customProperties ["Health"];
//			uiManager.ChangeHealth(health);
//			string blah = "HEALTH: " + health.ToString();
//			Debug.Log (blah);

			if((health >= 0) && (!dead) && (col.gameObject.tag == "TankShell"))
			{
				Debug.Log("DaMaGe****SHOULD****HaPpEn");
				if(col.gameObject.GetPhotonView().owner.customProperties["Team"] != gameObject.GetPhotonView().owner.customProperties["Team"])
				{
					theOwner = gameObject.GetPhotonView().owner;
					health = (int)theOwner.customProperties ["Health"];
					uiManager.ChangeHealth(health);
					string blah = "HEALTH: " + health.ToString();
					Debug.Log (blah);
					Debug.Log("SuCcEsSfUl ShOt**********");
						//photonView.RPC("MakeBulletExplode",PhotonTargets.All,col.gameObject);
					if((health-(int)TankShellDamage) <= 0)
					{
						health = 0;
						//gameObject.SendMessage("AdjustPercent",health,SendMessageOptions.RequireReceiver);
						if(photonView.isMine)
							uiManager.ChangeHealth(health);
						photonView.RPC("AdjustPercent",PhotonTargets.All,gameObject.GetPhotonView().ownerId,health);
						photonView.RPC ("AdjustHealthBar",PhotonTargets.All,gameObject.GetPhotonView().ownerId,1);
						photonView.RPC("ReduceMyHealth",PhotonTargets.All,gameObject.GetPhotonView().ownerId,1,col.gameObject.GetPhotonView().ownerId);
					}else if((health-(int)TankShellDamage) > 0)
					{
						health -= (int)TankShellDamage;
						//gameObject.SendMessage("AdjustPercent",health,SendMessageOptions.RequireReceiver);
						if(photonView.isMine)
							uiManager.ChangeHealth(health);
						photonView.RPC("AdjustPercent",PhotonTargets.All,gameObject.GetPhotonView().ownerId,health);
						photonView.RPC ("AdjustHealthBar",PhotonTargets.All,gameObject.GetPhotonView().ownerId,2);
						photonView.RPC("ReduceMyHealth",PhotonTargets.All,gameObject.GetPhotonView().ownerId,2,col.gameObject.GetPhotonView().ownerId);

					}
				}
			}
		}
	}


	[RPC]
	void ReduceMyHealth(int myViewID,int theCase,int theKiller)
	{
		//if((photonView.isMine) && (theCase == 1) && (photonView.ownerId == myViewID) && (!dead))
		if(theCase == 1 && (photonView.ownerId == myViewID))
		{
			hurtPlayer = gameObject.GetPhotonView().owner;

			ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
			//uiManager.ChangeHealth(health);
			hash.Add("Health",health);

			dead = true;
			transform.rigidbody.AddExplosionForce(150000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);

			hurtPlayer.SetCustomProperties(hash);

			//Debug.Log("SHOULD HAVE EXPLODED");

		}else if(theCase == 2 && (photonView.ownerId == myViewID)){//if((photonView.isMine) && (theCase == 2) && (photonView.ownerId == myViewID)){

			hurtPlayer = gameObject.GetPhotonView().owner;
			//hurtPlayer.customProperties ["Health"] = health;
			ExitGames.Client.Photon.Hashtable hash2 = new ExitGames.Client.Photon.Hashtable();
			//uiManager.ChangeHealth(health);
			hash2.Add("Health",health);
			transform.rigidbody.AddExplosionForce(15000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);
			hurtPlayer.SetCustomProperties(hash2);

		}else if(true)//if photonView == theKiller && theCase == 1
		{

		}
	}
	/*
	//[RPC]
	void MakeBulletExplode(GameObject bullet)
	{
		bullet.SendMessage ("Explosion", SendMessageOptions.RequireReceiver);
	}
	*/
	////////////////////////////
	////////////////////////////
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
				//Debug.Log ("GreenHealthBar1");
				//Vector3[] corners = new Vector3();
				//myRect.offsetMin.x += ((TankShellDamage/100.0f)*7.0f);
				//Debug.Log (myRect.offsetMin.x);
				//Debug.Log (myRect.offsetMax.x);
				Vector2 tempVectTwo = new Vector2(7.01f,0.0f);
				myRect.offsetMin = tempVectTwo;
			}
		}else if(bounds == 2 && (photonView.ownerId == myViewID))
		{
			if(myRect.tag == "GreenHealthBar")
			{
				//Debug.Log ("GreenHealthBar2");
				Vector2 tempVectTwoTwo = new Vector2(((TankShellDamage/100.0f)*7.1f),0.0f);
				myRect.offsetMin += tempVectTwoTwo;//((TankShellDamage/100.0f)*7.01f);
			}
		}
	}
}
