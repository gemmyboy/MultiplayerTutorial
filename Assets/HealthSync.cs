using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthSync : Photon.MonoBehaviour {
	// Use this for initialization
    //void Start () {
    //    if (photonView.isMine)
    //    {
    //        this.enabled = false;//Only enable inter/extrapol for remote players
    //    }
    //    else
    //    {
    //        health = (int)PhotonNetwork.player.customProperties["Health"];
    //        uiManager = FindObjectOfType<UIManager>();
    //    }
    //}

    //void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    // Always send transform (depending on reliability of the network view)
    //    if (stream.isWriting)
    //    {
    //        stream.SendNext(health);
    //        Debug.Log(info.sender.allProperties);
    //    }
    //    // When receiving, buffer the information
    //    else
    //    {
    //        health = (int)stream.ReceiveNext();
    //    }
    //}

    //void Update()
    //{
    //    int oldHealth = (int)PhotonNetwork.player.customProperties["Health"];
    //    if(oldHealth != health){
    //        Debug.Log("Yeah");
    //        uiManager.ChangeHealth(oldHealth);
    //    }
    //}

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
		if(uiManagerStillNull && photonView.isMine)
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
		////////////////////////////
		//I ADDED THIS JACOB ---Adam
		////////////////////////////
		//if(!dead && photonView.isMine)
		//{

		//}
		////////////////////////////
		////////////////////////////

        //ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.player.allProperties;
        //Debug.Log(PhotonNetwork.player.allProperties);
    }

	////////////////////////////
	//I ADDED THIS JACOB ---Adam
	////////////////////////////
	void OnCollisionEnter(Collision col)
	{
		if (photonView.isMine) 
		{
//			theOwner = gameObject.GetPhotonView().owner;
//			health = (int)theOwner.customProperties ["Health"];
//			uiManager.ChangeHealth(health);
//			string blah = "HEALTH: " + health.ToString();
//			Debug.Log (blah);

			if((health >= 0) && (!dead) && (col.gameObject.tag == "TankShell"))
			{
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
						photonView.RPC("AdjustPercent",PhotonTargets.All,health);
						//uiManager.ChangeHealth(health);
						photonView.RPC ("AdjustHealthBar",PhotonTargets.All,1);
						photonView.RPC("ReduceMyHealth",PhotonTargets.All,gameObject.GetPhotonView().ownerId,1,col.gameObject.GetPhotonView().ownerId);
					}else if((health-(int)TankShellDamage) > 0)
					{
						health -= (int)TankShellDamage;
						//gameObject.SendMessage("AdjustPercent",health,SendMessageOptions.RequireReceiver);
						photonView.RPC("AdjustPercent",PhotonTargets.All,health);
						uiManager.ChangeHealth(health);
						photonView.RPC ("AdjustHealthBar",PhotonTargets.All,2);

						//RectTransform[] healthBar;
						//healthBar = gameObject.GetComponentsInParent<RectTransform>();
//						foreach(RectTransform myRect in healthBar)
//						{
//							if(myRect.tag == "GreenHealthBar")
//							{
//								Debug.Log ("GreenHealthBar2");
//								Vector2 tempVectTwoTwo = new Vector2(((TankShellDamage/100.0f)*7.1f),0.0f);
//								myRect.offsetMin += tempVectTwoTwo;//((TankShellDamage/100.0f)*7.01f);
//								break;
//							}
//						}
						photonView.RPC("ReduceMyHealth",PhotonTargets.All,gameObject.GetPhotonView().ownerId,2,col.gameObject.GetPhotonView().ownerId);

					}
				}
			}
		}
	}


	[RPC]
	void ReduceMyHealth(int myViewID,int theCase,int theKiller)
	{
		if((photonView.isMine) && (theCase == 1) && (photonView.ownerId == myViewID) && (!dead))
		{
			hurtPlayer = gameObject.GetPhotonView().owner;

			ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
			uiManager.ChangeHealth(health);
			hash.Add("Health",health);

			dead = true;
			transform.rigidbody.AddExplosionForce(150000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);

			hurtPlayer.SetCustomProperties(hash);

			//Debug.Log("SHOULD HAVE EXPLODED");

		}else if((photonView.isMine) && (theCase == 2) && (photonView.ownerId == myViewID)){

			hurtPlayer = gameObject.GetPhotonView().owner;
			//hurtPlayer.customProperties ["Health"] = health;
			ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
			hash.Add("Health",health);
			transform.rigidbody.AddExplosionForce(15000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);
			hurtPlayer.SetCustomProperties(hash);

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
	void AdjustPercent(int health)
	{
		myText.text = health.ToString ();
	}

	[RPC]
	void AdjustHealthBar(int bounds)
	{
		if(bounds == 1)
		{
			if(myRect.tag == "GreenHealthBar")
			{
				Debug.Log ("GreenHealthBar1");
				//Vector3[] corners = new Vector3();
				//myRect.offsetMin.x += ((TankShellDamage/100.0f)*7.0f);
				Debug.Log (myRect.offsetMin.x);
				Debug.Log (myRect.offsetMax.x);
				Vector2 tempVectTwo = new Vector2(7.01f,0.0f);
				myRect.offsetMin = tempVectTwo;
			}
		}else if(bounds == 2)
		{
			if(myRect.tag == "GreenHealthBar")
			{
				Debug.Log ("GreenHealthBar2");
				Vector2 tempVectTwoTwo = new Vector2(((TankShellDamage/100.0f)*7.1f),0.0f);
				myRect.offsetMin += tempVectTwoTwo;//((TankShellDamage/100.0f)*7.01f);
			}
		}
	}
}
