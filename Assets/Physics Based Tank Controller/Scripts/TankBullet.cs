using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]

public class TankBullet : Photon.MonoBehaviour {

	public GameObject explosionPrefab;
	public int lifeTimeOfTheBullet = 5;
	private float lifeTime;
    PhotonView view;

    public double m_CreationTime;
    public int m_projectileID;
	void Start(){
        view = GetComponent<PhotonView>();

		rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

		Physics.minPenetrationForPenalty = 0;

		if(LayerMask.LayerToName(10) != "Bullet"){
			Debug.LogError ("Couldn't found ''Bullet'' layer! Create ''Bullet'' layer for 10th line. You can add layers from Project Settings --> Tags and Layers");
			Debug.Break();
		}

	}

	void Update () {
        //transform.LookAt(transform.position + transform.forward);
        transform.LookAt(transform.position + rigidbody.velocity);
        if(view.isMine){
            lifeTime += Time.deltaTime;
            if (gameObject.activeSelf && lifeTime > lifeTimeOfTheBullet)
            {
                Explosion();
            }
        }
	}
	

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.layer == LayerMask.NameToLayer("TankCollider")){
			if(col.gameObject.GetComponent<PhotonView>() != null)
			{
            	if (view.isMine && col.gameObject.GetComponent<PhotonView>().owner.customProperties["Team"] != view.owner.customProperties["Team"])
	            {
	                Debug.Log("hit tank");
					Explosion ();
	            }
			}
        }
        else
        {
            Debug.LogError(view.owner);
            Debug.LogError(view.isMine);
            if(view.isMine){
                Explosion();
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Shield"))
        {
            Debug.Log(col.gameObject.GetComponentInParent<PhotonView>().owner.customProperties["Team"].ToString() + " "  +  gameObject.GetPhotonView().owner.customProperties["Team"].ToString());
            if (col.gameObject.GetComponentInParent<PhotonView>().owner.customProperties["Team"].ToString() != gameObject.GetPhotonView().owner.customProperties["Team"].ToString())
            {
                if (view.isMine)
                {
                    Debug.Log("Other way");
                    GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
                    //transform.Rotate(0, 180, 0);
                    PhotonNetwork.Instantiate("Sparks_Manager", transform.position, transform.rotation, 0);
                    GetComponent<Rigidbody>().AddForce(-transform.forward * 50, ForceMode.VelocityChange);
                    GetComponent<Rigidbody>().AddForce(transform.up * 25, ForceMode.VelocityChange);
                    photonView.RPC("askForOwnership", PhotonNetwork.player, col.gameObject.GetComponentInParent<PhotonView>().owner);
                }
            }
        }
    }

	void Explosion(){
        PhotonNetwork.Instantiate("large flames", transform.position, transform.rotation, 0);

		if(photonView.isMine && gameObject != null)
		{
			PhotonNetwork.Destroy(gameObject);
		}
	}

    PhotonView bulletView;
    [RPC]
    void askForOwnership(PhotonPlayer player)
    {
        StartCoroutine(wait(player));
    }

    IEnumerator wait(PhotonPlayer player)
    {
        yield return new WaitForSeconds(.1f);
        view.TransferOwnership(player);
    }
}
