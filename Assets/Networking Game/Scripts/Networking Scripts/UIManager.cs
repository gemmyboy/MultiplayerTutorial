﻿using UnityEngine;
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

    public int roundTimeLimitMins;

    public  float updateInterval = 0.5F;
    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    //Color pingColor;
	// Use this for initializatio
	void Start () {
        if(!photonView.isMine){
            //gameObject.SetActive(false);
        }

        timeleft = updateInterval;
        //pingColor = PingLabel.GetComponent<Image>().color;

        GameTimeUI.GetComponentInChildren<Text>().text = "" + roundTimeLimitMins + ":00";
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
	}

    public void ChangeAmmo(int ammo)
    {
        ammoLabel.GetComponentInChildren<Text>().text = "" + ammo;
        //Color ammoColor = ammoLabel.GetComponentInChildren<Image>().color;
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
