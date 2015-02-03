using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon;
public class LeaderBoardsDisplayer : PunBehaviour {
    public Animator leaderBoards;
    public Animator taendom;

    public GameObject label;
    public GameObject LeaderBoardsWindow;
    PhotonPlayer[] players;
	// Use this for initialization
	void Start () {
        int i = 0;
        players = new PhotonPlayer[PhotonNetwork.room.maxPlayers];
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            players[i] = player;
            i++;
        }

        for (int j = 0; j < players.Length; j++)
        {
            if(players[j] != null){
                label = PhotonNetwork.Instantiate("GameLabelLeaderBoards", LeaderBoardsWindow.transform.position, Quaternion.identity, 0);
                label.name = "LeaderBoardsLabel";
                fixLabel(players[j], j);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Tab))
        {
            leaderBoards.SetBool("Faded", false);
            taendom = leaderBoards.GetComponentInChildren<Animator>();
        }
        if(Input.GetKeyUp(KeyCode.Tab)){
            leaderBoards.SetBool("Faded", true);
        }
	}

    public void fixLabel(PhotonPlayer player,int j)
    {
        label.transform.parent = LeaderBoardsWindow.gameObject.transform;
        label.transform.localScale = new Vector3(1, 1, 1);
        label.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, (-50 * j) + 140, 0);
        label.transform.rotation = new Quaternion(0, 0, 0, 0);
        label.GetComponentInChildren<Text>().text = (j + 1) + ". " + player.name + "--" + player.GetScore();
    }
}
