using UnityEngine;
using System.Collections;
using Photon;
public class NetworkManager : PunBehaviour
{
    #region Members
    public Texture wolfTexture;
    public Texture eagleTexture;
    public Texture angelsTexture;
    public Texture excorcistTexture;

    public Material eagleMaterial;
    public Material excorsistMaterial;
    public Material wolfMaterial;
    public Material angelMaterial;
    #endregion



    #region Unity
    public Transform playerPrefab;

    public void Awake()
    {
        // in case we started this demo with the wrong scene being active, simply load the menu scene
        if (!PhotonNetwork.connected)
        {
            Application.LoadLevel(Start_Menu_Server_Check.SceneNameMenu);
            return;
        }

        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
        //PhotonNetwork.Instantiate(this.playerPrefab.name, transform.position + Vector3.up * 5, Quaternion.identity, 0);
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

        // back to main menu        
        Application.LoadLevel(Start_Menu_Server_Check.SceneNameMenu);
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("OnDisconnectedFromPhoton");

        // back to main menu        
        Application.LoadLevel(Start_Menu_Server_Check.SceneNameMenu);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate " + info.sender);    // you could use this info to store this or react
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerConnected: " + player);
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
        //Change the texture of the tank
        meshes = newPlayerObject.GetComponentsInChildren<MeshRenderer>();
        if(PhotonNetwork.player.customProperties["Team"] == "Eagles"){

            foreach (MeshRenderer mesh in meshes)
            {
                mesh.gameObject.renderer.material = eagleMaterial;
            }
        }
        else if (PhotonNetwork.player.customProperties["Team"] == "Excorcist")
        {
            foreach (MeshRenderer mesh in meshes)
            {
                mesh.gameObject.renderer.material = excorsistMaterial;
            }
        }
        else if (PhotonNetwork.player.customProperties["Team"] == "Wolves")
        {
            foreach (MeshRenderer mesh in meshes)
            {
                mesh.gameObject.renderer.material = wolfMaterial;
            }
        }
        else if (PhotonNetwork.player.customProperties["Team"] == "Angel")
        {
            foreach (MeshRenderer mesh in meshes)
            {
                mesh.gameObject.renderer.material = angelMaterial;
            }
        }
        //Add the camera target
        orbit = FindObjectOfType<MouseOrbitC>();
        //add the tankgun target
        tankGun = newPlayerObject.GetComponentInChildren<TankGunController>();
        Target = GameObject.Find("Target");
        orbit.target = newPlayerObject.transform;
        tankGun.target = Target.transform;
    }
    #endregion
}
