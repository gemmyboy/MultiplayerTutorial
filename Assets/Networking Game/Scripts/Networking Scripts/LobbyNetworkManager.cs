using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using System;
using ExitGames.Client.Photon;
using System.Linq;
public class LobbyNetworkManager : Photon.MonoBehaviour {

    public GameObject dropDownMenu;
    public int previousPlayerAmount = 0;
    int i = 0;

    void Start()
    {
        GameObject label = PhotonNetwork.Instantiate("GameLabel", gameObject.transform.position, Quaternion.identity, 0);
        int view = label.GetPhotonView().viewID;
        photonView.RPC("fixLabel", PhotonTargets.AllBuffered, new object[] {PhotonNetwork.player,view});
        if (PhotonNetwork.player.isMasterClient)
        {
            createDropDownMenu();
        }
    }
    void Update()
    {
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //To fix the label for the network
    GameObject obj;
    [RPC]
    public void fixLabel(PhotonPlayer player,int view)
    {
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach(PhotonView vie in views){
            if(vie.viewID == view){
                GameObject obj = vie.gameObject;
                obj.transform.parent = gameObject.transform;
                obj.transform.localScale = new Vector3(1, 1, 1);
                obj.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, (-50 * i) + 120, 0);
                obj.transform.rotation = new Quaternion(0, 0, 0, 0);
                if (player.isMasterClient)
                {
                    obj.GetComponentInChildren<Text>().text = (i+1) + ". " + PhotonNetwork.player.name + " - Master";
                }
                else
                {
                    obj.GetComponentInChildren<Text>().text = (i+1) + ". " + PhotonNetwork.player.name + " - Client";
                }
                i++;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //Making the drop down menu
    GameObject dropMenu;
    public void createDropDownMenu()
    {
        dropMenu = PhotonNetwork.InstantiateSceneObject("DropDownButtonMenu", gameObject.transform.position, Quaternion.identity, 0, null);
        photonView.RPC("fixDropDown", PhotonTargets.AllBuffered);
    }
    [RPC]
    void fixDropDown()
    {
        dropMenu = GameObject.Find("DropDownButtonMenu(Clone)");
        dropMenu.transform.parent = gameObject.transform;
        dropMenu.transform.localScale = new Vector3(1, 1, 1);
        dropMenu.transform.rotation = new Quaternion(0, 0, 0, 0);
        dropMenu.GetComponentInChildren<RectTransform>().localPosition = new Vector3(450, 100, 0);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //Deleting the labels
    public void DeleteLabels()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Server_Label");
        if(objects == null){
            return;
        }
        i = 0;
        foreach(GameObject obj in objects){
            Destroy(obj);
        }
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        previousPlayerAmount = 0;
    }
}

