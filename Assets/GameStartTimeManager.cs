using System.IO;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;
using UnityEngine.UI;
public class GameStartTimeManager : PunBehaviour
{
    private const string GameTime = "GameTime";
    private double timeToStart = 0.0f;
    public double SecondsBeforeEnd = 11.0f; // set in inspector
    public GameObject startTimeUI;
    public double time;

    public bool IsItTimeYet
    {
        get { return IsTimeToStartKnown && PhotonNetwork.time > this.timeToStart; }
    }
    public bool IsTimeToStartKnown
    {
        get { return this.timeToStart > 0.001f; }
    }
    public double SecondsUntilItsTime
    {
        get
        {
            if (this.IsTimeToStartKnown)
            {
                double delta = this.timeToStart - PhotonNetwork.time;
                return (delta > 0.0f) ? delta : 0.0f;
            }
            else
            {
                return 0.0f;
            }
        }
    }
    void Update()
    {
        if (PhotonNetwork.isMasterClient)
        {
            // the master client checks if a start time is set. we check a min value
            if (!this.IsTimeToStartKnown && PhotonNetwork.time > 0.0001f)
            {
                // no startTime set for room. calculate and set it as property of this room
                this.timeToStart = PhotonNetwork.time + SecondsBeforeEnd;
                Hashtable timeProps = new Hashtable() { { GameTime, this.timeToStart } };
                PhotonNetwork.room.SetCustomProperties(timeProps);
            }
        }
        time = this.SecondsUntilItsTime;
        startTimeUI.GetComponentInChildren<Text>().text = "" + (int)time;
        if((int)time == 0){
            Destroy(startTimeUI);
        }
    }
    public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(GameTime))
        {
            this.timeToStart = (double)propertiesThatChanged[GameTime];
            Debug.Log("Got GameTime: " + this.timeToStart + " is it time yet?! " + this.IsItTimeYet);
        }
    }
    void OnGUI()
    {
        //GUILayout.Label("Is it time yet: " + this.IsItTimeYet);
        //GUILayout.Label("Seconds until it's time: " + (float)this.SecondsUntilItsTime);
    }
}