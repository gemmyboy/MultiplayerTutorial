/* Third person Camera Controller
 * 	-Used primarily for mouse control of the camera
 * */
using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public GameObject theCamera;
	public GameObject thePlayer;

	public int cameraVelocity = 250; 
	public float distance = 10f;		//Only the initial distance

	private float yMin = -20.0f;		//Governs bounds for camera on y axis
	private float yMax = 80.0f; 		//^^^^

	private float tempX = 0.0f;		//Temp Vars
	private float tempY = 0.0f;

	// Use this for initialization
	void Start () 
	{
		theCamera = GameObject.Find("Main Camera");
		thePlayer = GameObject.Find ("Plane");

		tempX = theCamera.transform.eulerAngles.x;
		tempY = theCamera.transform.eulerAngles.y;

		//Freeze the Rigidbody rotation for some reason
		if(theCamera.GetComponent<Rigidbody>())
			theCamera.GetComponent<Rigidbody>().freezeRotation = true;

	}//End Start()
	
	// Update is called once per frame
	void FixedUpdate () 
	{

		if(Input.GetMouseButton(2) || Input.GetAxis("Mouse ScrollWheel") != 0.0f)//Middle mouse button clicked
		{
			tempX += Input.GetAxis("Mouse X") * cameraVelocity * 0.02f;
			tempY -= Input.GetAxis("Mouse Y") * cameraVelocity * 0.02f;

			tempY = ClampAngle(tempY, yMin, yMax);

			distance = Mathf.Clamp(distance - (Input.GetAxis("Mouse ScrollWheel")*6), 3, 30);

			Quaternion newRot = Quaternion.Euler(tempY, tempX, 0.0f);
			Vector3 newPos = new Vector3(0.0f, 0.0f, (-1 * distance));
			newPos = newRot * newPos + thePlayer.transform.position;

			theCamera.transform.rotation = newRot;
			theCamera.transform.position = newPos;

		}//End if
	
	}//End Update()

	//Clamp the actual angle the Camera is pointing towards
	static float ClampAngle(float y, float min, float max)
	{
		if(y < -360)
			y += 360;
		if(y > 360)
			y -= 360;
		return Mathf.Clamp(y, min, max);
	}//End Clamp()


}//End CameraController
