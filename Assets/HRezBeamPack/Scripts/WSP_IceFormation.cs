using UnityEngine;
using System.Collections;

public class WSP_IceFormation : MonoBehaviour {

	private System.Random random;

	private Transform myTransform;
	public GameObject IceCrystalPrefab;

	public int IceCrystalsToSpawn = 4;
	public int IceCrystalsSpawned = 0;

	private float iceCrystalSpawnTimer = 0;
	private float IceCrystalSpawnFreq = 1;

	// Use this for initialization
	void Awake () {
		random = new System.Random((int)Time.deltaTime * 20);
		myTransform = gameObject.transform;
		IceCrystalsToSpawn = random.Next(3, 6);;
		IceCrystalSpawnFreq = random.Next(1, 2);
	}
	
	// Update is called once per frame
	void Update () {
		if (IceCrystalsSpawned < IceCrystalsToSpawn) {
			if (iceCrystalSpawnTimer < IceCrystalSpawnFreq) {
				iceCrystalSpawnTimer += Time.deltaTime;
			}
			else {
				SpawnIceCrystal();
				iceCrystalSpawnTimer = 0;
				IceCrystalSpawnFreq = random.Next(1, 2);
			}
		}
	}

	private void SpawnIceCrystal() {
		float randomXRotation = (float)random.Next(15, 260);
		random = new System.Random((int)Time.realtimeSinceStartup);
		float randomYRotation = (float)random.Next(15, 260);
		random = new System.Random((int)Time.realtimeSinceStartup);
		float randomZRotation = (float)random.Next(15, 260);
		random = new System.Random((int)Time.realtimeSinceStartup);
		Vector3 randomRotationEulers = new Vector3(randomXRotation, randomYRotation, randomZRotation);
		GameObject newIceCrystal = GameObject.Instantiate(IceCrystalPrefab, myTransform.position, Quaternion.Euler(randomRotationEulers)) as GameObject;
		newIceCrystal.name = "IceCrystal";
		IceCrystalsSpawned++;
	}
}
