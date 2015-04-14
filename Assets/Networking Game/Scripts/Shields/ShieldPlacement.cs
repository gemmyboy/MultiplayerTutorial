using UnityEngine;
using System.Collections;

public class ShieldPlacement : Photon.MonoBehaviour {
    public GameObject shield;
    public float cooldownTime = 60.0f;
    public float timer = 0.0f;
    UIManager guiManager;
	void Start () {
        timer = cooldownTime;
        guiManager = FindObjectOfType<UIManager>();
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        if(PhotonNetwork.room.customProperties["GameType"].ToString() == "OmegaTank"){
            if (PhotonNetwork.player.customProperties["TheOmega"].ToString() != "1")
            {
                GameObject teamShield = PhotonNetwork.Instantiate("Game_Shield", transform.position, transform.rotation, 0);
                timer = 0;
                guiManager.shieldShot = true;
                guiManager.setToZero(guiManager.shieldTimerRect.parent.gameObject);
            }
        }
        else{
	        if(Input.GetKeyDown(KeyCode.F) && timer > cooldownTime  && photonView.isMine){
                GameObject teamShield = PhotonNetwork.Instantiate("Game_Shield",transform.position,transform.rotation,0);
                timer = 0;
                guiManager.shieldShot = true;
                guiManager.setToZero(guiManager.shieldTimerRect.parent.gameObject);
            }
        }
	}
}
