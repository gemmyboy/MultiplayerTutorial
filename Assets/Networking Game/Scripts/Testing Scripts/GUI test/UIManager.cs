using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UIManager : Photon.MonoBehaviour {
    public GameObject PingLabel;
    public GameObject FPSLabel;

    public GameObject ammoLabel;
    public GameObject healthLabel;
    public GameObject killsLabel;
    public GameObject deathLabel;
    public GameObject assistLabel;
    
    public  float updateInterval = 0.5F;
    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    Color pingColor;
	// Use this for initialization
	void Start () {
        if(!photonView.isMine){
            gameObject.SetActive(false);
        }

        timeleft = updateInterval;
        pingColor = PingLabel.GetComponent<Image>().color;
	}
	
	// Update is called once per frame
	void Update () {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale/Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if( timeleft <= 0.0 )
        {
        // display two fractional digits (f2 format)
	    float fps = accum/frames;
        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //Change the color of the FPS Label
        FPSLabel.GetComponentInChildren<Text>().text = "FPS: " + (int)fps;
        Color FPSColor = FPSLabel.GetComponent<Image>().color;

        if (fps < 30)
        {
            FPSLabel.GetComponent<Image>().color = new Color(255,255,0,FPSColor.a);
        }
        else
        {
            if (fps < 10)
            {
                FPSLabel.GetComponent<Image>().color = new Color(255,0,0,FPSColor.a);
            }
            else
            {
                FPSLabel.GetComponent<Image>().color = new Color(0,250,0,FPSColor.a);
            }
        }
        //-----------------------------------------------------------------------------------------
        PingLabel.GetComponentInChildren<Text>().text = "Ping: " + PhotonNetwork.GetPing();

        if (PhotonNetwork.GetPing() >= 175)
        {
            PingLabel.GetComponent<Image>().color = new Color(255, 0, 0, pingColor.a);
        }
        else if(PhotonNetwork.GetPing() < 175 && PhotonNetwork.GetPing() >= 100){
            PingLabel.GetComponent<Image>().color = new Color(255, 225, 0, pingColor.a);
        }
        else if (PhotonNetwork.GetPing() < 100)
        {
            PingLabel.GetComponent<Image>().color = new Color(0, 255, 0, pingColor.a);
        }

        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        timeleft = updateInterval;
        accum = 0.0F;
        frames = 0;
        }
	}

    public void ChangeAmmo(int ammo)
    {
        ammoLabel.GetComponentInChildren<Text>().text = "" + ammo;
        Color ammoColor = ammoLabel.GetComponentInChildren<Image>().color;

        if(ammo >= 75){
            Debug.Log("yes");
            ammoLabel.GetComponentInChildren<Image>().color = new Color(0, 255, 0, ammoColor.a);
            Debug.Log(ammoLabel.GetComponentInChildren<Image>().color);
        }
        else if(ammo <= 50 && ammo > 25){
            ammoLabel.GetComponentInChildren<Image>().color = new Color(255, 255, 0, ammoColor.a);
        }
        else if (ammo <= 25)
        {
            ammoLabel.GetComponentInChildren<Image>().color = new Color(255, 0, 0, ammoColor.a);
        }
    }

    public void ChangeHealth(int health)
    {
        healthLabel.GetComponentInChildren<Text>().text = "" + health;
    }

    public void changeKills(int kills)
    {
        killsLabel.GetComponentInChildren<Text>().text = "" + kills;
    }
    public void changeDeaths(int deaths)
    {
        deathLabel.GetComponentInChildren<Text>().text = "" + deaths;

    }
    public void changeAssist(int assist)
    {
        assistLabel.GetComponentInChildren<Text>().text = "" + assist;
    }
}
