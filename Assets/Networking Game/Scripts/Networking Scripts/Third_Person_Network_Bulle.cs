using UnityEngine;
using System.Collections;

public class Third_Person_Network_Bulle : Photon.MonoBehaviour {
    TankBullet bulletScript;

    private Vector3 correctBulletPos = Vector3.zero; //We lerp towards this
    private Quaternion correctBulletRot = Quaternion.identity; //We lerp towards this
    private Vector3 correctBulletVelocity = Vector3.zero;
    void Awake()
    {
        bulletScript = GetComponent<TankBullet>();
        gameObject.name = gameObject.name + photonView.viewID;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rigidbody.velocity);
        }
        else
        {
            //Network player, receive data
            correctBulletPos = (Vector3)stream.ReceiveNext();
            correctBulletRot = (Quaternion)stream.ReceiveNext();
            correctBulletVelocity = (Vector3)stream.ReceiveNext();
        }
    }

    void Update()
    {
        if (!photonView.isMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.position = Vector3.Lerp(transform.position, correctBulletPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctBulletRot, Time.deltaTime * 5);
        }
    }
}
