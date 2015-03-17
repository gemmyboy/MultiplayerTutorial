using UnityEngine;
using System.Collections;

public class TankBulletMovement : Photon.MonoBehaviour {
    //NETWORKING
    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;

    private Quaternion syncStartRotation = Quaternion.identity;
    private Quaternion syncEndRotation = Quaternion.identity;
    void Awake()
    {
        lastSynchronizationTime = Time.time;
        if (photonView.isMine)
        {
            this.enabled = false;
        }
        else
        {
            PhotonNetwork.sendRate = 25;
            PhotonNetwork.sendRateOnSerialize = 25;
        }
    }

    void Update()
    {
        if (!photonView.isMine)
        {
            SyncedMovement();
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Vector3 syncPosition = Vector3.zero;
        Vector3 syncVelocity = Vector3.zero;
        Quaternion syncRotation = Quaternion.identity;
        if (stream.isWriting)
        {
            syncPosition = rigidbody.position;
            stream.Serialize(ref syncPosition);

            syncPosition = rigidbody.velocity;
            stream.Serialize(ref syncVelocity);

            //Quaternion rot = transform.rotation;
            //stream.Serialize(ref rot);

            syncRotation = rigidbody.rotation;
            stream.Serialize(ref syncRotation);
        }
        else
        {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);
            stream.Serialize(ref syncRotation);

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncEndPosition = syncPosition + syncVelocity * syncDelay;
            syncStartPosition = rigidbody.position;

            syncStartRotation = rigidbody.rotation;
            syncEndRotation = syncRotation;
        }
    }

    private void SyncedMovement()
    {
        syncTime += Time.deltaTime;

        rigidbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
        rigidbody.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime / syncDelay);
    }
}
