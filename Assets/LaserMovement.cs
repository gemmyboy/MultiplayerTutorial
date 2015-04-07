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

    void Explosion()
    {
        PhotonNetwork.Instantiate("Chaos Explosion", transform.position, transform.rotation, 0);

        if (photonView.isMine && gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
