using UnityEngine;
using System.Collections;

public class WSP_TargetMover : MonoBehaviour {

	private Transform myTransform;
	public float MoveSpeed = 2;

	public bool MovingForward = true;

	// Use this for initialization
	void Start () {
		myTransform = gameObject.transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (MovingForward) {
			if (myTransform.position.z < 10) {
				myTransform.Translate(Vector3.forward * (MoveSpeed * Time.deltaTime));
			}
			else {
				MovingForward = false;
			}
		}
		else {
			if (myTransform.position.z > -10) {
				myTransform.Translate(-Vector3.forward * (MoveSpeed * Time.deltaTime));
			}
			else {
				MovingForward = true;
			}
		}
	}
}
