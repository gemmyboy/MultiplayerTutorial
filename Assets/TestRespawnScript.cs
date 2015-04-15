using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class TestRespawnScript : Photon.MonoBehaviour {
	
	private PhotonPlayer player1;
	private PhotonPlayer player2;
	private PhotonPlayer player3;
	private PhotonPlayer player4;
	private bool respawn;
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
		respawn = false;
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
			
			
			//Debug.Log ("Respawn GAMEMODE has been set!****:  ");
			if((PhotonNetwork.room.customProperties["GameType"].ToString() == "Free For All"))
			{
				FreeForAll = true;
				Debug.Log (FreeForAll);
			}
			if((PhotonNetwork.room.customProperties["GameType"].ToString() == "Capture The Flag"))
			{
				CaptureTheFlag = true;
				Debug.Log(CaptureTheFlag);
			}
			if((PhotonNetwork.room.customProperties["GameType"].ToString() == "Omega Tank"))
			{
				OmegaTank = true;
				Debug.Log(OmegaTank);
			}
		}
		
		if(notInstantiated)
		{
			if(PhotonPlayer.Find(1) != null)
			{
				player1 = PhotonPlayer.Find (1);
				notInstantiated = false;
				//Debug.Log ("Player 1 set!***");
			}
			if(PhotonPlayer.Find (2) != null)
			{
				player2 = PhotonPlayer.Find (2);
				//Debug.Log ("Player 2 set!***");
			}
			if(PhotonPlayer.Find (3) != null)
			{
				player3 = PhotonPlayer.Find (3);
				//Debug.Log ("Player 3 set!***");
			}
			if(PhotonPlayer.Find (4) != null)
			{
				player4 = PhotonPlayer.Find (4);
				//Debug.Log ("Player 4 set!***");
			}
			
			if(!notInstantiated)
				allPlayers = GameObject.FindGameObjectsWithTag("Player");
			
		}
		if(respawn == true)
		{
			if(player1 != null)
			{
				StartCoroutine(spamForFiveSeconds());
				allPlayers = GameObject.FindGameObjectsWithTag("Player");
				checkPlayer(player1);
			}
			if(player2 != null)
			{
				allPlayers = GameObject.FindGameObjectsWithTag("Player");
				checkPlayer(player2);
			}
			if(player3 != null)
			{
				allPlayers = GameObject.FindGameObjectsWithTag("Player");
				checkPlayer(player3);
			}
			if(player4 != null)
			{
				allPlayers = GameObject.FindGameObjectsWithTag("Player");
				checkPlayer(player4);
			}
		}
	}
	
	MouseOrbitC orbit;
	TankGunController tankGun;
	GameObject Target;
	void checkPlayer(PhotonPlayer player)
	{
		if(player != null)
		{
			allPlayers = GameObject.FindGameObjectsWithTag("Player");
			foreach(GameObject currPlayer in allPlayers)
			{
				if((respawn == false) && ((int)currPlayer.GetPhotonView().owner.customProperties["Dead"] == 1) && (currPlayer.GetPhotonView().owner == player) && (currPlayer.GetComponent<HealthSync>().activateRespawn == true) && photonView.isMine)
				{
					StartCoroutine(waitFiveSeconds());
				}
				if(((int)currPlayer.GetPhotonView().owner.customProperties["Dead"] == 1) && (currPlayer.GetPhotonView().owner == player) && (currPlayer.GetComponent<HealthSync>().activateRespawn == true) && respawn == true && photonView.isMine)
				{
					StartCoroutine (getSpawnPoint(player));
					
					photonView.RPC ("RemoveNetworkTrash",PhotonTargets.All,currPlayer.GetPhotonView().viewID);
					
					allPlayers = GameObject.FindGameObjectsWithTag("Player");
					break;
				}
			}
		}
	}
	
	[RPC]
	void RespawnThePlayer(Vector3 thePosition)
	{
		GameObject currPlayerHolder = PhotonNetwork.Instantiate("T-90_Prefab_Network", thePosition, Quaternion.identity,0);
		
		//Add the camera target
		orbit = FindObjectOfType<MouseOrbitC>();
		
		//add the tankgun target
		tankGun = currPlayerHolder.GetComponentInChildren<TankGunController>();
		Target = GameObject.Find("Target");
		orbit.target = currPlayerHolder.transform;
		tankGun.target = Target.transform;
		
		//Turn off own health system
		Transform TankHealthSystem = (Transform)currPlayerHolder.transform.Find ("TankHealthSystem").FindChild ("TankHealthSystemCanvas");
		TankHealthSystem.gameObject.SetActive(false);
		
		ExitGames.Client.Photon.Hashtable hash2 = new ExitGames.Client.Photon.Hashtable();
		//hash2.Add("Kills", (int)PhotonNetwork.player.customProperties["Kills"]);
		//hash2.Add("Deaths",(int)PhotonNetwork.player.customProperties["Deaths"]+1);
		//hash2.Add("Assist",(int)PhotonNetwork.player.customProperties["Assist"]);
		hash2.Add("Health",100);
		PhotonNetwork.player.SetCustomProperties(hash2);
		GameObject tempUI = GameObject.FindObjectOfType<UIManager> ().gameObject;
		tempUI.GetComponent<UIManager>().changeDeaths((int)PhotonNetwork.player.customProperties["Deaths"]);
	}
	
	[RPC]
	void RemoveNetworkTrash(int viewID)
	{
		foreach(GameObject eachMofo in allPlayers)
		{
			if(eachMofo.GetPhotonView().viewID == viewID)
				Destroy (eachMofo.gameObject);
		}
		
		GameObject[] trash = GameObject.FindGameObjectsWithTag("Trash");
		foreach(GameObject currTrash in trash)
		{
			Destroy(currTrash);
		}
	}
	
	IEnumerator getSpawnPoint(PhotonPlayer thePlayer)
	{
		if(FreeForAll)
		{
			if(photonView.isMine){
				goodSpawn = false;
				position = new Vector3 (Random.Range (140, 1230), 200.0f, Random.Range (-315, 580));
				while(!goodSpawn)
				{
					if(checkSpawn(position))
					{
						goodSpawn = true;
					}else{
						position = new Vector3 (Random.Range (140, 1230), 200.0f, Random.Range (-315, 580));
					}
				}
				goodSpawn = false;
				yield return new WaitForSeconds(1.0f);
				respawn = false;
				photonView.RPC ("RespawnThePlayer",thePlayer,position);
			}
		}else if(CaptureTheFlag){
			Debug.Log (thePlayer);
			Debug.Log(thePlayer.customProperties["Team"].ToString());
			if (photonView.isMine && thePlayer.customProperties["Team"].ToString() == "Eagles")
			{
				int randX = Random.Range(0, 30);
				int randZ = Random.Range(0, 30);
				position = GameObject.Find("EaglesSpawnPoint").transform.position + new Vector3(randX, 100, randZ);
				yield return new WaitForSeconds(1.0f);
				respawn = false;
				photonView.RPC ("RespawnThePlayer",thePlayer,position);
			}
			else if (photonView.isMine && thePlayer.customProperties["Team"].ToString() == "Exorcist")
			{
				int randX = Random.Range(0, 30);
				int randZ = Random.Range(0, 30);
				position = GameObject.Find("ExorcistSpawnPoint").transform.position + new Vector3(randX, 100, randZ);
				yield return new WaitForSeconds(1.0f);
				respawn = false;
				photonView.RPC ("RespawnThePlayer",thePlayer,position);
			}
			else if (photonView.isMine && thePlayer.customProperties["Team"].ToString() == "Wolves")
			{
				int randX = Random.Range(0, 30);
				int randZ = Random.Range(0, 30);
				position = GameObject.Find("WolfSpawnPoint").transform.position + new Vector3(randX, 100, randZ);
				yield return new WaitForSeconds(1.0f);
				respawn = false;
				photonView.RPC ("RespawnThePlayer",thePlayer,position);
			}
			else if (photonView.isMine && thePlayer.customProperties["Team"].ToString() == "Angel")
			{
				int randX = Random.Range(0, 30);
				int randZ = Random.Range(0, 30);
				position = GameObject.Find("BloodSpawnPoint").transform.position + new Vector3(randX, 100, randZ);
				yield return new WaitForSeconds(1.0f);
				respawn = false;
				photonView.RPC ("RespawnThePlayer",thePlayer,position);
			}
			
			
			
		}else if(OmegaTank){
			
			if(photonView.isMine && (int)photonView.owner.customProperties["TheOmega"] == 1)
			{
				Debug.Log("NOT GOING TO RESPAWN - OMEGA TANK");
			}else{
				goodSpawn = false;
				position = new Vector3 (Random.Range (140, 1230), 200.0f, Random.Range (-315, 580));
				while(!goodSpawn)
				{
					if(checkSpawn(position))
					{
						goodSpawn = true;
					}else{
						position = new Vector3 (Random.Range (140, 1230), 200.0f, Random.Range (-315, 580));
					}
				}
				goodSpawn = false;
				yield return new WaitForSeconds(1.0f);
				respawn = false;
				photonView.RPC ("RespawnThePlayer",thePlayer,position);
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
	
	
	IEnumerator waitFiveSeconds()
	{
		if(photonView.isMine)
		{
			yield return new WaitForSeconds (RespawnTime);
			respawn = true;
		}
	}
	
	bool checkSpawn(Vector3 pos)
	{
		RaycastHit hit;
		
		Vector3 positionCheck = new Vector3(pos.x,200f,pos.z);
		if (Physics.Raycast(positionCheck, Vector3.down, out hit,250f) && !OmegaTank)
		{
			Debug.DrawRay(positionCheck, Vector3.down * hit.distance, Color.blue, 200f);
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
					//					foreach (Collider collider in hitColliders)
					//					{
					//						Debug.Log(collider.gameObject.name);
					//					}
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
	
	void ActivateRespawn(PhotonPlayer thePlayer)
	{
		allPlayers = GameObject.FindGameObjectsWithTag("Player");
		checkPlayer(thePlayer);
		/*
		if(player1 != null)
		{
			if(player1 == thePlayer)
			{
				allPlayers = GameObject.FindGameObjectsWithTag("Player");
				checkPlayer(player1);
			}
		}
		if(player2 != null){
			if(player2 == thePlayer)
			{
				allPlayers = GameObject.FindGameObjectsWithTag("Player");
				checkPlayer(player2);
			}
		}
		if(player3 != null){
			if(player3 == thePlayer)
			{
				allPlayers = GameObject.FindGameObjectsWithTag("Player");
				checkPlayer(player3);
			}
		}
		if(player4 != null){
			if(player4 == thePlayer)
			{
				allPlayers = GameObject.FindGameObjectsWithTag("Player");
				checkPlayer(player4);
			}
		}
		*/
	}


}