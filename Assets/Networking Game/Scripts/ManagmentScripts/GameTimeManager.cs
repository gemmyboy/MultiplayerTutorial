using System.IO;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;
using UnityEngine.UI;
public class GameTimeManager : PunBehaviour{

    private const string TimeToStartProp = "st";
    private double timeToStart = 0.0f;
    public double SecondsBeforeStart; // set in inspector

    public GameObject GameTimeUI;
    GameStartTimeManager startTimer;

    public double time;

    UIManager uimanager;
    LeaderBoardsDisplayer leaderBoards;

    bool gameOver = false;
    void Start(){
        leaderBoards = FindObjectOfType<LeaderBoardsDisplayer>();
        uimanager = FindObjectOfType<UIManager>();
        SecondsBeforeStart = uimanager.roundTimeLimitMins * 60 + GameObject.FindObjectOfType<GameStartTimeManager>().SecondsBeforeEnd;
        startTimer = FindObjectOfType<GameStartTimeManager>();
        timeToStart = startTimer.timeToStart;
    }

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
                this.timeToStart = PhotonNetwork.time + SecondsBeforeStart;
                Hashtable timeProps = new Hashtable() { { TimeToStartProp, this.timeToStart } };
                PhotonNetwork.room.SetCustomProperties(timeProps);
            }
        }
        
        if(startTimer.IsItTimeYet){
            time = this.SecondsUntilItsTime;
            changeToNormalTime((int)time);
            if ((int)time == 0 && !gameOver)
            {
                StartCoroutine("displayLeaderboards");
                gameOver = true;
            }
        }
    }

    public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(TimeToStartProp))
        {
            this.timeToStart = (double)propertiesThatChanged[TimeToStartProp];
        }
    }
    
    public void changeToNormalTime(int numOfSeconds)
    {
        int minutes = (int)(numOfSeconds / 60);
        int seconds = (int)(numOfSeconds % 60);
        if (seconds >= 10)
        {
            GameTimeUI.GetComponentInChildren<Text>().text = "" + minutes + ":" + seconds;
        }
        else
        {
            GameTimeUI.GetComponentInChildren<Text>().text = "" + minutes + ":0" + seconds;
        }
    }

    System.Collections.IEnumerator displayLeaderboards()
    {
        yield return new WaitForSeconds(2.0f);
        leaderBoards.leaderBoards.SetBool("Faded", true);
        leaderBoards.leaderBoards.enabled = false;
    }

}
