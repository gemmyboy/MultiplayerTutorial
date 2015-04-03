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

	public int yDropHeight = 20;

	//Instantiation limit
	static public int spawned = 0;
	public int spawnLimit = 40;

	// Use this for initialization
	void Start () 
	{
		//initialize
		theObjects = new GameObject[8];

		//Load all of the prefabs
		theObjects[0] = Resources.LoadAssetAtPath("Assets/FallingObjects/Objects/AtomBall.prefab", typeof(Object));
		theObjects[1] = Resources.LoadAssetAtPath("Assets/FallingObjects/Objects/BombBall.prefab", typeof(Object));
		theObjects[2] = Resources.LoadAssetAtPath("Assets/FallingObjects/Objects/BuckyBall.prefab", typeof(Object));
		theObjects[3] = Resources.LoadAssetAtPath("Assets/FallingObjects/Objects/EyeBall.prefab", typeof(Object));
		theObjects[4] = Resources.LoadAssetAtPath("Assets/FallingObjects/Objects/SpikeBall.prefab", typeof(Object));
		theObjects[5] = Resources.LoadAssetAtPath("Assets/FallingObjects/Objects/SplitMetalBall.prefab", typeof(Object));
		theObjects[6] = Resources.LoadAssetAtPath("Assets/FallingObjects/Objects/WheelBall.prefab", typeof(Object));
		theObjects[7] = Resources.LoadAssetAtPath("Assets/FallingObjects/Objects/WoodenBall.prefab", typeof(Object));

	}//End Start()
	
	// Update is called once per frame
	void Update () 
	{
		if(spawned < spawnLimit)
		{
			int theX = 0;
			int theZ = 0;
			int theBall = 0;
			GameObject temp;

			//Randomly pick a point between xMin and xMax
			theX = Random.Range(xMin, xMax);

			//Randomly pick a point between yMin and yMax
			theZ = Random.Range(zMin, zMax);

			//Randomly pick a ball
			theBall = Random.Range(0, 8);

			//Instantiate at the designated x, yDropHeight, and z
			temp = Instantiate(theObjects[theBall], new Vector3(theX, yDropHeight, theZ), Quaternion.Euler(Vector3.forward)) as GameObject;
			spawned++;
		}//End if
	}//End Update()
}//End FallingObjectGenerator
