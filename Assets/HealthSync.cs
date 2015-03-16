using UnityEngine;
using System.Collections;

public class HealthSync : Photon.MonoBehaviour {
    public int health;
    public UIManager uiManager;
	// Use this for initialization
	void Start () {
        if (photonView.isMine)
        {
            this.enabled = false;//Only enable inter/extrapol for remote players
        }
        else
        {
            health = (int)PhotonNetwork.player.customProperties["Health"];
            uiManager = FindObjectOfType<UIManager>();
        }
	}

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Always send transform (depending on reliability of the network view)
        if (stream.isWriting)
        {
            stream.SendNext(health);
            Debug.Log(info.sender + "Send: " + health);
        }
        // When receiving, buffer the information
        else
        {
            health = (int)stream.ReceiveNext();
            Debug.Log(info.sender.name + "sent this");
        }
    }

    void Update()
    {
        int oldHealth = (int)PhotonNetwork.player.customProperties["Health"];
        if(oldHealth != health){
            Debug.Log("Yeah");
            uiManager.ChangeHealth(oldHealth);
        }
    }
}
