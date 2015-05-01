using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthSync : Photon.MonoBehaviour {

	public UIManager uiManager;
	private UIManager testForManager;

	public bool dead;
	private bool uiManagerStillNull;

	public float TankShellDamage;
	private PhotonPlayer hurtPlayer;
	private Text myText;
	private RectTransform myRect;

	public GameObject myTextObj;
	public GameObject myRectObj;

	public static int healthAmount;
	private GameObject theBullet;
	public bool activateRespawn;
	private float respawnTimer;
	public GameObject mainCam;
	public bool respawnTimePassed;

	private GameObject tempSpecObject;


	public float CameraMoveTime = 1.0f;
	public GameObject dummy;
	void Start()
	{
		respawnTimePassed = false;
		dead = false;
		uiManagerStillNull = true;
		uiManager = null;
		testForManager = null;
		hurtPlayer = null;
		myText = myTextObj.GetComponent<Text> ();
		myRect = myRectObj.GetComponent<RectTransform> ();

		hurtPlayer = gameObject.GetPhotonView().owner;
		healthAmount = (int)gameObject.GetPhotonView ().owner.customProperties ["Health"];
		//photonView.RPC("AdjustPercent",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,healthAmount);

		ExitGames.Client.Photon.Hashtable hash4 = new ExitGames.Client.Photon.Hashtable();
		hash4.Add("Dead",0);
		hurtPlayer.SetCustomProperties(hash4);

		activateRespawn = false;
		respawnTimer = 0.0f;
		mainCam = GameObject.Find ("Main Camera");
	}

    void Update()
    {
		Debug.Log (PhotonNetwork.player.customProperties["Kills"]);
		Debug.Log (PhotonNetwork.player.customProperties["Deaths"]);

		if(uiManagerStillNull && photonView.isOwnerActive && (photonView.ownerId == gameObject.GetPhotonView().ownerId))
		{
			testForManager = FindObjectOfType<UIManager>();
			if(testForManager != null)
			{
				uiManagerStillNull = false;
				uiManager = testForManager;
				uiManager.ChangeHealth(healthAmount);
				photonView.RPC("AdjustPercent",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,healthAmount);
			}
		}
    }

	void OnCollisionEnter(Collision col)
	{
        Debug.Log("HIT TANK");
        theBullet = col.gameObject;
        if (photonView.isMine && (col.gameObject.tag == "TankShell"))
        {
            Debug.Log("HIT TANK2");
            healthAmount = (int)PhotonNetwork.player.customProperties["Health"];
            Debug.Log(healthAmount);
            Debug.Log(col.gameObject.tag);
            if (healthAmount > 0)
            {
                Debug.Log("HIT TANK3");
                if (theBullet.GetPhotonView().owner.customProperties["Team"].ToString() != gameObject.GetPhotonView().owner.customProperties["Team"].ToString())
                {
                    if (((healthAmount - (int)TankShellDamage)) <= 0)
                    {
                        photonView.RPC("ReduceMyHealth", PhotonTargets.MasterClient, photonView.ownerId, 1, theBullet.GetPhotonView().ownerId);
                        //photonView.RPC("tankGoBoom", PhotonTargets.All, gameObject.GetPhotonView().viewID);
                    }
                    else if ((healthAmount - (int)TankShellDamage) > 0)
                    {
                        photonView.RPC("ReduceMyHealth", PhotonTargets.MasterClient, photonView.ownerId, 2, theBullet.GetPhotonView().ownerId);
                    }
                }
            }
        }
	}


	[RPC]
	void ReduceMyHealth(int myViewID,int theCase,int theKiller)
	{
		Debug.LogError ("GOT HERE");
		if(theCase == 1)
		{
			Debug.LogError("PERSON GOT SHOT - master should be only one seeing this");

			PhotonPlayer tempPlayer = PhotonPlayer.Find(theKiller);
			PhotonPlayer tempPlayer2 = PhotonPlayer.Find(myViewID);

			ExitGames.Client.Photon.Hashtable hash3 = new ExitGames.Client.Photon.Hashtable();
			hash3.Add("Health",0);
			tempPlayer2.SetCustomProperties(hash3);
			healthAmount = (int)tempPlayer2.customProperties["Health"];
			//
			ExitGames.Client.Photon.Hashtable hash4 = new ExitGames.Client.Photon.Hashtable();
			hash4.Add("Dead",1);
			tempPlayer2.SetCustomProperties(hash4);

			respawnTimer = Time.time + 5.0f;

			//transform.rigidbody.AddExplosionForce(150000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);
			photonView.RPC ("AdjustHealthBar",PhotonTargets.All,myViewID,1);
			photonView.RPC("AdjustPercent",PhotonTargets.All,myViewID,healthAmount);

			//person killed gets his ui health updated
			photonView.RPC ("UpdateHealthUI",tempPlayer2,(int)tempPlayer2.customProperties["Health"]);

			//killer gets his kills updated
			ExitGames.Client.Photon.Hashtable hash10 = new ExitGames.Client.Photon.Hashtable();
			hash10.Add("Kills",((int)tempPlayer.customProperties["Kills"]+1));
			tempPlayer.SetCustomProperties(hash10);
			Debug.LogError(tempPlayer);
			Debug.LogError((int)tempPlayer.customProperties["Kills"]);
			photonView.RPC ("UpdateKillsUI",tempPlayer, tempPlayer.customProperties["Kills"]);


			//your deaths get updated
			ExitGames.Client.Photon.Hashtable hash11 = new ExitGames.Client.Photon.Hashtable();
			hash11.Add("Deaths",(int)tempPlayer2.customProperties["Deaths"]+1);
			tempPlayer2.SetCustomProperties(hash11);
			Debug.LogError(tempPlayer2);
			Debug.LogError((int)tempPlayer2.customProperties["Deaths"]);
			photonView.RPC ("UpdateDeathsUI",tempPlayer2,(int)tempPlayer2.customProperties["Deaths"]);

			photonView.RPC("tankGoBoom", PhotonTargets.All,tempPlayer2.ID,theKiller);
			//transform.rigidbody.AddExplosionForce(150000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);
			
		}else if(theCase == 2 && photonView.isMine){
			Debug.LogError("PERSON GOT SHOT2 - master should be only one seeing this");
			PhotonPlayer tempPlayer = PhotonPlayer.Find(theKiller);
			PhotonPlayer tempPlayer2 = PhotonPlayer.Find(myViewID);

			ExitGames.Client.Photon.Hashtable hash2 = new ExitGames.Client.Photon.Hashtable();
			healthAmount = (int)tempPlayer2.customProperties["Health"] - (int)TankShellDamage;
			hash2.Add("Health",healthAmount);
			tempPlayer2.SetCustomProperties(hash2);
			photonView.RPC ("UpdateHealthUI",tempPlayer2,(int)tempPlayer2.customProperties["Health"]);

			photonView.RPC ("MakePersonRock",tempPlayer2);

			photonView.RPC("AdjustPercent",PhotonTargets.All,tempPlayer2.ID,healthAmount);
			photonView.RPC ("AdjustHealthBar",PhotonTargets.All,tempPlayer2.ID,2);

		}
	}

	[RPC]
	void MakePersonRock()
	{
		transform.rigidbody.AddExplosionForce (15000.0f, transform.position, 10.0f, 0.0f, ForceMode.Impulse);
	}

	[RPC]
	void AdjustPercent(int myViewID, int health)
	{
		if(photonView.ownerId == myViewID)
		{
			if(myText != null)
				myText.text = health.ToString ();
		}
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

    GameObject tank;
    Transform[] children;
    GameObject trash;

    [RPC]
    void tankGoBoom(int ownerID,int myKiller)
    {
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach(PhotonView view in views){
            if(view.ownerId == ownerID)
			{
                tank = view.gameObject;
				if(tank.GetComponent<HealthSync>())
					tank.GetComponent<HealthSync>().activateRespawn = true;
				GameObject[] tempPlayers = GameObject.FindGameObjectsWithTag("Player");
				foreach(GameObject tempPlayer in tempPlayers)
				{
					if(tempPlayer.GetPhotonView().ownerId == myKiller && photonView == tank.GetPhotonView())
					{
						photonView.RPC("AdjustCameraView",tank.GetPhotonView().owner,myKiller);
					}
				}
            }
        }
        Destroy(tank.GetComponent<TankController>());
        Destroy(tank.GetComponent<TankInterpolationMovement>());

		if(tank.transform.Find ("TankHealthSystem"))
			tank.transform.Find ("TankHealthSystem").tag = "Trash";

        Destroy(tank.GetComponent<Kills_Deaths_Assist>());
		Destroy (tank.GetComponent<AdjustTankSkins> ());
		Destroy (tank.GetComponent<TestExplosionForceScript> ());
		Destroy(tank.GetComponentInChildren<TankGunController>());
		Destroy(tank.GetComponentInChildren<TankLerpMovement>());
        if(PhotonNetwork.room.customProperties["GameType"].ToString() == "Capture The Flag"){
            Destroy(tank.GetComponent<PickUpFlag_CapFlag>());
        }

		GameObject[] mainGuns = GameObject.FindGameObjectsWithTag ("MainGun");
		foreach(GameObject mainGun in mainGuns)
		{
			if(mainGun.transform.parent.name == "MainGun")
				fixForExplosion(mainGun);
			if(mainGun.transform.parent == tank.transform.parent)
				fixForExplosion(mainGun);
		}
		if(tank.transform.Find("MainGun") != null)
		{
       		fixForExplosion(tank.transform.Find("MainGun").gameObject);
		}

		if(tank.transform.Find("WheelTransforms"))
		{
			if(tank.transform.Find("WheelTransforms").gameObject.transform.Find("WheelTransforms_L"))
        		detachMultiple(tank.transform.Find("WheelTransforms").gameObject.transform.Find("WheelTransforms_L").gameObject);
			if(tank.transform.Find("WheelTransforms").gameObject.transform.Find("WheelTransforms_R"))
       			detachMultiple(tank.transform.Find("WheelTransforms").gameObject.transform.Find("WheelTransforms_R").gameObject);
		}
		if(tank.transform.Find("UselessGearsTransforms"))
		{
			if(tank.transform.Find("UselessGearsTransforms").gameObject.transform.Find("UselessGearsTransforms_L"))
       			detachMultiple(tank.transform.Find("UselessGearsTransforms").gameObject.transform.Find("UselessGearsTransforms_L").gameObject);
			if(tank.transform.Find("UselessGearsTransforms").gameObject.transform.Find("UselessGearsTransforms_R"))
        		detachMultiple(tank.transform.Find("UselessGearsTransforms").gameObject.transform.Find("UselessGearsTransforms_R").gameObject);
		}
		if(tank.transform.Find ("WheelColliders"))
			Destroy (tank.transform.Find ("WheelColliders").gameObject);
		if(tank.transform.Find("Skirts"))
		{
			if(tank.transform.Find("Skirts").gameObject.transform.Find("Skirts_R"))
			{
       			DestroyHinge(tank.transform.Find("Skirts").gameObject.transform.Find("Skirts_R").gameObject);
				detachMultiple(tank.transform.Find("Skirts").gameObject.transform.Find("Skirts_R").gameObject);
			}
			if(tank.transform.Find("Skirts").gameObject.transform.Find("Skirts_L"))
			{
        		DestroyHinge(tank.transform.Find("Skirts").gameObject.transform.Find("Skirts_L").gameObject);
				detachMultiple(tank.transform.Find("Skirts").gameObject.transform.Find("Skirts_L").gameObject);
			}
		}

		if(tank.transform.Find("Tracks"))
       		Destroy(tank.transform.Find("Tracks").gameObject);
		if(tank.transform.Find("HeadLights"))
        	Destroy(tank.transform.Find("HeadLights").gameObject);
		if(tank.transform.Find("HeavyExhaust"))
       		Destroy(tank.transform.Find("HeavyExhaust").gameObject);
		if(tank.transform.Find("NormalExhaust"))
        	Destroy(tank.transform.Find("NormalExhaust").gameObject);

		PhotonView[] childrenViews = tank.GetComponentsInChildren<PhotonView> ();
		foreach(PhotonView thisChildView in childrenViews)
		{
			if(thisChildView.GetComponent<PhotonTransformView>() != null)
			{
				Destroy (thisChildView.GetComponent<PhotonTransformView>());
			}
			if(thisChildView.tag != "Player")
				Destroy (thisChildView.GetComponent<PhotonView>());
		}

		if(tank.transform.Find ("WheelTransforms"))
			Destroy (tank.transform.Find ("WheelTransforms").gameObject);
		if(tank.transform.Find ("MainGunColliders"))
			Destroy (tank.transform.Find ("MainGunColliders").gameObject);
		if(tank.transform.Find ("EngineIdleAudioClip"))
			Destroy (tank.transform.Find ("EngineIdleAudioClip").gameObject);
		if(tank.transform.Find ("EngineRunningAudioClip"))
			Destroy (tank.transform.Find ("EngineRunningAudioClip").gameObject);
		if(tank.transform.Find ("UselessGearsTransforms"))
			Destroy (tank.transform.Find ("UselessGearsTransforms").gameObject);
		if(tank.transform.Find ("Skirts"))
			Destroy (tank.transform.Find ("Skirts").gameObject);
		if(tank.transform.Find ("Misc"))
			Destroy (tank.transform.Find ("Misc").gameObject);
		if(tank.transform.Find ("COM"))
			Destroy (tank.transform.Find ("COM").gameObject);
		if(tank.transform.Find ("BoneTransforms"))
			Destroy (tank.transform.Find ("BoneTransforms").gameObject);
		if(tank.transform.Find ("Dynamic Com"))
			Destroy (tank.transform.Find ("Dynamic Com").gameObject);


        if(tank.transform.Find ("BoostLocator"))
            Destroy(tank.transform.Find("BoostLocator").gameObject);
        if (tank.transform.Find("Exhaust_Location"))
            Destroy(tank.transform.Find("Exhaust_Location").gameObject);
        if (tank.transform.Find("TankHealthSystem"))
            Destroy(tank.transform.Find("TankHealthSystem").gameObject);

        tank.transform.DetachChildren();


		Destroy(tank.GetComponentInChildren<TankGunColliders>());

        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, 10);
        foreach (Collider hit in colliders) {
			//Debug.Log(hit.name);
			if (hit.tag != "Terrain" && hit.gameObject.layer != LayerMask.NameToLayer ("Flag") && hit.name != "Map-3-26") {
				if (hit.GetComponent<Rigidbody> () == null) {
					hit.gameObject.AddComponent<Rigidbody> ();
					hit.gameObject.GetComponent<Rigidbody> ().mass = 3000f;
					hit.gameObject.tag = "Trash";
				}
			}

			if (hit && hit.rigidbody) {
				hit.rigidbody.isKinematic = false;
				hit.gameObject.GetComponent<Rigidbody> ().mass = 3000f;
				hit.gameObject.tag = "Trash";
				if(hit.name != "MainBody Mesh")
					hit.rigidbody.AddExplosionForce (100000, hit.rigidbody.transform.position, 10.0f, 0.0f, ForceMode.Impulse);
//				else if(hit.name == "MainBody Mesh")
//				{
//					tempSpecObject = hit.gameObject; 
//					Debug.Log ("SHOULD HAVE WORKED");
//				}
			}
           
		}
		if(photonView.isMine && photonView == tank.GetPhotonView())
			photonView.RPC ("ActivateRespawn",tank.GetPhotonView().owner,tank.GetPhotonView ().owner);
    }
    void fixForExplosion(GameObject obj)
    {
        //Debug.Log(obj);
        obj.AddComponent<BoxCollider>();
        obj.GetComponent<Rigidbody>().useGravity = true;
		obj.GetComponent<Rigidbody> ().mass = 3000f;
		obj.tag = "Trash";

        if (obj.GetComponent<HingeJoint>() != null)
        {
            Destroy(obj.GetComponent<HingeJoint>());
        }	
    }

    void DestroyHinge(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            Destroy(child.GetComponent<HingeJoint>());
        }
    }

    void detachMultiple(GameObject obj)
    {
        obj.tag = "Trash";
        if (checkObject(obj))
        {
            return;
        }
        foreach (Transform child in obj.transform)
        {

            if (child.GetComponent<BoxCollider>() == null)
            {
                child.gameObject.AddComponent<BoxCollider>();
            }
            
            child.tag = "Trash";
            child.DetachChildren();
        }
        obj.transform.DetachChildren();
    }
    bool checkObject(GameObject obj)
    {
        string[] list = new string[] { "Tracks", "MainGun", "Barrel" };
        foreach (string sting in list)
        {
            if (sting == obj.name)
            {
                return true;
            }
        }
        return false;
    }

	[RPC]
	void UpdateKillsUI(int kills)
	{
		uiManager.changeKills(kills);
	}

	[RPC]
	void UpdateDeathsUI(int deaths)
	{
		uiManager.changeDeaths (deaths);
	}

	[RPC]
	void UpdateHealthUI(int health)
	{
		uiManager.ChangeHealth (health);
	}

	[RPC]
	void AdjustCameraView(int ourKiller)
	{
		GameObject[] tempPlayers = GameObject.FindGameObjectsWithTag("Player");

		foreach(GameObject thisPlayer in tempPlayers)
		{
			if(thisPlayer.GetPhotonView().ownerId == ourKiller)
			{
				mainCam.GetComponent<MouseOrbitC> ().moving = true;

				tempSpecObject = new GameObject();
				tempSpecObject.transform.position = mainCam.transform.position - new Vector3(7.0f,7.0f,7.0f);
				mainCam.GetComponent<MouseOrbitC>().target = tempSpecObject.transform;
				StartCoroutine("lerpPosition",thisPlayer.transform);
			}
		}
	}

	IEnumerator lerpPosition(Transform tempPlayer)
	{
		Vector3 startPos = mainCam.transform.position;
		Vector3 directon = mainCam.transform.position - tempPlayer.transform.position;
		directon.Normalize ();
		Vector3 endPos = tempPlayer.position + (directon * 20);

		Quaternion startRot = Quaternion.LookRotation(mainCam.GetComponent<MouseOrbitC> ().target.transform.position - mainCam.transform.position,Vector3.up);
		Quaternion endRot = Quaternion.LookRotation(tempPlayer.transform.position - mainCam.transform.position,Vector3.up);
		
		float t = 0.0f;
		float seconds = CameraMoveTime;

		yield return new WaitForSeconds (5.0f);

		while (t <= 1.0f)
		{
			t += Time.deltaTime / seconds;
			mainCam.transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0.0f, 1.0f, t));
			mainCam.transform.rotation = Quaternion.Lerp(startRot,endRot,Mathf.SmoothStep(0.0f, 1.0f, t));

			directon = mainCam.transform.position - tempPlayer.transform.position;
			directon.Normalize ();
			endPos = tempPlayer.position + (directon * 20);
			endRot = Quaternion.LookRotation(tempPlayer.transform.position - mainCam.transform.position,Vector3.up*10f);

			yield return new WaitForEndOfFrame();
		}
		Destroy (tempSpecObject);

		mainCam.GetComponent<MouseOrbitC> ().target = tempPlayer.transform;
		mainCam.transform.LookAt (tempPlayer.transform);
		mainCam.GetComponent<MouseOrbitC> ().moving = false;
		yield return null;
	}

}
