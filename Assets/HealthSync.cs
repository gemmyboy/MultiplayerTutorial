using UnityEngine;
using System.Collections;

public class HealthSync : Photon.MonoBehaviour {
    public int health;

	// Use this for initialization
	void Start () {
        if (photonView.isMine)
        {
            this.enabled = false;//Only enable inter/extrapol for remote players
        }
        else
        {
            health = (int)PhotonNetwork.player.customProperties["Health"];
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
            //int oldhealth = health;
            health = (int)stream.ReceiveNext();
            //if(oldhealth != health){
                Debug.Log(info.sender.name + "sent this");
            //}
        }
    }
}
