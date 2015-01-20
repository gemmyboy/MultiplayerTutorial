using UnityEngine;
using System.Collections;

public class BulletMotion : MonoBehaviour {
    //Movement Speed of the Bullet
    public float movementSpeed = 25.0f;
    //Range of the bullet
    public float range = 15.0f;

    //Timer for bullet
    float timer = 0.0f;
	// Use this for initialization
	void Start () {
        timer = range / movementSpeed;
        rigidbody.velocity = transform.forward * movementSpeed;
	}
	
	// Update is called once per frame
	void Update () {
        timer -= Time.deltaTime;
        if (timer < 0.0f)
        {
            Destroy(gameObject);
        }
	}
}
