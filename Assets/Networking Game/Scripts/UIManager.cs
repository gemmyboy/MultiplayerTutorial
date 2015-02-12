using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UIManager : Photon.MonoBehaviour {
    public GameObject PingLabel;
    public GameObject ammoLabel;
    public GameObject FPSLabel;
    
    public  float updateInterval = 0.5F;
    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    TankGunController[] tankGuns;
    TankGunController m_gunController;
	// Use this for initialization
	void Start () {
        //tankGuns = FindObjectsOfType<TankGunController>();
        //foreach(TankGunController gun in tankGuns){
        //    if(gun.GetComponent<PhotonView>().isMine){
        //        m_gunController = gun.GetComponent<TankGunController>();
        //    }
        //}

        timeleft = updateInterval;
        //ammoLabel.GetComponentInChildren<Text>().text = "Ammo: " + m_gunController.ammo;
	}
	
	// Update is called once per frame
	void Update () {
	    PingLabel.GetComponentInChildren<Text>().text = "Ping: " + PhotonNetwork.GetPing();


        timeleft -= Time.deltaTime;
        accum += Time.timeScale/Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if( timeleft <= 0.0 )
        {
        // display two fractional digits (f2 format)
	    float fps = accum/frames;
        FPSLabel.GetComponentInChildren<Text>().text = "FPS: " + (int)fps;

        if (fps < 30)
            FPSLabel.GetComponentInChildren<Image>().color = Color.yellow;
        else
            if (fps < 10)
                FPSLabel.GetComponentInChildren<Image>().color = Color.red;
            else
                FPSLabel.GetComponentInChildren<Image>().color = Color.green;

            timeleft = updateInterval;
            accum = 0.0F;
            frames = 0;
        }
	}

    public void ChangeAmmo(int ammo)
    {
        ammoLabel.GetComponentInChildren<Text>().text = "Ammo: " + ammo;
    }
}
