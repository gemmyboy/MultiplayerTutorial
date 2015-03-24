using UnityEngine;
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
	void Start()
	{
		dead = false;
		uiManagerStillNull = true;
		uiManager = null;
		testForManager = null;
		hurtPlayer = null;
		hurtPlayersTransform = null;
		theOwner = null;
		TankShellDamage = 30.0f;
		//health = 100;

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
				Debug.Log("UIMANAGER WAS SET");
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
			theOwner = gameObject.GetPhotonView().owner;
			health = (int)theOwner.customProperties ["Health"];
			uiManager.ChangeHealth(health);
			string blah = "HEALTH: " + health.ToString();
			Debug.Log (blah);

			if((health >= 0) && (!dead) && (col.gameObject.tag == "TankShell"))
			{
				if(col.gameObject.GetPhotonView().owner.customProperties["Team"] != this.gameObject.GetPhotonView().owner.customProperties["Team"])
				{
					Debug.Log("SuCcEsSfUl ShOt**********");
						//photonView.RPC("MakeBulletExplode",PhotonTargets.All,col.gameObject);
					if((health-TankShellDamage) <= 0)
					{
						health = 0;
						uiManager.ChangeHealth(health);
						RectTransform[] healthBar;
						healthBar = gameObject.GetComponentsInParent<RectTransform>();
						foreach(RectTransform myRect in healthBar)
						{
							if(myRect.tag == "GreenHealthBar")
							{
								//Vector3[] corners = new Vector3();
								//myRect.offsetMin.x += ((TankShellDamage/100.0f)*7.0f);
								Debug.Log (myRect.offsetMin.x);
								Debug.Log (myRect.offsetMax.x);
								Vector2 tempVectTwo = new Vector2(7.01f,0.0f);
								myRect.offsetMin = tempVectTwo;
								break;
							}
						}
						dead = true;
						photonView.RPC("ReduceMyHealth",PhotonTargets.All,gameObject.GetPhotonView().ownerId,1);
					}else if((health-TankShellDamage) > 0)
					{
						health -= (int)TankShellDamage;
						uiManager.ChangeHealth(health);
						RectTransform[] healthBar;
						healthBar = gameObject.GetComponentsInParent<RectTransform>();
						foreach(RectTransform myRect in healthBar)
						{
							if(myRect.tag == "GreenHealthBar")
							{
								Vector2 tempVectTwoTwo = new Vector2(((TankShellDamage/100.0f)*7.1f),0.0f);
								myRect.offsetMin += tempVectTwoTwo;//((TankShellDamage/100.0f)*7.01f);
								break;
							}
						}
						photonView.RPC("ReduceMyHealth",PhotonTargets.All,gameObject.GetPhotonView().ownerId,2);
					}
				}
			}
		}
	}


	[RPC]
	void ReduceMyHealth(int myViewID,int theCase)
	{
		if((photonView.isMine) && (theCase == 1) && (photonView.ownerId == myViewID))
		{
			hurtPlayer = gameObject.GetPhotonView().owner;
			hurtPlayersTransform = gameObject.GetComponentInParent<Transform>();
			//hurtPlayer.customProperties ["Health"] = health;
			ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
			hash.Add("Health",health);
			hurtPlayer.SetCustomProperties(hash);
			//hurtPlayer.rigidbody.AddExplosionForce(1000.0f,hurtPlayer.transform,10.0f,3.0f);
			hurtPlayersTransform.rigidbody.AddExplosionForce(1000.0f,hurtPlayersTransform.position,10.0f,3.0f,ForceMode.Force);

		}else if(photonView.isMine && theCase == 2){

			hurtPlayer = gameObject.GetPhotonView().owner;
			//hurtPlayer.customProperties ["Health"] = health;
			ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
			hash.Add("Health",health);
			hurtPlayer.SetCustomProperties(hash);

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
}
