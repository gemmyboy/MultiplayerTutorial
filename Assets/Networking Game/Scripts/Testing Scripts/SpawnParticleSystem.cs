using UnityEngine;
using System.Collections;
using Photon;
public class SpawnParticleSystem : PunBehaviour {
    public AudioClip sound;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(photonView.isMine){
            if (Input.GetKeyDown(KeyCode.R))
            {
                PhotonNetwork.Instantiate("ExplosionSmoke1", transform.position, transform.rotation, 0);
                audio.PlayOneShot(sound);
            }
            Ray ray = camera.ScreenPointToRay(new Vector3(200, 200, 0));
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        }
	}
}
