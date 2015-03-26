using UnityEngine;
using System.Collections;
using Photon;
using System.Collections.Generic;
public class NetworkManager : PunBehaviour
{
    #region PhotonConnect&Disconnect
    public void Awake()
    {
        // in case we started this demo with the wrong scene being active, simply load the menu scene
        if (!PhotonNetwork.connected)
        {
            Application.LoadLevel(Start_Menu_Server_Check.SceneNameMenu);
            return;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Handling Photons Networking
    public void OnMasterClientSwitched(PhotonPlayer player)
    {
        Debug.Log("OnMasterClientSwitched: " + player);

        string message;
        InRoomChat chatComponent = GetComponent<InRoomChat>();  // if we find a InRoomChat component, we print out a short message

        if (chatComponent != null)
        {
            // to check if this client is the new master...
            if (player.isLocal)
            {
                message = "You are Master Client now.";
            }
            else
            {
                message = player.name + " is Master Client now.";
            }


            chatComponent.AddLine(message); // the Chat method is a RPC. as we don't want to send an RPC and neither create a PhotonMessageInfo, lets call AddLine()
        }
    }

    public void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom (local)");

        string message;
        InRoomChat chatComponent = GetComponent<InRoomChat>();  // if we find a InRoomChat component, we print out a short message
        message = PhotonNetwork.player + " has left the game!";
        if(PhotonNetwork.player.isLocal == false){
            chatComponent.AddLine(message);
        }
        // back to main menu        
        Application.LoadLevel(Start_Menu_Server_Check.SceneNameMenu);
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("OnDisconnectedFromPhoton");

        // back to main menu        
        Application.LoadLevel(Start_Menu_Server_Check.SceneNameMenu);
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPlayerDisconneced: " + player);
    }

    public void OnFailedToConnectToPhoton()
    {
        Debug.Log("OnFailedToConnectToPhoton");

        // back to main menu        
        Application.LoadLevel(Start_Menu_Server_Check.SceneNameMenu);
    }
    #endregion
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Photon
    MouseOrbitC orbit;
    TankGunController tankGun;
    GameObject Target;
    MeshRenderer[] meshes;
    void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        {
            CreatePlayerObject();
        }
    }
    private void CreatePlayerObject()
    {
        //OMEGA TANK BABY
        //--------------------------------------------------------------------------------------------------------------
        if(PhotonNetwork.room.customProperties["GameType"].ToString() == "Omega Tank" && PhotonNetwork.isMasterClient){
            Random.seed = Mathf.RoundToInt(Time.time * 100);
            Debug.Log(Random.seed = Mathf.RoundToInt(Time.time * 100));
            //Choose random omega and its faction
            int randomOmega = Random.Range(0, PhotonNetwork.playerList.Length);
            int randomFaction = Random.Range(0, 3);

            //Make the things we need
            List<string> teams = new List<string>() {"Eagles", "Excorcist", "Wolves", "Angel" };
            PhotonPlayer[] players = PhotonNetwork.playerList;
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            PhotonPlayer theOmega = players[randomOmega];

            //Add it and secure the baby boys
            hash.Add("Team", teams[randomFaction]);
            theOmega.SetCustomProperties(hash);

            //now to make a team for the other players
            teams.Remove(teams[randomFaction]);
            randomFaction = Random.Range(0, 2);

            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if(player != theOmega){
                    hash = new ExitGames.Client.Photon.Hashtable();
                    hash.Add("Team", teams[randomFaction]);
                    player.SetCustomProperties(hash);
                }
            }
        }
        //--------------------------------------------------------------------------------------------------------------
        //NWAPS STNIOP YBAB ----spawn points baby
        createSpawns();
        //--------------------------------------------------------------------------------------------------------------

        //Instanitate Tank
        GameObject newPlayerObject = PhotonNetwork.Instantiate("T-90_Prefab_Network", spawnPoint, Quaternion.identity, 0);
        //Add the camera target
        orbit = FindObjectOfType<MouseOrbitC>();
        //add the tankgun target
        tankGun = newPlayerObject.GetComponentInChildren<TankGunController>();
        Target = GameObject.Find("Target");
        orbit.target = newPlayerObject.transform;
        tankGun.target = Target.transform;
        //Turn off own health system
		Transform TankHealthSystem = (Transform)newPlayerObject.transform.Find ("TankHealthSystem").FindChild ("TankHealthSystemCanvas");
        TankHealthSystem.gameObject.SetActive(false);
        //--------------------------------------------------------------------------------------------------------------

