using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]

public class TankBullet : MonoBehaviour {

	public GameObject explosionPrefab;
	public int lifeTimeOfTheBullet = 5;
	private float lifeTime;
    PhotonView view;
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
	   
		lifeTime += Time.deltaTime;

        if (gameObject.activeSelf && lifeTime > lifeTimeOfTheBullet)
        {
            if (view.isMine)
            {
                Explosion();
            }
        }
	}
	

	void OnCollisionEnter (Collision col) {
        if(view.isMine){
            Explosion();
        }
	}

	void Explosion(){
        PhotonNetwork.Instantiate("large flames", transform.position, transform.rotation, 0);

        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, 15);
        foreach (Collider hit in colliders)
        {
            if (hit && hit.rigidbody)
            {
                hit.rigidbody.isKinematic = false;
                hit.rigidbody.AddExplosionForce(100, gameObject.transform.position, 15, 3);
            }
        }
        PhotonNetwork.Destroy(gameObject);
	}
}
