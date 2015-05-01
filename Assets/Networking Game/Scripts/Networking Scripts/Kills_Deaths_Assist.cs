using UnityEngine;
using System.Collections;

public class Kills_Deaths_Assist : MonoBehaviour {
    public int kills;
    public int captures;
    public int deaths;
    public UIManager uiManager;
	// Use this for initialization
	void Start () {
        uiManager = FindObjectOfType<UIManager>();
        kills = (int)PhotonNetwork.player.customProperties["Kills"];
        deaths = (int)PhotonNetwork.player.customProperties["Deaths"];
        if(PhotonNetwork.room.customProperties["GameType"].ToString() == "Capture The Flag"){
            captures = (int)PhotonNetwork.player.customProperties["Captures"];
            uiManager.changeCaptures(captures);
        }
        uiManager.changeKills(kills);
        uiManager.changeDeaths(deaths);
	}
	/*
    public void updateKills()
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        kills = (int)PhotonNetwork.player.customProperties["Kills"] + 1;
        hash.Add("Kills", kills);
        PhotonNetwork.SetPlayerCustomProperties(hash);
    }
    public void updateDeaths()
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        deaths = (int)PhotonNetwork.player.customProperties["Deaths"] + 1;
        hash.Add("Deaths", deaths);
        PhotonNetwork.SetPlayerCustomProperties(hash);
    }
    public void updateAssist()
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        assist = (int)PhotonNetwork.player.customProperties["Assist"] + 1;
        hash.Add("Deaths", assist);
        PhotonNetwork.SetPlayerCustomProperties(hash);
    }
    */
}
