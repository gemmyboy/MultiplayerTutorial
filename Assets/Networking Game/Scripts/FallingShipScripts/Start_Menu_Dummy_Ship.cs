using UnityEngine;
using System.Collections;

public class Start_Menu_Dummy_Ship : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(WaitToLaunch());
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
    IEnumerator WaitToLaunch()
    {

        yield return new WaitForSeconds(0.5f);
        rigidbody.AddForce(Vector3.down * 10, ForceMode.VelocityChange);
        gameObject.GetComponentInChildren<ParticleSystem>().enableEmission = true;
    }
}
