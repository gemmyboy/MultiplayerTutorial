using UnityEngine;
using System.Collections;

public class RespawnScript : Photon.MonoBehaviour {

	private PhotonPlayer player1;
	private PhotonPlayer player2;
	private PhotonPlayer player3;
	private PhotonPlayer player4;
	private GameObject[] allPlayers;

	private Vector3 spawnPoint;

	private bool notInstantiated;

	private float timer1;
	// Use this for initialization
	void Start () 
	{
		notInstantiated = true;
		timer1 = Time.time;
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
				//allPlayers = GameObject.FindGameObjectsWithTag("Player");
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

	void checkPlayer(PhotonPlayer player)
	{
		if(player != null)
		{
			foreach(GameObject currPlayer in allPlayers)
			{
				if(currPlayer.GetComponent<HealthSync>() != null)
				{
					Debug.Log("******ATLEAST GOT HERE******");
					if((int)player.customProperties["Dead"] == 1 && (currPlayer.GetComponent<HealthSync>().activateRespawn == true))
					{
						Debug.Log ("****** PERFORM RESPAWN SHIT ******");
						//Perform necessary steps for respawning player.
					}
				}
			}
		}
	}

	public GameObject sphere;
	bool checkSpawn(Vector3 pos)
	{
		RaycastHit hit;
		Vector3 positionCheck = new Vector3(pos.x,300,pos.z);
		if (Physics.Raycast(positionCheck, Vector3.down, out hit, 250.0f))
		{
			Debug.DrawRay(positionCheck, Vector3.down * hit.distance, Color.blue, 200.0f);
			if(hit.collider.gameObject.tag == "Terrain"){
				Collider[] hitColliders = Physics.OverlapSphere(hit.point, 100.0f);
				Debug.Log(hitColliders.Length);
				if (hitColliders.Length == 0)
				{
					spawnPoint = hit.point + new Vector3(0,10,0);
					return true;
				}
				else if(hitColliders.Length == 1){
					if(hitColliders[0].tag == "Terrain"){
						Debug.Log("Ready to spawn");
						spawnPoint = hit.point + new Vector3(0, 15, 0);
						return true;
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
