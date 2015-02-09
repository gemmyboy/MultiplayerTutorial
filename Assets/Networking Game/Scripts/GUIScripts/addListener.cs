using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using System;
using ExitGames.Client.Photon;
using System.Collections.Generic;
public class addListener : MonoBehaviour {
    Start_Menu_Server_Check start_Menu;
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => ChangeButtonColor());
    }

    public void ChangeButtonColor()
    {
        start_Menu = FindObjectOfType<Start_Menu_Server_Check>();
        start_Menu.AlertColorChange(gameObject,start_Menu.myLabelViewID);
    }
}
