using UnityEngine;
using System.Collections;

public class Third_Person_Network_Turret : Photon.MonoBehaviour {

    TankGunController controllerScript;
    public GameObject barrel;

    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    private Vector3 correctPlayerPosBarrel = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRotBarrel = Quaternion.identity; //We lerp towards this
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
            //stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

            stream.SendNext(barrel.transform.position);
            stream.SendNext(barrel.transform.rotation);
        }
        if(stream.isReading)
        {
            //Network player, receive data
            //correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();

            correctPlayerPosBarrel = (Vector3)stream.ReceiveNext();
            correctPlayerRotBarrel = (Quaternion)stream.ReceiveNext();
        }
    }

    void Update()
    {
        if (!photonView.isMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)

            //transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
            //transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);

            //barrel.transform.position = Vector3.Lerp(barrel.transform.position, correctPlayerPosBarrel, Time.deltaTime * 5);
            //barrel.transform.rotation = Quaternion.Lerp(barrel.transform.rotation, correctPlayerRotBarrel, Time.deltaTime * 5);
        }
    }
}
