using UnityEngine;
using System.Collections;
using Photon;
public class AutoDestroyParticleSystem : PunBehaviour {
    public ParticleSystem particleSystem;
	// Use this for initialization
	void Start () {
        particleSystem = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
	    if(particleSystem.isStopped){
            if(photonView.isMine){
                if (particleSystem.GetComponent<PhotonView>() != null)
                {
                    Debug.Log("Online particle");
                    PhotonNetwork.Destroy(particleSystem.gameObject);
                }
                else
                {
                    Debug.Log("Just particle");
                    Destroy(particleSystem.gameObject);
                }
            }
        }
	}
}
