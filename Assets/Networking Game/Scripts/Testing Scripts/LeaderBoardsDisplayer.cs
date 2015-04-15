using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon;
public class LeaderBoardsDisplayer : PunBehaviour {
    public Animator leaderBoards;

    public GameObject LeaderBoardsWindow;
    PhotonPlayer[] players;
	// Use this for initialization
	void Start () {
        LeaderBoardsWindow.transform.Find("Title").transform.Find("TitleLabel").GetComponent<Text>().text = PhotonNetwork.room.customProperties["GameType"].ToString();
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
}
