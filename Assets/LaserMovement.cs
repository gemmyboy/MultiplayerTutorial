using UnityEngine;
using System.Collections;

public class LaserMovement : Photon.MonoBehaviour {
    public float lifeTime = 8.0f;
    public float timer = 0.0f;
    void Update()
    {
        if (photonView.isMine)
        {
            timer += Time.deltaTime;
            if (gameObject.activeSelf && timer > lifeTime)
            {
                Explosion();
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (photonView.isMine)
        {
            Explosion();
        }
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Shield"))
        {
            Debug.Log(col.gameObject.GetComponentInParent<PhotonView>().owner.customProperties["Team"].ToString() + " " + gameObject.GetPhotonView().owner.customProperties["Team"].ToString());
            if (col.gameObject.GetComponentInParent<PhotonView>().owner.customProperties["Team"].ToString() != gameObject.GetPhotonView().owner.customProperties["Team"].ToString())
            {
                if (photonView.isMine)
                {
                    Debug.Log("Other way");
                    GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                    PhotonNetwork.Instantiate("Sparks_Manager", transform.position, transform.rotation, 0);
                    GetComponent<Rigidbody>().AddForce(-transform.forward * 50, ForceMode.VelocityChange);
                }
            }
        }
    }

    void Explosion()
    {
        PhotonNetwork.Instantiate("Chaos Explosion", transform.position, transform.rotation, 0);

        if (photonView.isMine && gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
