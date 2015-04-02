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

		//attempting to fix unity crash bug.
		//PhotonNetwork.DestroyPlayerObjects(player);
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
		Debug.Log ("The LEVEL IS: ");
		Debug.Log (level);

        if (level == 1 || Start_Menu_Server_Check.SceneNameGame == "Demo Scene")
        {
            CreatePlayerObject();
        }
    }
    private void CreatePlayerObject()
    {
        //OMEGA TANK BABY
        Random.seed = Mathf.RoundToInt(Time.time * 100);
        //--------------------------------------------------------------------------------------------------------------
        if(PhotonNetwork.room.customProperties["GameType"].ToString() == "Omega Tank" && PhotonNetwork.isMasterClient){
            Debug.Log(Random.seed = Mathf.RoundToInt(Time.time * 100));
            //Choose random omega and its faction
            int randomOmega = Random.Range(0, PhotonNetwork.playerList.Length);
            int randomFaction = Random.Range(0, 3);

            //Make the things we need
            List<string> teams = new List<string>() {"Eagles", "Exorcist", "Wolves", "Angel" };
            PhotonPlayer[] players = PhotonNetwork.playerList;
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            PhotonPlayer theOmega = players[randomOmega];

            //Add it and secure the baby boys
            hash.Add("Team", teams[randomFaction]);
            hash.Add("TheOmega",1);
            theOmega.SetCustomProperties(hash);

            //now to make a team for the other players
            teams.Remove(teams[randomFaction]);
            randomFaction = Random.Range(0, 2);

            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if(player != theOmega){
                    hash = new ExitGames.Client.Photon.Hashtable();
                    hash.Add("Team", teams[randomFaction]);
                    hash.Add("TheOmega",0);
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
		Debug.Log ("SHOULD HAVE SPAWNED PLAYER - INITIAL");
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
        //If Capture the flag, add the flag script
        if (PhotonNetwork.room.customProperties["GameType"].ToString() == "Capture The Flag")
        {
            newPlayerObject.AddComponent<PickUpFlag>();
        }
        //--------------------------------------------------------------------------------------------------------------

        //Setup scoring system
        ExitGames.Client.Photon.Hashtable hash2 = new ExitGames.Client.Photon.Hashtable();
        if(PhotonNetwork.room.customProperties["GameType"].ToString() != "Free For All"){
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
    void createSpawns()
    {
        //NWAPS STNIOP YBAB ----spawn points baby
        //Free for all
        if (PhotonNetwork.room.customProperties["GameType"].ToString() == "Free For All")
        {
            GameObject spawnPoints = Instantiate(spawnpointsFFA,transform.position,transform.rotation) as GameObject;
            spawnPoints.transform.localPosition = new Vector3(400,0,300);
            if (PhotonNetwork.player.customProperties["Team"].ToString() == "Eagles")
            {
                spawnPoint = GameObject.Find("EaglesSpawnPoint").transform.position;
            }
			else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Exorcist")
            {
                spawnPoint = GameObject.Find("ExorcistSpawnPoint").transform.position;
            }
			else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Wolves")
            {
                spawnPoint = GameObject.Find("WolfSpawnPoint").transform.position;
            }
			else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Angel")
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
			if (PhotonNetwork.player.customProperties["Team"].ToString() == "Eagles")
            {
                int randX = Random.Range(0, 30);
                int randZ = Random.Range(0, 30);
                spawnPoint = GameObject.Find("EaglesSpawnPoint").transform.position + new Vector3(randX, 20, randZ);
            }
			else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Exorcist")
            {
                int randX = Random.Range(0, 30);
                int randZ = Random.Range(0, 30);
                spawnPoint = GameObject.Find("ExorcistSpawnPoint").transform.position + new Vector3(randX, 20, randZ);
            }
			else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Wolves")
            {
                int randX = Random.Range(0, 30);
                int randZ = Random.Range(0, 30);
                spawnPoint = GameObject.Find("WolfSpawnPoint").transform.position + new Vector3(randX, 20, randZ);
            }
			else if (PhotonNetwork.player.customProperties["Team"].ToString() == "Angel")
            {
                int randX = Random.Range(0, 30);
                int randZ = Random.Range(0, 30);
                spawnPoint = GameObject.Find("BloodSpawnPoint").transform.position + new Vector3(randX, 20, randZ);
            }

            if (PhotonNetwork.isMasterClient)
            {
                GameObject eagleFlag = PhotonNetwork.Instantiate("Eagle_Flag", GameObject.Find("EaglesSpawnPoint").transform.position, Quaternion.identity, 0);
                GameObject exorcistFlag = PhotonNetwork.Instantiate("Exorcist_Flag", GameObject.Find("ExorcistSpawnPoint").transform.position, Quaternion.identity, 0);
                GameObject wolfFlag = PhotonNetwork.Instantiate("Wolf_Flag", GameObject.Find("WolfSpawnPoint").transform.position, Quaternion.identity, 0);
                GameObject bloodFlag = PhotonNetwork.Instantiate("Blood_Flag", GameObject.Find("BloodSpawnPoint").transform.position, Quaternion.identity, 0);
            }
        }
        else if (PhotonNetwork.room.customProperties["GameType"].ToString() == "Omega Tank")
        {
            if((int)PhotonNetwork.player.customProperties["TheOmega"] == 1){
                spawnPoint = new Vector3(400,1000,300);
            }
            else
            {
                spawnPoint = new Vector3(400, -1000, 300);
            }
        }
    }

    public GameObject sphere;
    bool checkSpawn(Transform pos)
    {
        RaycastHit hit;
        Vector3 positionCheck = new Vector3(pos.position.x,300,pos.position.z);
        if (Physics.Raycast(positionCheck, Vector3.down, out hit, 250.0f))
        {
            Debug.DrawRay(positionCheck, Vector3.down * hit.distance, Color.blue, 200.0f);
            if(hit.collider.gameObject.tag == "Terrain"){
                Collider[] hitColliders = Physics.OverlapSphere(hit.point, 100.0f);
                Debug.Log(hitColliders.Length);
                if (hitColliders.Length == 0)
                {
                    spawnPoint = hit.point + new Vector3(0,10,0);
                    return true;
                }
                else if(hitColliders.Length == 1){
                    if(hitColliders[0].tag == "Terrain"){
                        Debug.Log("Ready to spawn");
                        spawnPoint = hit.point + new Vector3(0, 15, 0);
                        return true;
                    }
                }
                else
                {
                    foreach (Collider collider in hitColliders)
                    {
                        Debug.Log(collider.gameObject.name);
                    }
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    #endregion
}
