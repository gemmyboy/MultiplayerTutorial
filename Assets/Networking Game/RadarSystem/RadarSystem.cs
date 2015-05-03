using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadarSystem : MonoBehaviour 
{
	//So we can access it's position when an update request comes in
	public GameObject updateGradient;
	public GameObject theRadarSystem;

	//Do a minimap update?
	private bool doUpdate = false;
	private float updateTimer;
	private float updateTimerLimit;

	//Dimensions of the map
	public int worldWidth;		//X Dimension
	public int worldHeight;		//Y Dimension

	//Each index will contain an int array with 3 pieces of data
	//	1st int will stand for one of the following:
	//		0 = current player
	//		1 = ally player
	//		2 = enemy player
	//	2nd int will stand for x world cord position
	//	3rd int will stand for y world cord position
	private float [,] playerPositions;

	//Speed in which the UpdateGradient will move downwards
	private float updateSpeed = 0.05f;

	//List of cords we already put on the map
	private List<int> postedList;

	//List of all currently instantiaed objects on minimap
	private List<GameObject> listCordsPosted;

	//List of prefabs
	private Object currentPlayer;
	private Object allyPlayer;
	private Object enemyPlayer;

	//List start cord
	private float gradientYCord;

	// Use this for initialization
	void Start () 
	{
		//Obvious
		updateGradient = GameObject.Find("UpdateLineThing");
		theRadarSystem = GameObject.Find ("RadarSystem");

		//Assign World Dimensions here
		worldWidth = 350;
		worldHeight = 350;

		//Calculate the amount of time it'll take to reach the border based on speed
		updateTimerLimit = 4 / updateSpeed;

		//Make invisible
		updateGradient.SetActive(false);

		postedList = new List<int>();
		listCordsPosted = new List<GameObject>();

		//Load in all of the prefabs
        currentPlayer = Resources.LoadAssetAtPath("Assets/Networking Game/RadarSystem/CurrentPlayer.prefab", typeof(Object));
        allyPlayer = Resources.LoadAssetAtPath("Assets/Networking Game/RadarSystem/AllyPlayer.prefab", typeof(Object));
		enemyPlayer = Resources.LoadAssetAtPath("Assets/Networking Game/RadarSystem/EnemyPlayer.prefab", typeof(Object));

		//Test DEBUG CODE--vv Forces a single sweep
		float [,] temp = new float[3,3] { {0.0f, 175.0f, 175.0f}, {1.0f, 50.0f, 50.0f}, {2.0f, 340.0f, 340.0f} };

		RequestRadarSweep(temp);

	}//End Start()

	//Called outside of this class so that the MiniMap will know when to update
	public void RequestRadarSweep(float [,] newPlayerPositions)
	{
		playerPositions = new float[newPlayerPositions.GetLength(0), 3];

		//Update the new Player Positions
		for (int i = 0; i < newPlayerPositions.GetLength(0); i++)
		{
			//For current/ally/enemy marker
			playerPositions[i, 0] = newPlayerPositions[i, 0];

			//For hashing the x value
			playerPositions[i, 1] = newPlayerPositions[i, 1] / (worldWidth / 4);

			//For hashing the y value
			playerPositions[i, 2] = newPlayerPositions[i, 2] / (worldHeight / 4);
		}//End foreach

		//Let update() know that it's time to update the minimap
		doUpdate = true;

		//Reset updateTimer
		updateTimer = 0.0f;

		//Make the gradient visible
		updateGradient.SetActive(true);

		gradientYCord = 0.0f;

	}//End RequestRadarSweep()

	public bool RadarSweepisCompleted()
	{
		if(doUpdate)
			return false;
		else
			return true;
	}//End RadarSweepisCompleted()
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		Debug.Log("Player Position: " + playerPositions[0,2]);
		if(doUpdate)
		{
			for( int i = 0; i < playerPositions.GetLength(0); i++)
			{
				if(!postedList.Contains(i) && gradientYCord > (playerPositions[i,2]))
				{


					//Recalculate the translation needed based on Minimap
					float xTemp;
					float yTemp;

					//For xCords
					if(playerPositions[i,1] > 2)
					{
						xTemp = playerPositions[i,1] - 2;
					}//End if
					else if (playerPositions[i,1] < 2)
					{
						xTemp = (playerPositions[i,1] - 2);
					}//End else if
					else
					{
						xTemp = 0;
					}//End else

					//For yCords
					if(playerPositions[i,2] > 2)
					{
						yTemp = playerPositions[i,2] - 2;
					}//End if
					else if (playerPositions[i,2] < 2)
					{
						yTemp = (playerPositions[i,2] - 2);
					}//End else if
					else
					{
						yTemp = 0;
					}//End else

					//Pushes in which direction
					Vector3 aVector = new Vector3(xTemp, 0.11f, yTemp);

					GameObject temp;

					switch((int) playerPositions[i,0])
					{
					//Instantiation place for current player
					case 0:
						temp = GameObject.Instantiate(currentPlayer, theRadarSystem.transform.position, theRadarSystem.transform.rotation) as GameObject;
						temp.transform.Translate(aVector);
						listCordsPosted.Add(temp);
						postedList.Add(i);
						break;
					case 1:
						temp = GameObject.Instantiate(allyPlayer, theRadarSystem.transform.position, theRadarSystem.transform.rotation) as GameObject;
						temp.transform.Translate(aVector);
						listCordsPosted.Add(temp);
						postedList.Add(i);
						break;
					case 2:
						temp = GameObject.Instantiate(enemyPlayer, theRadarSystem.transform.position, theRadarSystem.transform.rotation) as GameObject;
						temp.transform.Translate(aVector);
						listCordsPosted.Add(temp);
						postedList.Add(i);
						break;

					}//End Switch
				}//End if
			}//End for

			//Move the updateGradient down slowly
			updateGradient.transform.Translate(0, 0, 1 * updateSpeed);

			//Update change in Ycord
			gradientYCord = gradientYCord + updateSpeed;

			//Increment timer
			updateTimer++;

			if(updateTimerLimit < updateTimer)
			{
				//Push the update Gradient back up
				updateGradient.transform.Translate(0, 0, -1 * 4);
				updateGradient.SetActive(false);
				doUpdate = false;

			}//End if
		}//End if

	}//End Update
}//End RadarSystem
