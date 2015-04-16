using UnityEngine;
using System.Collections;

public class CameraResetPosition : MonoBehaviour {
	public float CameraMoveTime;
	private bool lerpin = false;
	public bool Colliding = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider col){
		if (col.gameObject.tag == "Terrain") {
			Colliding = true;
			if(!lerpin){
				StartCoroutine(lerpPosition());
			}
		}
	}

	void OnTriggerStay(Collider col){
		if (col.gameObject.tag == "Terrain") {
			if(!lerpin){
				StartCoroutine(lerpPosition());
			}
		}
	}

	void OnTriggerExit(Collider col){
		if (col.gameObject.tag == "Terrain") {
			Colliding = false;
		}
	}

	IEnumerator lerpPosition()
	{
		Debug.Log ("I be lerping");
		lerpin = true;
		Vector3 startPos = transform.position;
		Vector3 endPos = transform.position;

		Vector3 tankPos = GetComponent<MouseOrbitC> ().target.transform.position;

		RaycastHit hit;
		if (Physics.Raycast(tankPos,startPos-tankPos,out hit,100)){
			print("There is something in front of the object!");
			Vector3 hitPos = hit.transform.position;

			float distance = (hitPos-tankPos).magnitude;

			float scaledDistance = distance * 0.9f;

			Vector3 unitDirection = (hitPos-tankPos).normalized;

			endPos = unitDirection * scaledDistance;

		}
		
		float t = 0.0f;
		float seconds = CameraMoveTime;
		
		while (t <= 1.0f)
		{
			t += Time.deltaTime / seconds;
			
			transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0.0f, 1.0f, t));
			
			yield return new WaitForFixedUpdate();
		}
		Debug.Log ("I not be lerping");
		lerpin = false;
		yield return null;
	}
}