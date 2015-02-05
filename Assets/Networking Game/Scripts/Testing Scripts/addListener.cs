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

    public PhotonView otherView;
    PhotonView myView;

    void Start()
    {
        otherView = GameObject.Find("NetworkManager").GetComponent<PhotonView>();
    }
    public void setUp()
    {
        ////set up variables
        //networkCheck = FindObjectOfType<Start_Menu_Server_Check>();
        //myView = GetComponent<PhotonView>();

        ////Get the button view
        //myButtonViewID = myView.viewID;

        ////find the label viewID we want to change and send it to the RPC function in Start_Menu_Server_Check
        //PhotonView[] views = FindObjectsOfType<PhotonView>();
        //foreach (PhotonView vie in views)
        //{
        //    //Debug.Log(vie.name);
        //    if (vie.isMine && vie.name == "GameLabel(Clone)")
        //    {
        //        myLabelViewID = vie.viewID;
        //    }
        //}

    }

    public void onClick()
    {
        Debug.Log(gameObject.GetComponent<PhotonView>().viewID);
        //Send the message to change the color of that shit
        //otherView.RPC("ChangeColor", PhotonTargets.AllBuffered,GetComponent<PhotonView>().viewID);
        //we need to close that menu yo
        gameObject.SendMessageUpwards("CloseMenu",SendMessageOptions.RequireReceiver);
        GetComponent<Animator>().enabled = false;
    }
}
