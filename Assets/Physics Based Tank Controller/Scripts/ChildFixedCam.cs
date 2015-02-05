using UnityEngine;
using System.Collections;

public class ChildFixedCam : MonoBehaviour {

	[HideInInspector]
	public Transform player;
	public bool freeCam = false;

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(!freeCam)
			transform.LookAt(player);
	}
}
