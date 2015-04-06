using UnityEngine;
using System.Collections;

public class ShieldPlacement : Photon.MonoBehaviour {
    public GameObject shield;
    public float cooldownTime = 10.0f;
    public float timer = 0.0f;
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
	    if(Input.GetKeyDown(KeyCode.F) && timer > cooldownTime  && photonView.isMine){
            GameObject teamShield = PhotonNetwork.Instantiate("Game_Shield",transform.position,transform.rotation,0);
            timer = 0;
        }
	}
}
