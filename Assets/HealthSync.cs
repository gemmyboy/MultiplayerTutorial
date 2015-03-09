using UnityEngine;
using System.Collections;

public class HealthSync : Photon.MonoBehaviour {
    float health = 100;

	// Use this for initialization
	void Start () {
        if (photonView.isMine)
            this.enabled = false;//Only enable inter/extrapol for remote players
	}

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Always send transform (depending on reliability of the network view)
        if (stream.isWriting)
        {
            stream.SendNext(health);
        }
        // When receiving, buffer the information
        else
        {
            health = (float)stream.ReceiveNext();
        }
    }
}
