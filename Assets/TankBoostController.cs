using UnityEngine;
using System.Collections;
	
public class TankBoostController : MonoBehaviour {
	public float lifeTimeOfTheBullet = 5;
	private float lifeTime;
	PhotonView view;
	void Start(){
		view = GetComponent<PhotonView>();
		rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		//Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Bullet"), LayerMask.NameToLayer("TankCollider"));
		Physics.minPenetrationForPenalty = 0;
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
	void OnCollisionEnter(Collision col)
	{
		if(col.gameObject.layer == LayerMask.NameToLayer("TankCollider")){
			Debug.Log("hit tank");
			PhotonNetwork.Destroy(gameObject);
		}
	}
	
	void Explosion(){
		PhotonNetwork.Instantiate("I_Made_Fire", transform.position, transform.rotation, 0);
		PhotonNetwork.Destroy(gameObject);
	}
}
