using UnityEngine;
using System.Collections;

public class Shield : Photon.MonoBehaviour {
    public float lifeTime = 20.0f;
    public float timer = 0.0f;
	// Use this for initialization
	void Start () {
        timer = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
        if (photonView.isMine)
        {
            timer += Time.deltaTime;
            if (gameObject.activeSelf && timer > lifeTime)
            {
                Explosion();
            }
        }
	}

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("Shield Hit");
        if (col.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            Debug.Log(col.gameObject.GetComponent<PhotonView>().owner.customProperties["Team"].ToString());
            Debug.Log(PhotonNetwork.player.customProperties["Team"].ToString());
            if(col.gameObject.GetComponent<PhotonView>().owner.customProperties["Team"].ToString() != gameObject.GetPhotonView().owner.customProperties["Team"].ToString()){
                if(photonView.isMine){
                    Quaternion rot = Quaternion.Inverse(transform.rotation);
                    transform.rotation = rot;
                    GetComponent<Rigidbody>().AddForce(transform.forward * (.5f * GetComponent<Rigidbody>().velocity.magnitude), ForceMode.VelocityChange);
                }
            }
        }
    }
    void Explosion()
    {
        //PhotonNetwork.Instantiate("large flames", transform.position, transform.rotation, 0);

        if (photonView.isMine && gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
