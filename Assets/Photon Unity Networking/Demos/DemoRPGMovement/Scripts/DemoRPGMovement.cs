using UnityEngine;
using System.Collections;

public class DemoRPGMovement : MonoBehaviour 
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
        //PhotonNetwork.Instantiate(this.playerPrefab.name, transform.position + Vector3.up * 5, Quaternion.identity, 0);
    }
    void OnJoinedRoom()
    {
        //CreatePlayerObject();
        Debug.Log("Hey");
    }

    void CreatePlayerObject()
    {
        Vector3 position = new Vector3( 33.5f, 1.5f, 20.5f );
        GameObject newPlayerObject = PhotonNetwork.Instantiate( "Robot Kyle RPG", position, Quaternion.identity, 0 );
        Camera.Target = newPlayerObject.transform;
        Debug.Log("Player");
    }

    void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        {
            CreatePlayerObject();
        }
    }
}
