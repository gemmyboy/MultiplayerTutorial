using UnityEngine;
using System.Collections;

public class CameraSwitcher: MonoBehaviour {
	
	public int cameraChangeCount = 0;

	private Camera Maincam;
	private AudioListener Listener;
	public MainFixedCam FixedCams;
	private ChildFixedCam[] ChildFixedCams;

	void Start () {
		Maincam = GetComponent<Camera>();
		Listener = GetComponent<AudioListener>();
		ChildFixedCams = FixedCams.GetComponentsInChildren<ChildFixedCam>();
		cameraChangeCount = 0;
	}

	void Update(){
		
		if(Input.GetKeyDown(KeyCode.C))
			ChangeCamera();
		
	}

	void ChangeCamera () {

		cameraChangeCount ++;

		if(cameraChangeCount > 2)
			cameraChangeCount = 0;
	
		switch(cameraChangeCount){
		case 0:
			Maincam.enabled = true;
			FixedCams.canTrackNow = false;
			foreach(ChildFixedCam c in ChildFixedCams)
				c.freeCam = false;
			if(!Listener.enabled)
				Listener.enabled = true;
			break;
		case 1:
			Maincam.enabled = false;
			FixedCams.canTrackNow = true;
			foreach(ChildFixedCam c in ChildFixedCams)
				c.freeCam = false;
			if(!Listener.enabled)
				Listener.enabled = false;
			break;
		case 2:
			Maincam.enabled = false;
			FixedCams.canTrackNow = true;
			foreach(ChildFixedCam c in ChildFixedCams)
				c.freeCam = true;
			if(!Listener.enabled)
				Listener.enabled = false;
			break;
		}

	}



}
