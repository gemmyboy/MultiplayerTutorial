using UnityEngine;
using System.Collections;

public class TankHealth : MonoBehaviour {

	public int health;
	//public GameObject healthBar;


    void OnTriggerEnter(Collider other)
    {
        Debug.Log("CollisionDetected");
        if (other.gameObject.tag == "TankShell")
        {
            health -= 20;
            transform.localScale += new Vector3(Mathf.Lerp(transform.localScale.x, transform.localScale.x - .25f, Time.deltaTime), 0, 0);
            Mathf.Clamp(health, 0, 100);
        }

        if (health <= 15)
            renderer.material.color = Color.red;

        if (health <= 0)
        {
            //Destroy(gameObject); // Destroy parent which is the tank or handle whatever happens to the tank once it runs out of health	

        }
    }
}

