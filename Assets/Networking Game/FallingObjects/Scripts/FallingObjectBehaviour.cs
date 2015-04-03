using UnityEngine;
using System.Collections;

public class FallingObjectBehaviour : MonoBehaviour {

	GameObject myself;
    private float lifeTime;
    public int lifeTimeOfTheBullet = 10;
    public GameObject explosion;
	// Use this for initialization
	void Start () 
	{
		myself = gameObject;
	}//End Start()

	void OnCollisionEnter(Collision whoIHit)
	{
		//Particle Effect here!
		//******
        Instantiate(explosion, transform.position, transform.rotation);
		//Decrement so another can be spawned
		FallingObjectGenerator.spawned--;
		Destroy(myself);
	}//End OnCollisionEnter()


	// Update is called once per frame
	void Update () 
	{
        lifeTime += Time.deltaTime;
        if (gameObject.activeSelf && lifeTime > lifeTimeOfTheBullet)
        {
            FallingObjectGenerator.spawned--;
            Instantiate(explosion,transform.position,transform.rotation);
            Destroy(myself);
        }
	}//End Update()
}//End FallingObjectBehaviour
