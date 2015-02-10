using UnityEngine;
using System.Collections;

public class Fragments : MonoBehaviour {

	private bool broken = false;

	void Start () {
		rigidbody.isKinematic = true;
	}
	
	// Update is called once per frame
	void Update () {

        if (!broken) { }
			//Checking();

	}

	void Checking(){

		RaycastHit hit;
		Debug.DrawRay(transform.position, -transform.forward * .35f);
		
		if(Physics.Raycast(transform.position, -transform.forward, out hit)){
			if(hit.rigidbody && hit.rigidbody.isKinematic != true){
				rigidbody.isKinematic = false;
				broken = true;
			}
		}

	}

	void OnCollisionEnter (Collision collision) {
		
		if(collision.transform.gameObject.layer != LayerMask.NameToLayer("Fragment")){
			rigidbody.isKinematic = false;
		}
	}

    void OnCollisionExit(Collision col)
    {
        if (col.transform.gameObject.layer == LayerMask.NameToLayer("Fragment"))
        {
            rigidbody.isKinematic = false;
        }
    }


}
