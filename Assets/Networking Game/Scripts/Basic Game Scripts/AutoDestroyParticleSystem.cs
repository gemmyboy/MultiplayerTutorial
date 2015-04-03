using UnityEngine;
using System.Collections;
using Photon;
public class AutoDestroyParticleSystem : PunBehaviour {
    public ParticleSystem particleSystem;
    public ParticleEmitter particleEmiter;
	// Use this for initialization
	void Start () {
        particleEmiter = GetComponent<ParticleEmitter>();
        particleSystem = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
        if(particleEmitter == null){
	        if(particleSystem.isStopped){
                if (particleSystem.GetComponent<PhotonView>() != null)
                {
                    if (photonView.isMine)
                    {
                        Debug.Log("Online particle");
                        PhotonNetwork.Destroy(particleSystem.gameObject);
                    }
                }
                else
                {
                    Debug.Log("Just particle");
                    Destroy(particleSystem.gameObject);
                }
            }
        }
        else
        {
            if (particleEmiter.emit != true)
            {
                if (photonView.isMine)
                {
                    if (particleEmiter.GetComponent<PhotonView>() != null)
                    {
                        Debug.Log("Online particle");
                        PhotonNetwork.Destroy(particleEmiter.gameObject);
                    }
                    else
                    {
                        Debug.Log("Just particle");
                        Destroy(particleEmiter.gameObject);
                    }
                }
            }
        }
	}
}
