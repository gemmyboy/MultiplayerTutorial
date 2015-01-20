using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class InputFieldClickText : MonoBehaviour {
    public string currentPlaceholder;
    public GameObject OverallGameNameObject;
	// Use this for initialization
	void Start () {
        currentPlaceholder = GetComponent<Text>().text;
	}
	
	// Update is called once per frame
	void Update () {
	    if(EventSystem.current.currentSelectedGameObject == OverallGameNameObject){
            GetComponent<Text>().text = "";
        }
        else
        {
            GetComponent<Text>().text = currentPlaceholder;
        }
	}
}
