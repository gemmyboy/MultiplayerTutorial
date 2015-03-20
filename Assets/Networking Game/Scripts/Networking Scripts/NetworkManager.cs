using UnityEngine;
using System.Collections;
using Photon;
public class NetworkManager : PunBehaviour
{
    #region Members
    public Material eagleMaterial;
    public Material excorsistMaterial;
    public Material wolfMaterial;
    public Material angelMaterial;
    #endregion

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
    Vector3 spawnPoint;
    private void CreatePlayerObject()
    {
        foreach(PhotonPlayer player in PhotonNetwork.playerList){
            Debug.Log(player);
        }
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
        //Apply the time script to Master Client
        //if(PhotonNetwork.isMasterClient){
        //    newPlayerObject.AddComponent<GameTimeManager>();
        //}

        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        if(PhotonNetwork.room.customProperties["GameType"] != "Free For All"){
            hash.Add("TeamScore",0);
        }
        hash.Add("Kills", 0);
        hash.Add("Deaths",0);
        hash.Add("Assist",0);
        hash.Add("Health",100);
        PhotonNetwork.player.SetCustomProperties(hash);
    }
    #endregion
}
