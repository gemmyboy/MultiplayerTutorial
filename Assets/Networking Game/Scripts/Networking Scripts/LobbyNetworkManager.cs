using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using System;
using ExitGames.Client.Photon;
using System.Linq;
public class LobbyNetworkManager : Photon.MonoBehaviour {

    GameObject label;
    public bool gameStarted = false;
    public int previousPlayerAmount = 0;
    int i = 0;

    void Update()
    {
        if(PhotonNetwork.room != null){
            if (PhotonNetwork.room.playerCount > previousPlayerAmount)
            {
                DeleteLabels();
                foreach(PhotonPlayer player in  PhotonNetwork.playerList){
                    GameObject newPlayerObject = PhotonNetwork.Instantiate("GameLabel", gameObject.transform.position, Quaternion.identity, 0);
                    label = newPlayerObject;
                    previousPlayerAmount += 1;
                    i++;
                    fixLabel(player);
                }
            }
        }
    }

    [RPC]
    public void fixLabel(PhotonPlayer player)
    {
        label.transform.parent = gameObject.transform;
        label.transform.localScale = new Vector3(1, 1, 1);
        label.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, (-50 * i) + 140, 0);
        label.transform.rotation = new Quaternion(0, 0, 0, 0);
        if(player.isMasterClient){
            label.GetComponentInChildren<Text>().text = i + ". " + player.name + " - Master";
        }else{
            label.GetComponentInChildren<Text>().text = i + ". " + player.name + " - Client";
        }

    }

    public void DeleteLabels()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Respawn");
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

