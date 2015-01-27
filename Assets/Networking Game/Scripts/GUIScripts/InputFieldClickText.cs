using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon;
public class InputFieldClickText : PunBehaviour {
    public string currentPlaceholder;
    public GameObject OverallGameNameObject;
	// Use this for initialization
	void Start () {
        currentPlaceholder = GetComponent<Text>().text;
	}
	
	// Update is called once per frame
	void Update () {
        if(PhotonNetwork.connected){
            if (EventSystem.current.currentSelectedGameObject == OverallGameNameObject)
            {
                GetComponent<Text>().text = "";
            }
            else
            {
                GetComponent<Text>().text = currentPlaceholder;
            }
        }
	}
}
