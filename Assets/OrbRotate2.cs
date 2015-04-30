using UnityEngine;
using System.Collections;

public class OrbRotate2 : MonoBehaviour {
    public GameObject target;
    public float smooth;
    public float angle;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, target.transform.position, Time.deltaTime * smooth);
        //transform.RotateAround(target.transform.position, angle);
	}
}
