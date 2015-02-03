using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using System;
using ExitGames.Client.Photon;
using System.Collections.Generic;
public class addListener : MonoBehaviour {
    Start_Menu_Server_Check networkCheck;
    public List<GameObject> buttons;
    public PhotonView view;
    void Start()
    {
        view = GameObject.Find("NetworkManager").GetComponent<PhotonView>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("DropDownButton");
        foreach(GameObject obj in objs){
            buttons.Add(obj);
        }
        networkCheck = FindObjectOfType<Start_Menu_Server_Check>();
        gameObject.GetComponent<Button>().onClick.AddListener(delegate {  view.RPC("ChangeColor",PhotonTargets.AllBuffered);});
    }

    void OnClick()
    {
        
    }
}
