using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]

public class TankBullet : Photon.MonoBehaviour {

	public GameObject explosionPrefab;
	public int lifeTimeOfTheBullet = 5;
	private float lifeTime;
    PhotonView view;
    //TankGunController gunController;

    public double m_CreationTime;
    public int m_projectileID;
	void Start(){
        //gunController = FindObjectOfType<TankGunController>();
        view = GetComponent<PhotonView>();

		rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

		//Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Bullet"), LayerMask.NameToLayer("TankCollider"));
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
					//photonView.RPC ("BulletExplosionForce",PhotonTargets.All);
					//transform.rigidbody.AddExplosionForce(15000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);
					Explosion ();
					//PhotonNetwork.Destroy(gameObject);
					//Destroy(gameObject);

	                //PhotonPlayer player = col.gameObject.GetComponent<PhotonView>().owner;
	                //int changedHealth = (int)player.customProperties["Health"] - 30;

	                //ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
	                //hash.Add("Health",changedHealth);
	                //player.SetCustomProperties(hash);

	            }
			}
            
        }
        else
        {
            Debug.Log("hit something else");
			//Explosion ();
			//PhotonNetwork.Destroy (gameObject);
            if (view.isMine)
            {
           	 	Explosion();
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

//	[RPC]
//	void BulletExplosionForce()
//	{
//		transform.rigidbody.AddExplosionForce(15000.0f,transform.position,5.0f,0.0f,ForceMode.Impulse);
//	}
}
