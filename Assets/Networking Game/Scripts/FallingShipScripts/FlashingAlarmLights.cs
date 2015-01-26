using UnityEngine;
using System.Collections;

public class FlashingAlarmLights : MonoBehaviour {
    Light pointLight;
    bool increaseLight = false;
	// Use this for initialization
	void Start () {
        pointLight = GetComponent<Light>();
	}
	
	// Update is called once per frame
	void Update () {
        if(pointLight.intensity >= 0 && !increaseLight){
            pointLight.intensity -= .03f;
        }
        if(pointLight.intensity <= 3 && increaseLight){
            pointLight.intensity += .03f;
        }

        if (pointLight.intensity <= 0)
        {
            increaseLight = true;
        }
        if(pointLight.intensity >= 3)
        {
            increaseLight = false;
        }
	}
}
