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
        kills = 0;
        deaths = 0;
        assist = 0;
        uiManager.changeKills(kills);
        uiManager.changeDeaths(deaths);
        uiManager.changeAssist(assist);
	}

    public void updateKills()
    {
        kills += 1;
    }
    public void updateDeaths()
    {
        deaths += 1;
    }
    public void updateAssist()
    {
        assist += 1;
    }
}
