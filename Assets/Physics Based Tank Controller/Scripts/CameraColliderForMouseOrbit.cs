using UnityEngine;
using System.Collections;

public class CameraColliderForMouseOrbit : MonoBehaviour {

	public enum CameraControllerType{UseWithMouseOrbit};
	public CameraControllerType _cameraControllerTypeChoise;
	public LayerMask layerMask;

	private bool useWithMouseOrbit = false;

	private MouseOrbit orbitScript;

	private float occDist;
	private GameObject pivotTarget;
	private GameObject target;
	private float defaultDistance;

	void Start () {

		switch(_cameraControllerTypeChoise){
		case CameraControllerType.UseWithMouseOrbit:
			useWithMouseOrbit = true;
			break;
		}
	
		if(useWithMouseOrbit){
			orbitScript = GetComponent<MouseOrbit>();
			target = orbitScript.target.gameObject;
			defaultDistance = orbitScript.distance;
		}

		pivotTarget = new GameObject("Pivot Position");

	}

	void Update () {
	
		pivotTarget.transform.position = target.transform.position;
		pivotTarget.transform.LookAt(transform);
		
		RaycastHit hit;
		
		if(Physics.Raycast(pivotTarget.transform.position, pivotTarget.transform.TransformDirection(Vector3.forward), out hit, defaultDistance, layerMask))
			occDist = (Mathf.Lerp (defaultDistance, 0, hit.distance / (defaultDistance/1))) + 1; 
		else
			occDist = 0;

		if(useWithMouseOrbit)
			orbitScript.distance = (defaultDistance) - occDist;

	}
}
