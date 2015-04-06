using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class RespawnScript : Photon.MonoBehaviour {

	private PhotonPlayer player1;
	private PhotonPlayer player2;
	private PhotonPlayer player3;
	private PhotonPlayer player4;
	private bool respawn;
	private GameObject[] allPlayers;
	private Vector3 position;

	private Vector3 spawnPoint;

	private bool notInstantiated;
	private GameObject destroyThisGuy;
	private bool alreadyRespawnedPlayer;

	private float timer1;
	// Use this for initialization
	void Start () 
	{
		notInstantiated = true;
		timer1 = Time.time;
		respawn = false;
		alreadyRespawnedPlayer = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(notInstantiated)
		{
			if(PhotonPlayer.Find(1) != null)
			{
				player1 = PhotonPlayer.Find (1);
				notInstantiated = false;
				Debug.Log ("Player 1 set!***");
			}
			if(PhotonPlayer.Find (2) != null)
			{
				player2 = PhotonPlayer.Find (2);
				Debug.Log ("Player 2 set!***");
			}
			if(PhotonPlayer.Find (3) != null)
			{
				player3 = PhotonPlayer.Find (3);
				Debug.Log ("Player 3 set!***");
			}
			if(PhotonPlayer.Find (4) != null)
			{
				player4 = PhotonPlayer.Find (4);
				Debug.Log ("Player 4 set!***");
			}

			if(!notInstantiated)
				allPlayers = GameObject.FindGameObjectsWithTag("Player");

		}
		if(timer1 <= Time.time)
		{
			timer1 += 1.0f;

			if(player1 != null)
			{
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
				//allPlayers = GameObject.FindGameObjectsWithTag("Player");
				checkPlayer(player3);
			}
			if(player4 != null)
			{
				//allPlayers = GameObject.FindGameObjectsWithTag("Player");
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
				else if(((int)currPlayer.GetPhotonView().owner.customProperties["Dead"] == 1) && (currPlayer.GetPhotonView().owner == player) && (currPlayer.GetComponent<HealthSync>().activateRespawn == true) && respawn == true && photonView.isMine)
				{
					//Perform necessary steps for respawning player.
					StartCoroutine (getSpawnPoint(player));

					//photonView.RPC ("RespawnThePlayer",player);



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
			respawn = false;

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
			hash2.Add("Kills", (int)PhotonNetwork.player.customProperties["Kills"]);
			hash2.Add("Deaths",(int)PhotonNetwork.player.customProperties["Deaths"]+1);
			hash2.Add("Assist",(int)PhotonNetwork.player.customProperties["Assist"]);
			hash2.Add("Health",100);
			PhotonNetwork.player.SetCustomProperties(hash2);
	}

	[RPC]
	void RemoveNetworkTrash(int viewID)
	{
		//Destroy (currPlayer.gameObject);
		foreach(GameObject eachMofo in allPlayers)
		{
			if(eachMofo.GetPhotonView().viewID == viewID)
				Destroy (eachMofo.gameObject);
		}
		//Destroy (allPlayers [objIndex].gameObject);
		
		GameObject[] trash = GameObject.FindGameObjectsWithTag("Trash");
		foreach(GameObject currTrash in trash)
		{
			Destroy(currTrash);
		}
	}
	
	IEnumerator getSpawnPoint(PhotonPlayer thePlayer)
	{
		if(photonView.isMine){
			bool goodSpawn = false;
			position = new Vector3 (Random.Range (140, 1230), 200.0f, Random.Range (-315, 580));
			while(!goodSpawn)
			{
				if(checkSpawn(position))
				{
					Debug.Log ("FOUND A GOOD POSITION******");
					goodSpawn = true;
				}else{
					position = new Vector3 (Random.Range (140, 1230), 200.0f, Random.Range (-315, 580));
					Debug.Log ("CHECKING FOR A GOOD POSITION*********");
				}
			}
			yield return new WaitForSeconds(1.0f);
			photonView.RPC ("RespawnThePlayer",thePlayer,position);
		}
	}

	IEnumerator waitFiveSeconds()
	{
		if(photonView.isMine)
		{
			yield return new WaitForSeconds (5.0f);
			respawn = true;
		}
	}

	//public GameObject sphere;
	bool checkSpawn(Vector3 pos)
	{
		RaycastHit hit;

		Vector3 positionCheck = new Vector3(pos.x,200f,pos.z);
		if (Physics.Raycast(positionCheck, Vector3.down, out hit,250f))
		{
			Debug.DrawRay(positionCheck, Vector3.down * hit.distance, Color.blue, 200f);
			if(hit.collider.gameObject.tag == "Terrain"){
				Collider[] hitColliders = Physics.OverlapSphere(hit.point, 100.0f);
				Debug.Log(hitColliders.Length);

				if(hitColliders.Length == 1){
					if(hitColliders[0].tag == "Terrain"){
						Debug.Log("Ready to spawn");
						return true;
					}else{
						return false;
					}
				}
				else
				{
					foreach (Collider collider in hitColliders)
					{
						Debug.Log(collider.gameObject.name);
					}
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		return false;
	}
}
