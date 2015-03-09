using UnityEngine;
using System.Collections;

public class RotateEnemyHealth : MonoBehaviour {
    GameObject[] tanks;
    public GameObject health;
	// Use this for initialization
	void Start () {
        tanks = GameObject.FindGameObjectsWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
	    foreach(GameObject tank in tanks){
            Debug.Log(tank.name,tank.transform.Find("TankHealthSystemCanvas"));
            tank.transform.Find("TankHealthSystemCanvas").LookAt(transform);
        }
	}
}
