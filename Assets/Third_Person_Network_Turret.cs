using UnityEngine;
using System.Collections;

public class Third_Person_Network_Turret : Photon.MonoBehaviour {

    TankGunController controllerScript;

    void Awake()
    {
        controllerScript = GetComponent<TankGunController>();
        gameObject.name = gameObject.name + photonView.viewID;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.rotation);
        }
        else
        {
            //Network player, receive data
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
        }
    }

    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    void Update()
    {
        if (!photonView.isMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);
        }
    }
}
