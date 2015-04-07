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
            Debug.Log("hit something else");
            if (view.isMine)
            {
           	 	Explosion();
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("Shield Hit");
        if (col.gameObject.layer == LayerMask.NameToLayer("Shield"))
        {
            Debug.Log("Sheild: " + col.gameObject.GetComponentInParent<PhotonView>().owner.customProperties["Team"].ToString());
            Debug.Log("Bullet: " + gameObject.GetPhotonView().owner.customProperties["Team"].ToString());
            if (col.gameObject.GetComponentInParent<PhotonView>().owner.customProperties["Team"].ToString() != gameObject.GetPhotonView().owner.customProperties["Team"].ToString())
            {
                if (view.isMine)
                {
                    Debug.Log("Photon Change Directiion");
                    GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
                    GetComponent<Rigidbody>().AddForce(-transform.forward * 50, ForceMode.VelocityChange);
                    GetComponent<Rigidbody>().AddForce(transform.up * 25, ForceMode.VelocityChange);
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
}
