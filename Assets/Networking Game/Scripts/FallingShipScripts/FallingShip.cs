using UnityEngine;
using System.Collections;

public class FallingShip : MonoBehaviour {
    public float downwardVelocity;

    public AudioClip crash;
    public AudioSource[] sources;

    public ParticleSystem rushingAir;
    public ParticleSystem explosionAir;
    public ParticleSystem shockWave;

    public bool Crashed = false;
	void Start () {
        rigidbody.AddForce(Vector3.down*downwardVelocity,ForceMode.VelocityChange);
        Instantiate(shockWave,transform.position + Vector3.up * 10,transform.rotation);
	}

    void Update()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 50) && !Crashed)
        {
            Crashed = true;
        }
            
    }
    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Terrain"){
            rigidbody.isKinematic = true;
            transform.parent = col.transform;
            StopAllAudio();
            gameObject.AddComponent<AudioSource>().PlayOneShot(crash);
            Instantiate(explosionAir,transform.position,transform.rotation);
        }
    }

    void StopAllAudio()
    {
        sources = gameObject.GetComponents<AudioSource>();
        foreach(AudioSource source in sources){
            Destroy(source);
        }
    }
}
