using UnityEngine;
using System.Collections;

public class HRezDemoControls : MonoBehaviour {

	public string packageNameLabel = "High-Rez Beams - Weapon Systems Pack ";
	public string versionLabel = " v1.0";
	public string packageCreatorLabel = "Created by, Daniel Kole Productions";

	public Transform BeamTarget1Transform;
	public Transform BeamTarget2Transform;
	public Transform BeamTarget3Transform;
	public Transform BeamTarget4Transform;
	public Transform BeamTarget5Transform;
	public WSP_HighRezLaser HighRezLaser;
	public int CurrentHRezLaser = 0;
	public WSP_HighRezLaser[] HRezLaserList;
	public float TargetPointFollowSpeed = 5;

	public bool LockMainCamera = false;

	public int CurrentCameraPos = 1;
	public float CameraMoveSpeed = 2.0f;
	public float CameraRotationSpeed = 0.25f;
	private Transform cameraTransform;
	public Vector3 CameraPos1 = Vector3.zero;
	public Vector3 CameraRotation1 = Vector3.zero;
	public Vector3 CameraPos2 = Vector3.zero;
	public Vector3 CameraRotation2 = Vector3.zero;
	public Vector3 CameraPos3 = Vector3.zero;
	public Vector3 CameraRotation3 = Vector3.zero;

	// Use this for initialization
	void Start () {
		if (HRezLaserList.Length > 0) {
			HighRezLaser = HRezLaserList[CurrentHRezLaser];
		}
		cameraTransform = Camera.main.gameObject.transform;
		// Assign Targets to Beams
		for (int i = 0; i < HRezLaserList.Length; i++) {
			if (i == 0)
				HRezLaserList[i].CurrentTarget = BeamTarget1Transform;
			else if (i == 1)
				HRezLaserList[i].CurrentTarget = BeamTarget2Transform;
			else if (i == 2)
				HRezLaserList[i].CurrentTarget = BeamTarget3Transform;
			else if (i == 3)
				HRezLaserList[i].CurrentTarget = BeamTarget4Transform;
			else if (i == 4)
				HRezLaserList[i].CurrentTarget = BeamTarget5Transform;
		}
	}

	private void UpdateCurrentHRezLaser() {
		HighRezLaser = HRezLaserList[CurrentHRezLaser];
	}

	public bool ShowGUI = true;

	void OnGUI() {
		if (ShowGUI) {
			GUI.Label(new Rect(10, 10, 250, 50), packageNameLabel + versionLabel + "\n" + packageCreatorLabel);
			string instructionsString = "Demo Instructions:";
			instructionsString += "\n" + "Click and Hold Left Mouse Button on Desired Target to Fire Beam.";
			instructionsString += "\n" + "Note: You can move mouse while holding to move target position).";
			instructionsString += "\n";
			instructionsString += "\n" + "Active High-Rez Beam (Up Arrow/Down Arrow): " + HRezLaserList[CurrentHRezLaser].GUIDisplayString;
			instructionsString += "\n" + "Press the 'C' Key to change the Camera's location. ";
			GUI.Label(new Rect(10, 60, 450, 450), instructionsString);
		}
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.Mouse0)) {
			UpdateMouseTargeting();
		}
		if (Input.GetKeyUp(KeyCode.C)) {
			ChangeCamera();
		}

		// Change HRez Laser
		if (Input.GetKeyUp(KeyCode.UpArrow)) {
			if (CurrentHRezLaser > 0) {
				CurrentHRezLaser -= 1;
			}
			else {
				CurrentHRezLaser = HRezLaserList.Length - 1;
			}
			UpdateCurrentHRezLaser();
		}
		if (Input.GetKeyUp(KeyCode.DownArrow)) {
			if (CurrentHRezLaser < HRezLaserList.Length - 1) {
				CurrentHRezLaser += 1;
			}
			else {
				CurrentHRezLaser = 0;
			}
			UpdateCurrentHRezLaser();
		}

		if (LockMainCamera) {
			if (CurrentCameraPos == 1) {
				cameraTransform.position = Vector3.Lerp(cameraTransform.position, CameraPos1, CameraMoveSpeed * Time.deltaTime);
				cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.Euler(CameraRotation1), CameraRotationSpeed * Time.deltaTime);
			}
			else if (CurrentCameraPos == 2) {
				cameraTransform.position = Vector3.Lerp(cameraTransform.position, CameraPos2, CameraMoveSpeed * Time.deltaTime);
				cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.Euler(CameraRotation2), CameraRotationSpeed * Time.deltaTime);
			}
			else if (CurrentCameraPos == 3) {
				cameraTransform.position = Vector3.Lerp(cameraTransform.position, CameraPos3, CameraMoveSpeed * Time.deltaTime);
				cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.Euler(CameraRotation3), CameraRotationSpeed * Time.deltaTime);
			}
		}
	}

	private void ChangeCamera() {
		if (CurrentCameraPos == 1) {
			CurrentCameraPos = 2;
		}
		else if (CurrentCameraPos == 2) {
			CurrentCameraPos = 3;
		}
		else if (CurrentCameraPos == 3) {
			CurrentCameraPos = 1;
		}
	}

	private RaycastHit mouseHit;
	private Ray vRay;
	private void UpdateMouseTargeting() {
		if (HighRezLaser == HRezLaserList[CurrentHRezLaser]) {
			if (!HRezLaserList[CurrentHRezLaser].LaserFiring) {
				mouseHit = new RaycastHit();
				vRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				if(Physics.Raycast(vRay, out mouseHit, 1000))
				{
					HRezLaserList[CurrentHRezLaser].CurrentTarget.position = mouseHit.point;
					HRezLaserList[CurrentHRezLaser].FireLaser();
				}
			}
			else {
				// Laser Already Firing Just Update Target Position Lerp
				mouseHit = new RaycastHit();
				vRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				if(Physics.Raycast(vRay, out mouseHit, 1000))
				{
					HRezLaserList[CurrentHRezLaser].CurrentTarget.position = Vector3.Lerp(HRezLaserList[CurrentHRezLaser].CurrentTarget.position, mouseHit.point, TargetPointFollowSpeed * Time.deltaTime);
				}
			}
		}
		else {
			HighRezLaser = HRezLaserList[CurrentHRezLaser];
		}
	}
}
