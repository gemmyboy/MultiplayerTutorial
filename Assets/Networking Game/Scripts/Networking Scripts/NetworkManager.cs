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
    private void CreatePlayerObject()
    {
        Vector3 position = new Vector3(475, 100, 191);
        position.x += Random.Range(-50f, 50);
        //position.z += Random.Range(5f, 95f);

        //Instanitate Tank
        GameObject newPlayerObject = PhotonNetwork.Instantiate("T-90_Prefab_Network", position, Quaternion.identity, 0);
        //Add the camera target
        orbit = FindObjectOfType<MouseOrbitC>();
        //add the tankgun target
        tankGun = newPlayerObject.GetComponentInChildren<TankGunController>();
        Target = GameObject.Find("Target");
        orbit.target = newPlayerObject.transform;
        tankGun.target = Target.transform;
        //Turn off own health system
        Transform TankHealthSystem = newPlayerObject.transform.Find("TankHealthSystemCanvas");
        TankHealthSystem.gameObject.SetActive(false);
        //Change Material
        PhotonView view = newPlayerObject.GetComponent<PhotonView>();
        photonView.RPC("ChangeTankMaterial", PhotonTargets.AllBuffered, view.viewID, PhotonNetwork.player);
        //Apply the time script to Master Client
        //if(PhotonNetwork.isMasterClient){
        //    newPlayerObject.AddComponent<GameTimeManager>();
        //}

        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("Kills", 0);
        hash.Add("Deaths",0);
        hash.Add("Assist",0);
        hash.Add("Health",100);
        PhotonNetwork.player.SetCustomProperties(hash);
    }
    #endregion

    #region TankRPC
    GameObject tank;
    [RPC]
    public void ChangeTankMaterial(int viewID,PhotonPlayer player, PhotonMessageInfo info)
    {
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach(PhotonView view in views){
            if(view.viewID == viewID){
                tank = view.gameObject;
            }
        }
        Debug.Log(player.name + "\t" + tank.name);
        //Change the texture of the tank
        meshes = tank.GetComponentsInChildren<MeshRenderer>();
        Debug.Log(info.sender.name + "::Changing::" + tank.name);
        Debug.Log(player.customProperties["Team"]);
        Debug.Log("-------------------------------------------");



        if (player.customProperties["Team"] == "Eagles")
        {
            foreach (MeshRenderer mesh in meshes)
            {
                if (mesh.name == "MainGun Mesh")
                {
                    mesh.gameObject.renderer.materials[1].CopyPropertiesFromMaterial(eagleMaterial);
                }
                else
                {
                    mesh.gameObject.renderer.material = eagleMaterial;
                }
            }
        }
        else if (player.customProperties["Team"] == "Excorcist")
        {
            foreach (MeshRenderer mesh in meshes)
            {
                if (mesh.name == "MainGun Mesh")
                {
                    mesh.gameObject.renderer.materials[1].CopyPropertiesFromMaterial(excorsistMaterial);
                }
                else
                {
                    mesh.gameObject.renderer.material = excorsistMaterial;
                }
            }
        }
        //else if (player.customProperties["Team"] == "Wolves")
        //{
        //    foreach (MeshRenderer mesh in meshes)
        //    {
        //        if (mesh.name == "MainGun Mesh")
        //        {
        //            mesh.gameObject.renderer.materials[1].CopyPropertiesFromMaterial(wolfMaterial);
        //        }
        //        else
        //        {
        //            mesh.gameObject.renderer.material = wolfMaterial;
        //        }
        //    }
        //}
        //else if (player.customProperties["Team"] == "Angel")
        //{
        //    foreach (MeshRenderer mesh in meshes)
        //    {
        //        if (mesh.name == "MainGun Mesh")
        //        {
        //            mesh.gameObject.renderer.materials[1].CopyPropertiesFromMaterial(angelMaterial);
        //        }
        //        else
        //        {
        //            mesh.gameObject.renderer.material = angelMaterial;
        //        }
        //    }
        //}
    }
    #endregion
}
