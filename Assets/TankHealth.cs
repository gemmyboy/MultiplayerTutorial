using UnityEngine;
using System.Collections;

public class TankHealth : MonoBehaviour {

	public int health;
	//public GameObject healthBar;


	// Use this for initialization
	void Start () {
		//transform.LookAt ();
	    
	}
	
	// Update is called once per frame
	void Update () {
		//transform.localRotation = healthBar.transform.localRotation;
	}

    //void OnTriggerEnter(Collider other) {
    //    Debug.Log ("CollisionDetected");
    //    if (other.gameObject.tag == "TankShell") {
    //        health = health - 20;
    //        transform.localScale += new Vector3(-2.8F, 0, 0);

    //    }

    //    if (health < 15) 
    //        renderer.material.color = Color.red;
		    
    //    if (health < 0)
    //        Destroy (gameObject); // Destroy parent which is the tank or handle whatever happens to the tank once it runs out of health	
    //}

    void OnCollisionEnter(Collision coll)
    {
        Debug.Log("Collision Detected");
    }
}

