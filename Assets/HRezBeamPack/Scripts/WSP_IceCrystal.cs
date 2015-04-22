using UnityEngine;
using System.Collections;

public class WSP_IceCrystal : MonoBehaviour {

	private System.Random random;

	private Transform myTransform;

	private float lifeTime = 0;
	private float GrowthSpeed = 2.5f;
	private Vector3 finalScale = Vector3.zero;

	private bool melt = false;

	public ParticleSystem MeltDrops;

	// Use this for initialization
	void Awake () {		
		myTransform = gameObject.transform;
		int randomSeed = (int)myTransform.position.x + (int)myTransform.position.y + (int)myTransform.position.z;
		random = new System.Random(randomSeed);
		myTransform.localScale = new Vector3(0, 0, 0);
		float randomXScale = (float)(random.NextDouble() * 0.25f);
		if (randomXScale < 0.15f)
			randomXScale = 0.15f;
		float randomYScale = (float)(random.NextDouble() * 0.25f);
		if (randomYScale < 0.15f)
			randomYScale = 0.15f;
		float randomZScale = (float)(random.NextDouble() * 0.45f); // Length
		if (randomZScale < 0.25f)
			randomZScale = 0.25f;
		finalScale = new Vector3(randomXScale, randomYScale, randomZScale);
		if (MeltDrops != null) {
			MeltDrops.emissionRate = 0;
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!melt) {
			float sizeDifference = Vector3.Distance(myTransform.localScale, finalScale);
			if (sizeDifference > 0.15f)
				myTransform.localScale = Vector3.Lerp(myTransform.localScale, finalScale, GrowthSpeed * Time.deltaTime);
			else
				melt = true;
		}
		else {
			lifeTime += Time.deltaTime;
			if (lifeTime > 1.5f) {
				float sizeDifference = Vector3.Distance(myTransform.localScale, Vector3.zero);
				if (sizeDifference > 0.15f) {
					if (MeltDrops != null) {
						if (MeltDrops.emissionRate < 30)
							MeltDrops.emissionRate += 4 * Time.deltaTime;
					}
					myTransform.localScale = Vector3.Lerp(myTransform.localScale, Vector3.zero, GrowthSpeed * 0.05f * Time.deltaTime);
				}
				else
					Destroy(gameObject);
			}
		}
	}
}
