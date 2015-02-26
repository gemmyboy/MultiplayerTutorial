using UnityEngine;
using System.Collections;

public class RotateAroundScript : MonoBehaviour {
    public GameObject target;
    public float positionOffsetx;
    public float positionOffsety;
    public float positionOffsetz;
	// Use this for initialization
	void Start () {
        transform.LookAt(target.transform);
        transform.position = new Vector3(target.transform.position.x + positionOffsetx, target.transform.position.y + positionOffsety, target.transform.position.z + positionOffsetz);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.position = new Vector3(target.transform.position.x + positionOffsetx, target.transform.position.y + positionOffsety, target.transform.position.z + positionOffsetz);
        //transform.RotateAround(Vector3.up, Vector3.up, 20 * Time.deltaTime);
        transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
	}
}
