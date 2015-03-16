using UnityEngine;
using System.Collections;

public class Kills_Deaths_Assist : MonoBehaviour {
    public int kills;
    public int assist;
    public int deaths;
    public UIManager uiManager;
	// Use this for initialization
	void Start () {
        uiManager = FindObjectOfType<UIManager>();
        kills = (int)PhotonNetwork.player.customProperties["Kills"];
        deaths = (int)PhotonNetwork.player.customProperties["Deaths"]; ;
        assist = (int)PhotonNetwork.player.customProperties["Assist"]; ;
        uiManager.changeKills(kills);
        uiManager.changeDeaths(deaths);
        uiManager.changeAssist(assist);
	}

    public void updateKills()
    {
        PhotonNetwork.player.customProperties["Kills"] = (int)PhotonNetwork.player.customProperties["Kills"] + 1;
    }
    public void updateDeaths()
    {
        PhotonNetwork.player.customProperties["Deaths"] = (int)PhotonNetwork.player.customProperties["Deaths"] + 1;
    }
    public void updateAssist()
    {
        PhotonNetwork.player.customProperties["Assist"] = (int)PhotonNetwork.player.customProperties["Assist"] + 1;
    }
}
