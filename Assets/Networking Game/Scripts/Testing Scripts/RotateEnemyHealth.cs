using UnityEngine;
using System.Collections;

public class RotateEnemyHealth : MonoBehaviour {
	
	private GameObject[] tanks;
	private GameObject currentTankHealth;
	private GameObject tempObj;
	private float timer;
	System.Collections.Generic.List<GameObject> myTanks;
	void Start () {
		tanks = GameObject.FindGameObjectsWithTag("Player");
		currentTankHealth = null;
		timer = Time.time;
		tempObj = new GameObject();
		myTanks = new System.Collections.Generic.List<GameObject>(tanks);
	}
	
	// Update is called once per frame
	void Update () {
		if(timer <= Time.time)
		{
			tanks = GameObject.FindGameObjectsWithTag("Player");

			foreach(GameObject tank in tanks){
				if(!myTanks.Contains(tank))
				{
					myTanks.Add (tank);
				}
				if(tank.GetComponent<HealthSync>().dead == true)
				{
					//myTanks = new System.Collections.Generic.List<GameObject>(tanks);
					myTanks.Remove(tank);
					tanks = myTanks.ToArray ();
				}
				else if(tank.GetComponent<HealthSync>().dead == false)
				{
					//tempObj = tank.transform.Find("TankHealthSystem").parent.gameObject;
					if(tank.transform.Find ("TankHealthSystem").gameObject != null)
					{
						currentTankHealth = (GameObject)tank.transform.Find("TankHealthSystem").gameObject;
					}
					if(currentTankHealth != null)
					{
						currentTankHealth.transform.LookAt(Camera.main.transform);
						currentTankHealth = null;
					}
				}
				timer = Time.time+0.1f;
			}
		}
	}
}
