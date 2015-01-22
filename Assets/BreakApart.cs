using UnityEngine;
using System.Collections;

public class BreakApart : MonoBehaviour {
    public GameObject box;
    public float radius = 20.0F;
    public float power = 10.0F;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyDown(KeyCode.R)){
            Destroy(gameObject);
            Rigidbody boxes = Instantiate(box,transform.position,transform.rotation) as Rigidbody;
            //Vector3 explosionPos = transform.position;
            //Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
            //foreach (Collider hit in colliders)
            //{
            //    if (hit && hit.rigidbody)
            //        hit.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0F);

            //}
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.name == "Sphere Prefab(Clone)"){
            Destroy(gameObject);
            Rigidbody boxes = Instantiate(box, transform.position, transform.rotation) as Rigidbody;
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
            foreach (Collider hit in colliders)
            {
                if (hit && hit.rigidbody)
                    hit.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0F);

            }
        }
    }
}
