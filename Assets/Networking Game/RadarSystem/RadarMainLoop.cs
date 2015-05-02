using UnityEngine;
using System.Collections;

public class RadarMainLoop : MonoBehaviour {


	//Obvious
	private GameObject theRadarSystem;


	// Use this for initialization
	void Start () 
	{
		theRadarSystem = GameObject.Find("RadarSystem");
	}//End Start()



	// Update is called once per frame
	void FixedUpdate () 
	{
		//If the RadarSystem completed it's sweep. Start and do a new one.
		if(theRadarSystem.GetComponent<RadarSystem>().RadarSweepisCompleted())
		{
			//Get player positions here into this array--vvvvv
			//float [,] thePlayerPositions;

			//theRadarSystem.GetComponent<RadarSystem>().RequestRadarSweep(thePlayerPositions);

		}//End if
	}//End FixedUpdate
}//RadarMainLoop Class
