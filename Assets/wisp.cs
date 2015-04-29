using UnityEngine;
using System.Collections;

public class wisp : MonoBehaviour {

    public bool floatup;
    public float yvalue;
    public float xvalue;
    public float zvalue;
    void Start()
    {
        floatup = false;
        //xvalue = Random.Range(1, 5);
        yvalue = Random.Range(3, 5);
        //zvalue = Random.Range(1, 5);
    }
    void LateUpdate()
    {
        if (floatup)
            StartCoroutine(floatingup());
        else if (!floatup)
            StartCoroutine(floatingdown());
    }
    IEnumerator floatingup(){
        transform.position += new Vector3(xvalue * Time.deltaTime, yvalue * Time.deltaTime, zvalue * Time.deltaTime);
        yield return new  WaitForSeconds(3);
        floatup = false;
    }
    IEnumerator floatingdown(){
        transform.position -= new Vector3(xvalue * Time.deltaTime, yvalue * Time.deltaTime, zvalue * Time.deltaTime);
        yield return new WaitForSeconds(3);
        floatup = true;
 }
}
