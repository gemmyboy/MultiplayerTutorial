using UnityEngine;
using System.Collections;
using Photon;
using ExitGames.Client.Photon;
public class DemoRPGMovement : PunBehaviour 
{
    public RPGCamera Camera;
    public void Awake()
    {
        // in case we started this demo with the wrong scene being active, simply load the menu scene
        if (!PhotonNetwork.connected)
        {
            Application.LoadLevel(Start_Menu_Server_Check.SceneNameMenu);
            return;
        }
        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate

    }
    void CreatePlayerObject()
    {
        Vector3 position = new Vector3( 33.5f, 1.5f, 20.5f );
        GameObject newPlayerObject = PhotonNetwork.Instantiate( "Robot Kyle RPG", position, Quaternion.identity, 0 );
        Camera.Target = newPlayerObject.transform;
        switch (PhotonNetwork.player.customProperties["Team"].ToString())
        {
            case "Red":
                newPlayerObject.GetComponentInChildren<Renderer>().material.color = Color.red;
                break;
            case "Blue":
                newPlayerObject.GetComponentInChildren<Renderer>().material.color = Color.blue;
                break;
            case "Green":
                newPlayerObject.GetComponentInChildren<Renderer>().material.color = Color.green;
                break;
            case "Yellow":
                newPlayerObject.GetComponentInChildren<Renderer>().material.color = Color.yellow;
                break;
        }
        Debug.Log("Player");
    }

    void OnLevelWasLoaded(int level)
    {
        if (level == 2)
        {
            CreatePlayerObject();
            
        }
    }
}
