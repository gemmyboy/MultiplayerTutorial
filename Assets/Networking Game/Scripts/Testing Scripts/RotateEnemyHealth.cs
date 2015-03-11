using UnityEngine;
using System.Collections;

public class RotateEnemyHealth : MonoBehaviour {
    GameObject[] tanks;
	// Use this for initialization
	void Start () {
        tanks = GameObject.FindGameObjectsWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
	    foreach(GameObject tank in tanks){
            tank.transform.Find("TankHealthSystemCanvas").LookAt(transform);
        }
	}
}
