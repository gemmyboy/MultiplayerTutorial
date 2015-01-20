using UnityEngine;
using System.Collections;
using Photon;
public class NetworkManager : PunBehaviour
{
    #region Members

    private PhotonAnimatorView m_AnimatorView;  // local animatorView. set when we create our character in CreatePlayerObject()
    private Animator m_RemoteAnimator;          // to display the synchronized values on the right side in the GUI. A third player will simply be ignored (until the second player leaves)

    private float m_SlideIn = 0f;
    private float m_FoundPlayerSlideIn = 0f;
    private bool m_IsOpen = false;

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

    public void Update()
    {
        FindRemoteAnimator();

        m_SlideIn = Mathf.Lerp(m_SlideIn, m_IsOpen ? 1f : 0f, Time.deltaTime * 9f);
        m_FoundPlayerSlideIn = Mathf.Lerp(m_FoundPlayerSlideIn, m_AnimatorView == null ? 0f : 1f, Time.deltaTime * 5f);
    }

    /// <summary>Finds the Animator component of a remote client on a GameObject tagged as Player and sets m_RemoteAnimator.</summary>
    public void FindRemoteAnimator()
    {
        if (m_RemoteAnimator != null)
        {
            return;
        }

        // the prefab has to be tagged as Player
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < gos.Length; ++i)
        {
            PhotonView view = gos[i].GetComponent<PhotonView>();
            if (view != null && view.isMine == false)
            {
                m_RemoteAnimator = gos[i].GetComponent<Animator>();
            }
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
    public override void OnJoinedRoom()
    {
        //CreatePlayerObject();
        Debug.Log("Sexy Momma");
        Vector3 position = new Vector3(-2, 500, 0);
        position.x += Random.Range(25f, 50f);
        position.z += Random.Range(25f, 50f);
        PhotonNetwork.Instantiate("Pod", position, Quaternion.identity, 0);
    }

    void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        {
            Debug.Log("Sexy Momma");
            Vector3 position = new Vector3(-2, 500, 0);
            position.x += Random.Range(5f, 95f);
            position.z += Random.Range(5f, 95f);
            Quaternion pieceRotation = Quaternion.AngleAxis(270, Vector3.left);
            PhotonNetwork.Instantiate("Pod", position, pieceRotation, 0);
        }
    }

    private void CreatePlayerObject()
    {
        Vector3 position = new Vector3(-2, 0, 0);
        position.x += Random.Range(-3f, 3f);
        position.z += Random.Range(-4f, 4f);

        GameObject newPlayerObject = PhotonNetwork.Instantiate("Robot Kyle Mecanim", position, Quaternion.identity, 0);
        m_AnimatorView = newPlayerObject.GetComponent<PhotonAnimatorView>();
    }
    #endregion
}
