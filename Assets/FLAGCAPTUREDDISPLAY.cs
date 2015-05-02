using UnityEngine;
using System.Collections;

public class FLAGCAPTUREDDISPLAY : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        StartCoroutine("");
	}

    IEnumerator liftOff()
    {
        yield return new WaitForSeconds(2.0f);
        GetComponent<ParticleSystem>();
    }
}
