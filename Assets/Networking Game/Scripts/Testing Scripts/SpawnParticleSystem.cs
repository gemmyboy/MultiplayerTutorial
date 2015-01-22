using UnityEngine;
using System.Collections;
using Photon;
public class SpawnParticleSystem : PunBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(photonView.isMine){
            if (Input.GetKeyDown(KeyCode.R))
            {
                PhotonNetwork.Instantiate("ExplosionSmoke1", transform.position, transform.rotation, 0);
            }
        }
	}
}
