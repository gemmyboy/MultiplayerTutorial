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
    public GameObject GameTimeUI;


    public RectTransform bulletTimerRect;
    public RectTransform laserTimerRect;
    public RectTransform shieldTimerRect;

    public bool bulletShot = false;
    public bool laserShot = false;
    public bool shieldShot = false;

    public int roundTimeLimitMins;

    public  float updateInterval = 0.5F;
    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    //Color pingColor;
	// Use this for initializatio
	void Start () {
        bulletTimerRect = GameObject.Find("ReloadBulletTimer").transform.Find("GreenImage").GetComponent<RectTransform>();
        shieldTimerRect = GameObject.Find("ShieldTimer").transform.Find("GreenImage").GetComponent<RectTransform>();
        
        timeleft = updateInterval;
        GameTimeUI.GetComponentInChildren<Text>().text = "" + roundTimeLimitMins + ":00";
        activateLaserTimer();
	}
	
	// Update is called once per frame
	void Update () {
        //FPS
        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        timeleft -= Time.deltaTime;
        accum += Time.timeScale/Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if( timeleft <= 0.0 )
        {
            // display two fractional digits (f2 format)
	        float fps = accum/frames;
            //Change the color of the FPS Label
            FPSLabel.GetComponentInChildren<Text>().text = "" + (int)fps;
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
            PingLabel.GetComponentInChildren<Text>().text = "" + PhotonNetwork.GetPing();
            //-----------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------
            timeleft = updateInterval;
            accum = 0.0F;
            frames = 0;
        }
        //Ready timers
        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------
        if(bulletShot){
            if(bulletTimerRect.offsetMax.x < 0){
                bulletTimerRect.offsetMax += new Vector2(Time.deltaTime * 250/3, 0.0f);
            }
            else
            {
                bulletShot = false;
            }
        }
        if(laserShot){
            if (laserTimerRect.offsetMax.x < 0)
            {
                laserTimerRect.offsetMax += new Vector2(Time.deltaTime * 100, 0.0f);
            }
            else
            {
                laserShot = false;
            }
        }
        if(shieldShot){
            if (shieldTimerRect.offsetMax.x < 0)
            {
                shieldTimerRect.offsetMax += new Vector2(Time.deltaTime * 19.5f, 0.0f);
            }
            else
            {
                shieldShot = false;
            }
        }
	}

    public void ChangeAmmo(int ammo)
    {
        ammoLabel.GetComponentInChildren<Text>().text = "" + ammo;
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

    public void activateLaserTimer(){
        if(PhotonNetwork.room.customProperties["GameType"].ToString() == "OmegaTank"){
            if(PhotonNetwork.player.customProperties["TheOmega"].ToString() == "1"){
                laserTimerRect.parent.gameObject.SetActive(true);
                laserTimerRect = GameObject.Find("LaserTimer").transform.Find("GreenImage").GetComponent<RectTransform>();
            }
        }
    }

    public void setToZero(GameObject obj)
    {
        RectTransform rectTransform = obj.transform.Find("GreenImage").GetComponent<RectTransform>();
        rectTransform.offsetMax = new Vector2(-250,0);
    }
}
