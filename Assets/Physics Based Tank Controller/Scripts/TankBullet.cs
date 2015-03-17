using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]

public class TankBullet : MonoBehaviour {

	public GameObject explosionPrefab;
	public int lifeTimeOfTheBullet = 5;
	private float lifeTime;
    PhotonView view;
    TankGunController gunController;

    public double m_CreationTime;
    public int m_projectileID;
	void Start(){
        gunController = FindObjectOfType<TankGunController>();
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
	

    //void OnTriggerEnter (Collider col) {
    //    if(view.isMine){
    //        if(col.gameObject.tag == "TankSystem"){
    //            Explosion();
    //            col.gameObject.SendMessage("TakeDamage", SendMessageOptions.RequireReceiver);
    //        }
    //    }
    //}

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.layer == LayerMask.NameToLayer("TankCollider")){
            if (view.isMine && col.gameObject.GetComponent<PhotonView>().owner.customProperties["Team"] != view.owner.customProperties["Team"])
            {
                Debug.Log("hit tank");

                PhotonPlayer player = col.gameObject.GetComponent<PhotonView>().owner;
                player.customProperties["Health"] = (int)player.customProperties["Health"] - 30;
                Debug.Log(player + "" + player.customProperties["Health"]);
                Debug.Log(PhotonNetwork.player + "" + PhotonNetwork.player.customProperties["Health"]);
            }
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Debug.Log("hit something else");
        }
        //if (view.isMine)
        //{
        //    Explosion();
        //    col.gameObject.SendMessage("TakeDamage", SendMessageOptions.RequireReceiver);
        //}
    }

	void Explosion(){
        PhotonNetwork.Instantiate("I_Made_Fire", transform.position, transform.rotation, 0);
        //Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, 15);
        //foreach (Collider hit in colliders)
        //{
        //    if (hit && hit.rigidbody)
        //    {
        //        //hit.rigidbody.isKinematic = false;
        //        //hit.rigidbody.AddExplosionForce(1, gameObject.transform.position, 15, 3);
        //        Debug.Log("Explosion");
        //    }
        //}
        //PhotonNetwork.Destroy(gameObject);
        PhotonNetwork.Destroy(gameObject);
	}
}
