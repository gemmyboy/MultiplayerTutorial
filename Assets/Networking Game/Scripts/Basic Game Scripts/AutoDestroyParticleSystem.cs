using UnityEngine;
using System.Collections;

public class AutoDestroyParticleSystem : MonoBehaviour {
    public ParticleSystem particleSystem;
	// Use this for initialization
	void Start () {
        particleSystem = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
	    if(particleSystem.isStopped){
            Destroy(particleSystem.gameObject);
        }
	}
}
