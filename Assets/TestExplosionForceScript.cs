using UnityEngine;
using System.Collections;

public class TestExplosionForceScript : MonoBehaviour {

	public float power;
	public float radius;
	// Use this for initialization
	void Start () {
		power = 30000.0f;
		radius = 10.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.B))
		{
			transform.rigidbody.AddExplosionForce(power,transform.position,radius,0.0f,ForceMode.Impulse);
		}
	}
}
