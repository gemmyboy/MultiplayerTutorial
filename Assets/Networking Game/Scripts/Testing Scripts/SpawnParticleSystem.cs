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
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        }
	}
}
