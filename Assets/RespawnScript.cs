using UnityEngine;
using System.Collections;

public class RespawnScript : MonoBehaviour {

	private Vector3 spawnPoint;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public GameObject sphere;
	bool checkSpawn(Transform pos)
	{
		RaycastHit hit;
		Vector3 positionCheck = new Vector3(pos.position.x,300,pos.position.z);
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
