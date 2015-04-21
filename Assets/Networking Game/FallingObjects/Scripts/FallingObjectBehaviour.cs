using UnityEngine;
using System.Collections;

public class FallingObjectBehaviour : MonoBehaviour {

	GameObject myself;
    private float lifeTime;
    public int lifeTimeOfTheBullet = 10;
    public GameObject explosion;
	PhotonView m_view;
	// Use this for initialization
	void Start () 
	{
		myself = gameObject;
		transform.rotation = Random.rotation;
		rigidbody.AddForce (transform.right*10.0f,ForceMode.VelocityChange);
		m_view = GetComponent<PhotonView> ();

	}//End Start()

	void OnCollisionEnter(Collision whoIHit)
	{
		if(whoIHit.gameObject.layer == LayerMask.NameToLayer("TankCollider")){
			if(whoIHit.gameObject.GetComponent<PhotonView>() != null)
			{
				if(m_view.isMine){
					whoIHit.gameObject.GetComponent<PhotonView>().RPC("tankGoBoom",PhotonTargets.All,whoIHit.gameObject.GetComponent<PhotonView>().viewID,null);
					PhotonNetwork.Instantiate("FireBlast", transform.position, transform.rotation,0);
					//Decrement so another can be spawned
					FallingObjectGenerator.spawned--;
					PhotonNetwork.Destroy(myself);
				}
			}
		}
		else{
			if(m_view.isMine){
				PhotonNetwork.Instantiate("FireBlast", transform.position, transform.rotation,0);
				//Decrement so another can be spawned
				FallingObjectGenerator.spawned--;
				PhotonNetwork.Destroy(myself);
			}
		}
	}//End OnCollisionEnter()


	// Update is called once per frame
	void Update () 
	{
		if(m_view.isMine){
	        lifeTime += Time.deltaTime;
	        if (gameObject.activeSelf && lifeTime > lifeTimeOfTheBullet)
	        {
				PhotonNetwork.Instantiate("FireBlast", transform.position, transform.rotation,0);
				//Decrement so another can be spawned
				FallingObjectGenerator.spawned--;
				PhotonNetwork.Destroy(myself);
			}
		}
	}//End Update()
}//End FallingObjectBehaviour
