using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Flash_Color : MonoBehaviour {
    public float timer = .30f;
    protected float TotalTime;
    public Vector3 newVector;
    protected Color newColor;
    void Start(){
        TotalTime = Time.time + timer;
    }
	void Update () {
        if(Time.time > TotalTime){
            newVector = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            newColor = new Color(newVector.x, newVector.y, newVector.z, 1);
            //Change color of text
            if (gameObject.tag == "Flashing_Text")
            {
                GetComponent<Text>().color = newColor;
            }

            if(gameObject.tag == "Flashing_Window"){
                GetComponent<Image>().color = newColor;
            }
            TotalTime = Time.time + timer;
        }
	}
}
