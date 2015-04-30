using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class RespawnScript : Photon.MonoBehaviour {

	private GameObject[] allPlayers;
	private Vector3 position;
	
	//gameMode bools to determine what kind of respawns we're going to use
	private bool FreeForAll;
	private bool OmegaTank;
	private bool CaptureTheFlag;
	
	private bool notInstantiated;
	private bool goodSpawn;
	
	public float RespawnTime;
	
	// Use this for initialization
	void Start () 
	{
		goodSpawn = false;
		notInstantiated = true;

		RespawnTime = 10.0f;
		
		FreeForAll = false;
		OmegaTank = false;
		CaptureTheFlag = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(FreeForAll == false && OmegaTank == false && CaptureTheFlag == false)
		{
			if((PhotonNetwork.room.customProperties["GameType"].ToString() == "Free For All"))
			{
				FreeForAll = true;
			}
			if((PhotonNetwork.room.customProperties["GameType"].ToString() == "Capture The Flag"))
			{
				CaptureTheFlag = true;
			}
			if((PhotonNetwork.room.customProperties["GameType"].ToString() == "Omega Tank"))
			{
				OmegaTank = true;
			}
		}
		
		if(notInstantiated)
		{
			if(!notInstantiated)
				allPlayers = GameObject.FindGameObjectsWithTag("Player");
		}
	}
	
	MouseOrbitC orbit;
	TankGunController tankGun;
	GameObject Target;

	void checkPlayer(int playerId)
	{
			allPlayers = GameObject.FindGameObjectsWithTag("Player");
			StartCoroutine(waitFiveSeconds(playerId));
	}

	void RespawnThePlayer(Vector3 thePosition)
	{
		GameObject currPlayerHolder = PhotonNetwork.Instantiate("T-90_Prefab_Network", thePosition, Quaternion.identity,0);

		//spawn particle systems
		GameObject normalExhaust = PhotonNetwork.Instantiate("NormalExhaust", currPlayerHolder.transform.Find("Exhaust_Location").transform.position, transform.rotation, 0) as GameObject;
		GameObject heavyExhaust = PhotonNetwork.Instantiate("HeavyExhaust", currPlayerHolder.transform.Find("Exhaust_Location").transform.position, transform.rotation, 0) as GameObject;
		GameObject boost = PhotonNetwork.Instantiate("Boost", currPlayerHolder.transform.Find("BoostLocator").transform.position, new Quaternion(0,-90,0,0), 0) as GameObject;
		
		photonView.RPC ("setNormal",PhotonTargets.AllBuffered,currPlayerHolder.GetComponent<PhotonView>().viewID,normalExhaust.GetComponent<PhotonView>().viewID);
		photonView.RPC ("setHeavy",PhotonTargets.AllBuffered,currPlayerHolder.GetComponent<PhotonView>().viewID,heavyExhaust.GetComponent<PhotonView>().viewID);
		photonView.RPC ("setBoost",PhotonTargets.AllBuffered,currPlayerHolder.GetComponent<PhotonView>().viewID,boost.GetComponent<PhotonView>().viewID);


		//Add the camera target
		orbit = FindObjectOfType<MouseOrbitC>();
		
		//add the tankgun target
		tankGun = currPlayerHolder.GetComponentInChildren<TankGunController>();
		Target = GameObject.Find("Target");

		//flip the bool value in HealthSync script so the camera will stop lerping on the killer and can now be reassigned.
		//GetComponent<HealthSync> ().respawnTimePassed = true;

		orbit.target = currPlayerHolder.transform;
		tankGun.target = Target.transform;
		
		//Turn off own health system
		Transform TankHealthSystem = (Transform)currPlayerHolder.transform.Find ("TankHealthSystem").FindChild ("TankHealthSystemCanvas");
		TankHealthSystem.gameObject.SetActive(false);
		
		ExitGames.Client.Photon.Hashtable hash2 = new ExitGames.Client.Photon.Hashtable();
		hash2.Add("Health",100);
		PhotonNetwork.player.SetCustomProperties(hash2);
		GameObject tempUI = GameObject.FindObjectOfType<UIManager> ().gameObject;
		tempUI.GetComponent<UIManager>().changeDeaths((int)PhotonNetwork.player.customProperties["Deaths"]);

		if (PhotonNetwork.room.customProperties["GameType"].ToString() == "Capture The Flag")
		{
            currPlayerHolder.AddComponent<PickUpFlag_CapFlag>();
		}

		PhotonNetwork.Destroy(gameObject);
	}

	[RPC]
	void RemoveNetworkTrash(int ownerID)
	{
		allPlayers = GameObject.FindGameObjectsWithTag("Player");

		GameObject[] trash = GameObject.FindGameObjectsWithTag("Trash");
		foreach(GameObject currTrash in trash)
		{
			Destroy(currTrash);
		}
	}

	IEnumerator getSpawnPoint()
	{
		if(FreeForAll)
		{
			goodSpawn = false;
			position = new Vector3 (Random.Range (140, 1230), 250.0f, Random.Range (-315, 580));
			while(!goodSpawn)
			{
				if(checkSpawn(position))
				{
					goodSpawn = true;
				}else{
					position = new Vector3 (Random.Range (140, 1230), 250.0f, Random.Range (-315, 580));
				}
			}
			goodSpawn = false;
			yield return new WaitForSeconds(1.0f);
			RespawnThePlayer(position);

		}else if(CaptureTheFlag){
			if (PhotonNetwork.player.customProperties["Team"].ToString() == "Eagles")
			{
				int randX = Random.Range(0, 30);
				int randZ = Random.Range(0, 30);
				position = GameObject.Find("EaglesSpawnPoint").transform.position + new Vector3(randX, 50, randZ);
				yield return new WaitForSeconds(1.0f);
				RespawnThePlayer(position);
			}
			else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Exorcist")
			{
				int randX = Random.Range(0, 30);
				int randZ = Random.Range(0, 30);
				position = GameObject.Find("ExorcistSpawnPoint").transform.position + new Vector3(randX, 50, randZ);
				yield return new WaitForSeconds(1.0f);
				RespawnThePlayer(position);
			}
			else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Wolves")
			{
				int randX = Random.Range(0, 30);
				int randZ = Random.Range(0, 30);
				position = GameObject.Find("WolfSpawnPoint").transform.position + new Vector3(randX, 50, randZ);
				yield return new WaitForSeconds(1.0f);
				RespawnThePlayer(position);
			}
			else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Angel")
			{
				int randX = Random.Range(0, 30);
				int randZ = Random.Range(0, 30);
				position = GameObject.Find("BloodSpawnPoint").transform.position + new Vector3(randX, 50, randZ);
				yield return new WaitForSeconds(1.0f);
				RespawnThePlayer(position);
			}else
			{
				Debug.LogError("THIS GUY DOESN'T HAVE A TEAM WTF");
			}
			
		}else if(OmegaTank){
			
			if(photonView.isMine && (int)photonView.owner.customProperties["TheOmega"] == 1)
			{
				Debug.Log("NOT GOING TO RESPAWN - OMEGA TANK");
			}else if(photonView.isMine){
				goodSpawn = false;
				position = new Vector3 (Random.Range (140, 1230), 250.0f, Random.Range (-315, 580));
				while(!goodSpawn)
				{
					if(checkSpawn(position))
					{
						goodSpawn = true;
					}else{
						position = new Vector3 (Random.Range (140, 1230),250.0f, Random.Range (-315, 580));
					}
				}
				goodSpawn = false;
				yield return new WaitForSeconds(1.0f);
				RespawnThePlayer(position);
			}
		}
	}

	IEnumerator spamForFiveSeconds()
	{
		bool doThis = true;
		int currCount = 0;
		while(doThis)
		{
			if(currCount > 15)
			{
				doThis = false;
			}
			allPlayers = GameObject.FindGameObjectsWithTag("Player");
			currCount++;
			yield return new WaitForSeconds (1f);
		}
		currCount = 0;
		
	}

	IEnumerator waitFiveSeconds(int thisOwnerId)
	{
		yield return new WaitForSeconds (RespawnTime);
		photonView.RPC ("RemoveNetworkTrash",PhotonTargets.All,thisOwnerId);
		StartCoroutine(getSpawnPoint());
	}
	
	bool checkSpawn(Vector3 pos)
	{
		RaycastHit hit;
		
		Vector3 positionCheck = new Vector3(pos.x,250f,pos.z);
		if (Physics.Raycast(positionCheck, Vector3.down, out hit,250f) && !OmegaTank)
		{
			Debug.DrawRay(positionCheck, Vector3.down * hit.distance, Color.blue, 300f);
			if(hit.collider.gameObject.tag == "Terrain"){
				Collider[] hitColliders = Physics.OverlapSphere(hit.point, 100.0f);
				if(hitColliders.Length == 1){
					if(hitColliders[0].tag == "Terrain"){
						return true;
					}else{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}else if(Physics.Raycast(positionCheck, Vector3.down, out hit,250f))
		{
			Debug.DrawRay(positionCheck, Vector3.down * hit.distance, Color.blue, 200f);
			if(hit.collider.gameObject.tag == "Terrain"){
				Collider[] hitColliders = Physics.OverlapSphere(hit.point, 100.0f);
				if(hitColliders.Length == 2){
					if(hitColliders[0].tag == "Player" || hitColliders[1].tag == "Player"){
						if (PhotonNetwork.player.customProperties["Team"].ToString() == "Eagles")
						{
							if(hitColliders[0].tag == "Player")
							{
								if(hitColliders[0].gameObject.GetPhotonView().owner.customProperties["Team"].ToString() == "Eagles")
								{
									return true;
								}
							}else if(hitColliders[1].tag == "Player")
							{
								if(hitColliders[1].gameObject.GetPhotonView().owner.customProperties["Team"].ToString() == "Eagles")
								{
									return true;
								}
							}else{
								return false;
							}
						}
						else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Exorcist")
						{
							if(hitColliders[0].tag == "Player")
							{
								if(hitColliders[0].gameObject.GetPhotonView().owner.customProperties["Team"].ToString() == "Exorcist")
								{
									return true;
								}
							}else if(hitColliders[1].tag == "Player")
							{
								if(hitColliders[1].gameObject.GetPhotonView().owner.customProperties["Team"].ToString() == "Exorcist")
								{
									return true;
								}
							}else{
								return false;
							}
						}
						else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Wolves")
						{
							if(hitColliders[0].tag == "Player")
							{
								if(hitColliders[0].gameObject.GetPhotonView().owner.customProperties["Team"].ToString() == "Wolves")
								{
									return true;
								}
							}else if(hitColliders[1].tag == "Player")
							{
								if(hitColliders[1].gameObject.GetPhotonView().owner.customProperties["Team"].ToString() == "Wolves")
								{
									return true;
								}
							}else{
								return false;
							}
						}
						else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Angel")
						{
							if(hitColliders[0].tag == "Player")
							{
								if(hitColliders[0].gameObject.GetPhotonView().owner.customProperties["Team"].ToString() == "Angel")
								{
									return true;
								}
							}else if(hitColliders[1].tag == "Player")
							{
								if(hitColliders[1].gameObject.GetPhotonView().owner.customProperties["Team"].ToString() == "Angel")
								{
									return true;
								}
							}else{
								return false;
							}
						}
					}else{
						return false;
					}
				}
			}
			return false;
		}
		return false;
	}

	[RPC]
	void ActivateRespawn(PhotonPlayer thePlayer)
	{
		if(photonView.owner == thePlayer && photonView.isMine)
		{
			checkPlayer(thePlayer.ID);
		}
	}

	GameObject tank;
	GameObject system;
	[RPC]
	public void setNormal(int tankView,int particleSystemView){
		PhotonView[] views = FindObjectsOfType<PhotonView>();
		foreach(PhotonView view in views){
			if(view.viewID == tankView){
				tank = view.gameObject;
			}
			if(view.viewID == particleSystemView){
				system = view.gameObject;
			}
		}
		system.name = "Normal Exhaust";
		system.transform.parent = tank.transform.Find ("Exhaust_Location");
		system.transform.Rotate (0, 270, 0);
	}
	
	[RPC]
	public void setHeavy(int tankView,int particleSystemView){
		PhotonView[] views = FindObjectsOfType<PhotonView>();
		foreach(PhotonView view in views){
			if(view.viewID == tankView){
				tank = view.gameObject;
			}
			if(view.viewID == particleSystemView){
				system = view.gameObject;
			}
		}
		system.name = "Heavy Exhaust";
		system.transform.parent = tank.transform.Find ("Exhaust_Location");
		system.transform.Rotate (0, 270, 0);
	}
	
	[RPC]
	public void setBoost(int tankView,int particleSystemView){
		PhotonView[] views = FindObjectsOfType<PhotonView>();
		foreach(PhotonView view in views){
			if(view.viewID == tankView){
				tank = view.gameObject;
			}
			if(view.viewID == particleSystemView){
				system = view.gameObject;
			}
		}
		system.name = "Boost";
		system.transform.parent = tank.transform.Find ("BoostLocator");
		
	}

}
