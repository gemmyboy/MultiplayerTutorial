﻿using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class RespawnScript : Photon.MonoBehaviour {
	
	private PhotonPlayer player1;
	private PhotonPlayer player2;
	private PhotonPlayer player3;
	private PhotonPlayer player4;

	private bool respawn;
	private bool respawn1;
	private bool respawn2;
	private bool respawn3;
	private bool respawn4;
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
		respawn1 = false;
		respawn2 = false;
		respawn3 = false;
		respawn4 = false;

		RespawnTime = 5.0f;
		
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
	}
	
	MouseOrbitC orbit;
	TankGunController tankGun;
	GameObject Target;

	[RPC]
	void checkPlayer(int playerId)
	{
		if(player1.ID == playerId && photonView.isMine)
		{
			Debug.Log("GOT TO checkPlayer1");
			allPlayers = GameObject.FindGameObjectsWithTag("Player");
			StartCoroutine(waitFiveSeconds(player1.ID));

			photonView.RPC ("RemoveNetworkTrash",PhotonTargets.All,player1.ID);
		}else if(player2.ID == playerId && photonView.isMine)
		{
			Debug.Log("GOT TO checkPlayer2");
			allPlayers = GameObject.FindGameObjectsWithTag("Player");
			StartCoroutine(waitFiveSeconds(player2.ID));
			
			photonView.RPC ("RemoveNetworkTrash",PhotonTargets.All,player2.ID);
		}else if(player3.ID == playerId && photonView.isMine)
		{
			Debug.Log("GOT TO checkPlayer3");
			allPlayers = GameObject.FindGameObjectsWithTag("Player");
			StartCoroutine(waitFiveSeconds(player3.ID));
			
			photonView.RPC ("RemoveNetworkTrash",PhotonTargets.All,player3.ID);
		}else if(player4.ID == playerId && photonView.isMine)
		{
			Debug.Log("GOT TO checkPlayer4");
			allPlayers = GameObject.FindGameObjectsWithTag("Player");
			StartCoroutine(waitFiveSeconds(player4.ID));
			
			photonView.RPC ("RemoveNetworkTrash",PhotonTargets.All,player4.ID);
		}
	}
	
	[RPC]
	void RespawnThePlayer(Vector3 thePosition)
	{

		Debug.Log("GOT TO RespawnThePlayer");
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
		hash2.Add("Health",100);
		PhotonNetwork.player.SetCustomProperties(hash2);
		GameObject tempUI = GameObject.FindObjectOfType<UIManager> ().gameObject;
		tempUI.GetComponent<UIManager>().changeDeaths((int)PhotonNetwork.player.customProperties["Deaths"]);
	}
	
	[RPC]
	void RemoveNetworkTrash(int ownerID)
	{
		Debug.Log("GOT TO RemoveNetworkTrash");
		foreach(GameObject eachMofo in allPlayers)
		{
			if(eachMofo.GetPhotonView().ownerId == ownerID)
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
				Debug.Log("GOT TO getSpawnPoint");
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
			}else if(photonView.isMine){
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

	IEnumerator waitFiveSeconds(int thisOwnerId)
	{
		//if(photonView.ownerId == thisOwnerId)
		//{
			yield return new WaitForSeconds (RespawnTime);
			respawn = true;
			Debug.Log("GOT TO waitFiveSeconds");
			StartCoroutine(getSpawnPoint(PhotonPlayer.Find (thisOwnerId)));
		//}
	}
	
	bool checkSpawn(Vector3 pos)
	{
		Debug.Log("GOT TO checkSpawn");
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
		if(photonView.isMine)
		{
			Debug.Log("GOT TO ActivateRespawn");
			photonView.RPC("checkPlayer",PhotonTargets.All,thePlayer.ID);
		}
		//allPlayers = GameObject.FindGameObjectsWithTag("Player");
		//checkPlayer(thePlayer);
	}
	
	
}
