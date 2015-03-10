using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]

public class TankBullet : MonoBehaviour {

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
		//Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Bullet"), LayerMask.NameToLayer("TankCollider"));
		Physics.minPenetrationForPenalty = 0;

		if(LayerMask.LayerToName(10) != "Bullet"){
			Debug.LogError ("Couldn't found ''Bullet'' layer! Create ''Bullet'' layer for 10th line. You can add layers from Project Settings --> Tags and Layers");
			Debug.Break();
		}

	}

	void Update () {
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
        Debug.Log("hit something");
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
        Destroy(gameObject);
	}
}
