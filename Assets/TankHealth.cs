using UnityEngine;
using System.Collections;

public class TankHealth : Photon.MonoBehaviour {

	public int health;
    UIManager uiManager;
	//public GameObject healthBar;

    void Start()
    {
        if (!photonView.isMine)
        {
            this.enabled = false;
        }
        health = 100;
        uiManager = FindObjectOfType<UIManager>();
        uiManager.ChangeHealth(health);
    }

    public void TakeDamage()
    {
        Debug.Log("TankBullet Hit");
        health -= 20;
        uiManager.ChangeHealth(health);
        Mathf.Clamp(health, 0, 100);
        //transform.localScale += new Vector3(Mathf.Lerp(transform.localScale.x, transform.localScale.x - .25f, Time.deltaTime), 0, 0);

        if (health <= 0)
        {
            //Destroy(gameObject); // Destroy parent which is the tank or handle whatever happens to the tank once it runs out of health	
        }
    }
    //void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("CollisionDetected");
    //    if (other.gameObject.tag == "TankShell")
    //    {
    //        Debug.Log("TankBullet Hit");
    //        health -= 20;
    //        uiManager.ChangeHealth(health);
    //        Mathf.Clamp(health, 0, 100);
    //        //transform.localScale += new Vector3(Mathf.Lerp(transform.localScale.x, transform.localScale.x - .25f, Time.deltaTime), 0, 0);
    //    }

    //    if (health <= 0)
    //    {
    //        //Destroy(gameObject); // Destroy parent which is the tank or handle whatever happens to the tank once it runs out of health	
    //    }
    //}
}

