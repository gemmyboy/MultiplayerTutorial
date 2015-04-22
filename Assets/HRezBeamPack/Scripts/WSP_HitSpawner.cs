using UnityEngine;
using System.Collections;

public class WSP_HitSpawner : MonoBehaviour {

	private System.Random random;

	private WSP_HighRezLaser parentHighRezLaser;

	public Vector3 HitPosition = Vector3.zero;
	public GameObject PrefabToSpawn;

	public string HitGOName = "UnnamedHitObject";

	private float spawningTimer = 0;
	public float SpawnFreq = 1.0f;

	private Transform targetTrans;

	void Start() {		
		parentHighRezLaser = gameObject.GetComponent<WSP_HighRezLaser>();
		random = new System.Random((int)Time.realtimeSinceStartup);
	}

	void Update() {
		if (parentHighRezLaser != null) {
			if (targetTrans != parentHighRezLaser.TargetHit)
				targetTrans = parentHighRezLaser.TargetHit;
		}
	}

	public void RunHitSpawner(Vector3 hitPositionIn, string hitNameIn) {
		HitPosition = hitPositionIn;
		HitGOName = hitNameIn;
		if (spawningTimer < SpawnFreq) {
			spawningTimer += Time.deltaTime;
		}
		else {
			SpawnHitGameObject();
			spawningTimer = 0;
		}
	}

	private void SpawnHitGameObject() {
		if (PrefabToSpawn != null) {
			float randomXRotation = (float)random.Next(15, 260);
			random = new System.Random((int)Time.realtimeSinceStartup);
			float randomYRotation = (float)random.Next(15, 260);
			random = new System.Random((int)Time.realtimeSinceStartup);
			float randomZRotation = (float)random.Next(15, 260);
			random = new System.Random((int)Time.realtimeSinceStartup);
			Vector3 randomRotationEulers = new Vector3(randomXRotation, randomYRotation, randomZRotation);
			GameObject newHitGameObject = GameObject.Instantiate(PrefabToSpawn, HitPosition, Quaternion.Euler(randomRotationEulers)) as GameObject;
			newHitGameObject.name = HitGOName;
			if (targetTrans != null) {
				newHitGameObject.transform.parent = targetTrans;
				if (targetTrans.rigidbody != null)
					newHitGameObject.transform.localPosition = Vector3.zero;
			}
		}
	}
}
