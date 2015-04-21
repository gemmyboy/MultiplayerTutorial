using UnityEngine;
using System.Collections;

public class FallingObjectGenerator : MonoBehaviour {

	//Array containing all possible object we can spawn and drop
	public Object [] theObjects;

	//constraints
	public int xMin = -80;
	public int zMin = -80;
	public int xMax = 80;
	public int zMax = 80;

	public int yDropHeight = 1000;

	//Instantiation limit
	static public int spawned = 0;
	public int spawnLimit = 40;

	public float timer = 0.0f;
	public float maxTimer = .5f;
	// Update is called once per frame
	void Update () 
	{
		timer += Time.deltaTime;
		if(spawned < spawnLimit && timer > maxTimer && PhotonNetwork.isMasterClient)
		{
			int theX = 0;
			int theZ = 0;
			GameObject temp;

			//Randomly pick a point between xMin and xMax
			theX = Random.Range(xMin, xMax);

			//Randomly pick a point between yMin and yMax
			theZ = Random.Range(zMin, zMax);

			//Instantiate at the designated x, yDropHeight, and z
			temp = PhotonNetwork.Instantiate("Meteor", new Vector3(theX, yDropHeight, theZ), Quaternion.Euler(new Vector3(Random.Range(0,30),0,Random.Range(0,30))),0) as GameObject;
            temp.GetComponent<Rigidbody>().AddForce(Vector3.forward * 5,ForceMode.VelocityChange);
            spawned++;
			timer = 0.0f;
		}//End if
	}//End Update()
}//End FallingObjectGenerator
