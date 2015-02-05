using UnityEngine;
using System.Collections;

public class TankGunColliders : MonoBehaviour {

	public GameObject mainGun;
	public GameObject barrel;
	public GameObject barrelCollider;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		transform.localRotation = mainGun.transform.localRotation;

		barrelCollider.transform.position = barrel.transform.position;
		barrelCollider.transform.rotation = barrel.transform.rotation;

	}
}
