using UnityEngine;
using System.Collections;

public class SpawnLaserForOmega : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject laser = PhotonNetwork.Instantiate("Omega_Direction_Show", transform.position, transform.rotation, 0);
        laser.transform.SetParent(gameObject.transform);
	}
}