        //Setup scoring system
        ExitGames.Client.Photon.Hashtable hash2 = new ExitGames.Client.Photon.Hashtable();
        if(PhotonNetwork.room.customProperties["GameType"] != "Free For All"){
            hash2.Add("TeamScore",0);
            PhotonNetwork.player.SetScore(0);
        }
        hash2.Add("Kills", 0);
        hash2.Add("Deaths",0);
        hash2.Add("Assist",0);
        hash2.Add("Health",100);
        PhotonNetwork.player.SetCustomProperties(hash2);
    }

    public GameObject spawnpointsFFA;
    public GameObject spawnpointsCTF;
    Vector3 spawnPoint;
    Vector3 spawnPos;
    void createSpawns()
    {
        //NWAPS STNIOP YBAB ----spawn points baby
        //Free for all
        if (PhotonNetwork.room.customProperties["GameType"].ToString() == "Free For All")
        {
            GameObject spawnPoints = Instantiate(spawnpointsFFA,transform.position,transform.rotation) as GameObject;
            spawnPoints.transform.localPosition = new Vector3(400,0,300);
            if (PhotonNetwork.player.customProperties["Team"] == "Eagles")
            {
                spawnPoint = GameObject.Find("EaglesSpawnPoint").transform.position;
            }
            else if (PhotonNetwork.player.customProperties["Team"] == "Excorcist")
            {
                spawnPoint = GameObject.Find("ExorcistSpawnPoint").transform.position;
            }
            else if (PhotonNetwork.player.customProperties["Team"] == "Wolves")
            {
                spawnPoint = GameObject.Find("WolfSpawnPoint").transform.position;
            }
            else if (PhotonNetwork.player.customProperties["Team"] == "Angel")
            {
                spawnPoint = GameObject.Find("BloodSpawnPoint").transform.position;
            }
        }
        //Capture the flag
        else if (PhotonNetwork.room.customProperties["GameType"].ToString() == "Capture The Flag")
        {
            GameObject spawnPoints = Instantiate(spawnpointsCTF, transform.position, transform.rotation) as GameObject;
            spawnPoints.transform.localPosition = new Vector3(400, 0, 300);
            //Now spawn the player
            if (PhotonNetwork.player.customProperties["Team"] == "Eagles")
            {
                spawnPoint = GameObject.Find("EaglesSpawnPoint").transform.position + new Vector3(10,10,10);
            }
            else if (PhotonNetwork.player.customProperties["Team"] == "Excorcist")
            {
                spawnPoint = GameObject.Find("ExorcistSpawnPoint").transform.position + new Vector3(10, 10, 10);
            }
            else if (PhotonNetwork.player.customProperties["Team"] == "Wolves")
            {
                spawnPoint = GameObject.Find("WolfSpawnPoint").transform.position + new Vector3(10, 10, 10);
            }
            else if (PhotonNetwork.player.customProperties["Team"] == "Angel")
            {
                spawnPoint = GameObject.Find("BloodSpawnPoint").transform.position + new Vector3(10, 10, 10);
            }

            if (PhotonNetwork.isMasterClient)
            {
                GameObject eagleFlag = PhotonNetwork.Instantiate("Eagle_Flag", spawnPoint, Quaternion.identity, 0);
                int id = eagleFlag.GetPhotonView().viewID;
                photonView.RPC("fixFlag", PhotonTargets.All, id, "Eagle");

                GameObject exorcistFlag = PhotonNetwork.Instantiate("Exorcist_Flag", spawnPoint, Quaternion.identity, 0);
                id = exorcistFlag.GetPhotonView().viewID;
                photonView.RPC("fixFlag", PhotonTargets.All, id, "Exorcist");

                GameObject wolfFlag = PhotonNetwork.Instantiate("Wolf_Flag", spawnPoint, Quaternion.identity, 0);
                id = wolfFlag.GetPhotonView().viewID;
                photonView.RPC("fixFlag", PhotonTargets.All, id, "Wolf");

                GameObject bloodFlag = PhotonNetwork.Instantiate("Blood_Flag", spawnPoint, Quaternion.identity, 0);
                id = bloodFlag.GetPhotonView().viewID;
                photonView.RPC("fixFlag", PhotonTargets.All, id, "Blood");
            }
        }
    }
    #endregion
    GameObject currentFlag;
    [RPC]
    void fixFlag(int ID,string message)
    {
        //find the flag
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach(PhotonView view in views){
            if(ID == view.viewID){
                currentFlag = view.gameObject;
            }
        }
        if(message == "Eagle"){
            currentFlag.transform.position = GameObject.Find("EaglesSpawnPoint").transform.position;
            currentFlag.transform.FindChild("InteractiveCloth").GetComponent<InteractiveCloth>().enabled = true;
        }else if(message == "Exorcist"){
            currentFlag.transform.position = GameObject.Find("ExorcistSpawnPoint").transform.position;
            currentFlag.transform.FindChild("InteractiveCloth").GetComponent<InteractiveCloth>().enabled = true;
        }else if(message == "Wolf"){
            currentFlag.transform.position = GameObject.Find("WolfSpawnPoint").transform.position;
            currentFlag.transform.FindChild("InteractiveCloth").GetComponent<InteractiveCloth>().enabled = true;
        }else if (message == "Blood")
        {
            currentFlag.transform.position = GameObject.Find("BloodSpawnPoint").transform.position;
            currentFlag.transform.FindChild("InteractiveCloth").GetComponent<InteractiveCloth>().enabled = true;
        }

    }
}
