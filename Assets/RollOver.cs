using UnityEngine;
using System.Collections;

public class RollOver : MonoBehaviour {
    public float countTimer = 0.0f;
    public float TotalTime = 5.0f;
    public GameObject tank;
    public bool readyToLerp = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 up = transform.TransformDirection(Vector3.up);
        if(Physics.Raycast(transform.position,up,10)){
            countTimer += Time.deltaTime;
            if (countTimer >= TotalTime && !readyToLerp)
            {
                StartCoroutine("rotate");
                readyToLerp = true;
            }
        }
        else
        {
            countTimer = 0;
        }
        if (readyToLerp)
        {
            transform.position = Vector3.Lerp(transform.position, transform.position + new Vector3(0, 20, 0), Time.deltaTime);
            countTimer = 0;
        }
	}


    IEnumerator rotate()
    {
        yield return new WaitForSeconds(2.0f);
        transform.rotation = new Quaternion(0, 0, 0, 0);
        readyToLerp = false;
    }
}
