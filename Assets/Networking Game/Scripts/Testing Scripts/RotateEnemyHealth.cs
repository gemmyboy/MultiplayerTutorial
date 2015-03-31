using UnityEngine;
using System.Collections;

public class RotateEnemyHealth : MonoBehaviour {
	
	private GameObject[] tanks;
	private GameObject currentTankHealth;
	void Start () {
		tanks = GameObject.FindGameObjectsWithTag("Player");
		currentTankHealth = null;
	}
	
	// Update is called once per frame
	void Update () {
		foreach(GameObject tank in tanks){
			if(tank.transform.Find("TankHealthSystem").gameObject != null)
			{
				currentTankHealth = (GameObject)tank.transform.Find("TankHealthSystem").gameObject;
			}
			if(currentTankHealth != null)
			{
				currentTankHealth.transform.LookAt(Camera.main.transform);
			}
		}
	}
}
