using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon;
public class LeaderBoardsDisplayer : PunBehaviour {
    public Animator leaderBoards;

    public GameObject LeaderBoardsWindow;

	public GameObject teamLabel;
	public GameObject playerLayout;
	public GameObject playerlabel;

	public Color eagleColor;
	public Color exorcistColor;
	public Color wolfColor;
	public Color bloodColor;

	// Use this for initialization
	void Start () {
        LeaderBoardsWindow.transform.Find("Title").transform.Find("TitleLabel").GetComponent<Text>().text = PhotonNetwork.room.customProperties["GameType"].ToString();
		createLeaderboards ();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            leaderBoards.SetBool("Faded", false);
        }
        if (Input.GetKeyUp(KeyCode.CapsLock))
        {
            leaderBoards.SetBool("Faded", true);
        }
	}

    public void fixLabel(PhotonPlayer player,int j)
    {
        //label.transform.parent = LeaderBoardsWindow.gameObject.transform;
        //label.transform.localScale = new Vector3(1, 1, 1);
        //label.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, (-50 * j) + 140, 0);
        //label.transform.rotation = new Quaternion(0, 0, 0, 0);
        //label.GetComponentInChildren<Text>().text = (j + 1) + ". " + player.name + "--" + player.GetScore();
    }

	List<string> teams = new List<string>();
	List<string> playerList = new List<string>();

	public void createLeaderboards(){
		foreach(PhotonPlayer players in PhotonNetwork.playerList){
			if(teams.Contains(players.customProperties["Team"].ToString()) == false){
				teams.Add(players.customProperties["Team"].ToString());
				playerList.Add(players.name);
			}
		}
		int i = 0;
		foreach(string team in teams){
			GameObject teamLab = Instantiate(teamLabel,LeaderBoardsWindow.transform.position,LeaderBoardsWindow.transform.rotation) as GameObject;
			teamLab.transform.SetParent(LeaderBoardsWindow.transform.Find("LayoutTeamNames"));
			teamLab.GetComponentInChildren<Text>().text = team;
			checkColor(teamLab,team);


			GameObject plyLayout = Instantiate(playerLayout,LeaderBoardsWindow.transform.position,LeaderBoardsWindow.transform.rotation) as GameObject;
			plyLayout.transform.SetParent(LeaderBoardsWindow.transform.Find("LayoutTeamNames"));
			plyLayout.name = "LayoutPlayer";

			GameObject playerLabel = Instantiate(playerlabel,LeaderBoardsWindow.transform.position,LeaderBoardsWindow.transform.rotation) as GameObject;
			playerLabel.transform.SetParent(LeaderBoardsWindow.transform.Find("LayoutTeamNames").transform.Find("LayoutPlayer"));
			playerLabel.GetComponentInChildren<Text>().text = playerList[i];
			checkColor(playerLabel,team);
			i++;
		}
	}

	public void checkColor(GameObject obj,string team){
		if(team == "Eagles"){
			obj.GetComponentInChildren<Image>().color = eagleColor;
		}
		else if(team == "Exorcist"){
			obj.GetComponentInChildren<Image>().color = exorcistColor;
		}
		else if(team == "Wolves"){
			obj.GetComponentInChildren<Image>().color = wolfColor;
		}
		else if(team == "Angel"){
			obj.GetComponentInChildren<Image>().color = bloodColor;
		}
	}
}
