using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour {
    public GameObject pod;
    public GameObject spawning;
    public GameObject podddd;
	// Use this for initialization
	void Start () {
	    pod = GameObject.Find("Pod(Clone)");
	}
	
	// Update is called once per frame
	void Update () {
        if(pod != null){
            transform.LookAt(pod.transform);
        }
	}
    
}
