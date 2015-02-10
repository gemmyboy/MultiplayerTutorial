using UnityEngine;
using System.Collections;
using Photon;
public class testPing : PunBehaviour {

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Ping: " + PhotonNetwork.GetPing());
    }
}
