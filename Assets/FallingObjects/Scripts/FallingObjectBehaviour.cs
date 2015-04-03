using UnityEngine;
using System.Collections;

public class FallingObjectBehaviour : MonoBehaviour {

	GameObject myself;

	// Use this for initialization
	void Start () 
	{
		myself = gameObject;
	}//End Start()

	void OnCollisionEnter(Collision whoIHit)
	{
		//Particle Effect here!
		//******

		//Decrement so another can be spawned
		FallingObjectGenerator.spawned--;
		Destroy(myself);
	}//End OnCollisionEnter()


	// Update is called once per frame
	void Update () 
	{

	}//End Update()
}//End FallingObjectBehaviour
