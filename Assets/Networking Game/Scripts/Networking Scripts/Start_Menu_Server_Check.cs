using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using System;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using Photon;
public class Start_Menu_Server_Check : Photon.MonoBehaviour
{

    private bool isRefreshingHostList = false;
    public HostData[] hostList;

    public string gameName = "";
    public int connections;

    public GameObject StartButton;
    public GameObject RefreshButton;
    public GameObject preFabButton;
    public GameObject ErrorButton;

    public GameObject refreshWindow;
    public GameObject ConnectingRoomWindow;
    public GameObject canvas;
    public GameObject connectingText;
    public GameObject FailureButton;

    //Used to open windows via scripting
    public Animator LoginAnimator;
    public Animator MainMenu;
    public Animator ConnectingToRoomAnimator;
    public Animator NetworkAnimator;
    public Animator RefreshListAnimator;
    public PanelManager pm;
    //Displaying labels and placeholders via PhotonNetworking
    public GameObject inputNamePlaceHolder;
    public GameObject displayPlayersOnlineLabel;
    public GameObject scrollingText;

    //GameObjects for The RefreshMenu
    public GameObject NoListLabel;
    public GameObject ListLabel;
    public GameObject layout;

    //bools for checking connection and list
    public bool checkedList = false;
    public bool connected = false;

    //Used to control if the game can only be started by the Server hoster
    public GameObject StartTheGameButton;
    //used for the start of the game. which mode to use
    public string gameMode = "";
    //Photon View ID for the label you own
    public int myLabelViewID;

    //button text boxes for server listing
    public GameObject GameName;
    public GameObject GameType;
    public GameObject GamePlayers;
    public GameObject GamePing;
    //---------------------------------------------------------------------------------------
    //Used for Connections
    private bool connectFailed = false;
    public static readonly string SceneNameMenu = "start_menu";
    public static string SceneNameGame = "Demo Scene";

    private string errorDialog;
    private double timeToClearDialog;
    public string ErrorDialog
    {
        get
        {
            return errorDialog;
        }
        private set
        {
            errorDialog = value;
            if (!String.IsNullOrEmpty(value))
            {
                timeToClearDialog = Time.time + 1.5f;
            }
        }
    }

    public void Awake()
    {
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;

        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
        {
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            //PhotonNetwork.ConnectToBestCloudServer("v0.1");
            PhotonNetwork.ConnectUsingSettings("v0.1");
        }

        // if you wanted more debug out, turn this on:
        // PhotonNetwork.logLevel = NetworkLogLevel.Full;
    }
    void Update()
    {

        //Displaying the connection text for connecting to the photon server
        if (!PhotonNetwork.connected)
        {
            if (PhotonNetwork.connecting)
            {
                connectingText.GetComponent<Text>().text = "Connecting to: " + PhotonNetwork.ServerAddress;
            }
            else
            {
                connectingText.GetComponent<Text>().text = "Not connected. Check console output. Detailed connection state: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.ServerAddress;
            }

            if (this.connectFailed)
            {
                connectingText.GetComponent<Text>().text = "Connection failed. Check setup and use Setup Wizard to fix configuration.";
                connectingText.GetComponent<Text>().text += String.Format("Server: {0}", new object[] { PhotonNetwork.ServerAddress });
                connectingText.GetComponent<Text>().text += "AppId: " + PhotonNetwork.PhotonServerSettings.AppID;
                FailureButton.SetActive(true);
            }
            return;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //Lets generate a random name...gotta wait a second. takes some time for the connection to go through to register other players
        if (PhotonNetwork.connected && !connected)
        {
            pm.OpenPanel(LoginAnimator);
            connected = true;
            // generate a name for this player, if none is assigned yet
            if (String.IsNullOrEmpty(PhotonNetwork.playerName))
            {
                if (String.IsNullOrEmpty(PlayerPrefs.GetString("playerName"))){
                    PhotonNetwork.playerName = "Guest" + PhotonNetwork.countOfPlayers;
                }else{
                    PhotonNetwork.playerName = PlayerPrefs.GetString("playerName", PhotonNetwork.playerName);
                }
                //Change the PlaceHolder to the Generated Players name for the menu
            }
            inputNamePlaceHolder.GetComponent<InputField>().text = PhotonNetwork.playerName;
        }
        //Change the Text to the Number of rooms there are for the menu
        if (ListLabel.GetComponentInChildren<Text>() != null)
        {
            ListLabel.GetComponentInChildren<Text>().text = PhotonNetwork.GetRoomList().Length + " rooms available:";
        }
        //Display the Label for Number of players online
        if (displayPlayersOnlineLabel.active)
        {
            displayPlayersOnlineLabel.GetComponentInChildren<Text>().text = PhotonNetwork.countOfPlayers + " users are online in " + PhotonNetwork.countOfRooms + " rooms.";
        }
        //Display Error Message for a certain Time
        if (!string.IsNullOrEmpty(this.ErrorDialog))
        {
            ErrorButton.GetComponent<Animator>().SetBool("Faded",false);
            ErrorButton.GetComponentInChildren<Text>().text = this.errorDialog;
            if (timeToClearDialog < Time.time)
            {
                timeToClearDialog = 0;
                this.ErrorDialog = "";
            }
        }
        else
        {
            ErrorButton.GetComponent<Animator>().SetBool("Faded", true);
        }
    }
    //--------------------------------------------------------------------------------------------------
    GameObject obj;
    //The text fields that are in creating a server
    public void ChangePlayerName(InputField inputName)
    {

        PhotonNetwork.playerName = inputName.text;
        PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
    }
    public void CheckIfEmpty(InputField inputName)
    {
        if (inputName.text == "")
        {
            this.ErrorDialog = "Invalid name. Please Enter Valid Input";
        }else{
            pm.OpenPanel(MainMenu);
        }
    }

    public void ChangeRoomName(GameObject RoomNameField)
    {
        gameName = RoomNameField.GetComponent<InputField>().text;
    }

    public void ChangeConnections(int value)
    {
        connections = value;
    }

    public void ChangeGameMode(string gameMode)
    {
        this.gameMode = gameMode;

    }
    //--------------------------------------------------------------------------------------------------
    //Starting the server
    public void StartServer()
    {
        if(gameName == ""){
            this.ErrorDialog = "Please Enter a Room Name";
        }
        else if(gameMode == ""){
            this.ErrorDialog = "Please Select a Game Mode";
        }
        else{
            //string[] roomPropsInLobby = { "map", "ai" };
            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();
            customRoomProperties.Add("GameType",gameMode);
            //Game name   isVisible  isOpen   Maxplayers   customproperties
            PhotonNetwork.CreateRoom(this.gameName,true,true,connections,customRoomProperties,null);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // We have two options here: we either joined(by title, list or random) or created a room.
    public void OnJoinedRoom()
    {
        pm.closeWindow(RefreshListAnimator);
        pm.OpenPanel(ConnectingToRoomAnimator);
        createLabelForPlayer();
    }

    public void OnPhotonCreateRoomFailed()
    {
        this.ErrorDialog = "Error: Can't create room (room name maybe already used).";
    }

    public void OnPhotonJoinRoomFailed()
    {
        this.ErrorDialog = "Error: Can't join room (full or game is Starting. Please Wait).";
    }
    //Room Created
    public void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    public void OnFailedToConnectToPhoton(object parameters)
    {
        this.connectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.networkingPeer.ServerAddress);
    }

    public void OnLeftRoom()
    {
        Debug.Log("Disconnected From Room");
        pm.OpenPanel(NetworkAnimator);
    }

    public void DisconnectFromRoom()
    {
        if(PhotonNetwork.playerList.Length > 1){
			numberInLabel = 0;
            if(PhotonNetwork.player.isMasterClient){
                StartTheGameButton.SetActive(false);
                PhotonNetwork.DestroyAll();
                photonView.RPC("abandonShip", PhotonTargets.Others);
            }
            else
            {
                photonView.RPC("rebuildLabelLobby", PhotonTargets.Others);
            }
        }
        else
        {
            PhotonNetwork.Destroy(dropMenu);
        }
        PhotonNetwork.LeaveRoom();
    }
    //START THE GAMMMMMME
    public void StartTheGame()
    {
        if(CheckFactions()){
            photonView.RPC("closePanelForPlayers",PhotonTargets.All);
            PhotonNetwork.room.open = false;
            fadeOut();
            Start_Screen_Sound sound = FindObjectOfType<Start_Screen_Sound>(); 
            sound.StartGameCountdown();
            StartCoroutine(ShootOffPods()); 
        }
    }
    [RPC]
    void closePanelForPlayers()
    {
        pm.CloseCurrent();
    }
    [RPC]
    void abandonShip()
    {
        Debug.Log("Room");
        PhotonNetwork.LeaveRoom();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //GameObject ServerButton;
    public void RefreshServerList()
    {
        if (PhotonNetwork.GetRoomList().Length == 0)
        {
            NoListLabel.SetActive(true);
            ListLabel.SetActive(false);
            layout.SetActive(false);
        }
        else
        {
            NoListLabel.SetActive(false);
            ListLabel.SetActive(true);
            layout.SetActive(true);

            ClearButtons();
            int i = 0;
            GameObject.Find("Server_Button_Layout").GetComponent<VerticalLayoutGroup>().padding.bottom = 0;
            foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
            {
                GameObject ServerButton = Instantiate(preFabButton, refreshWindow.transform.position, refreshWindow.transform.rotation) as GameObject;
                ServerButton.name = "ServerButton";

                ServerButton.transform.SetParent(GameObject.Find("Server_Button_Layout").transform);
                ServerButton.transform.localScale = new Vector3(1, 1, 1);

                //Display info of game and room
                GameObject background = ServerButton.transform.Find("Background").gameObject;

                GameName = background.transform.Find("GameName").gameObject;
                GameType = background.transform.Find("GameType").gameObject;
                GamePing = background.transform.Find("GamePing").gameObject;
                GamePlayers = background.transform.Find("GamePlayers").gameObject;

                GameName.GetComponent<Text>().text = roomInfo.name;
                GameType.GetComponent<Text>().text = roomInfo.customProperties["GameType"].ToString();
                GamePing.GetComponent<Text>().text = PhotonNetwork.GetPing().ToString();
                GamePlayers.GetComponent<Text>().text = roomInfo.playerCount + "/" + roomInfo.maxPlayers;

                if(roomInfo.open == false){
                    ServerButton.GetComponent<Animator>().enabled = false;
                    ServerButton.GetComponentInChildren<Image>().color = Color.grey;
                }

                if(i > 0){
                    Debug.Log("heyo");
                    GameObject.Find("Server_Button_Layout").GetComponent<VerticalLayoutGroup>().padding.bottom -= 60;
                }
                i++;
                ServerButton.GetComponent<Button>().onClick.AddListener(() => ServerButtonClick(roomInfo));
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //Creating the label and assigning it the right spot
    public void createLabelForPlayer(){
        GameObject label = PhotonNetwork.Instantiate("GameLabel", ConnectingRoomWindow.transform.position, Quaternion.identity, 0);
        myLabelViewID = label.GetPhotonView().viewID;

        obj = ConnectingRoomWindow.transform.FindChild("SF Title").gameObject;
        obj.GetComponentInChildren<Text>().text = (string)PhotonNetwork.room.customProperties["GameType"];

        photonView.RPC("fixLabel", PhotonTargets.AllBuffered, new object[] { PhotonNetwork.player, myLabelViewID });
        if (PhotonNetwork.player.isMasterClient)
        {
            StartTheGameButton.SetActive(true);
            if ((string)PhotonNetwork.room.customProperties["GameType"] != "Omega Tank")
            {
                createDropDownMenu();
            }
        }
    }
    //number to represent where the label is placed
    int numberInLabel = 0;
    //Fixing the label so everyone can see
    [RPC]
    public void fixLabel(PhotonPlayer player,int view)
    {
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach (PhotonView vie in views)
        {
            if (vie.viewID == view)
            {
                GameObject obj = vie.gameObject;
                obj.transform.SetParent(ConnectingRoomWindow.transform.Find("GameLabelHolder"));
                obj.transform.localScale = new Vector3(1, 1, 1);
                obj.transform.rotation = new Quaternion(0, 0, 0, 0);
                obj.transform.GetComponent<RectTransform>().localPosition = new Vector3(0,0,0); ;
                if (player.isMasterClient)
                {
					obj.GetComponentInChildren<Text>().text = (numberInLabel + 1) + ". " + vie.owner.name + " - Master";
					numberInLabel++;
                }
                else
                {
					obj.GetComponentInChildren<Text>().text = (numberInLabel+1) + ". " + vie.owner.name + " - Client";
					numberInLabel++;
                }
            }
        }
    }
    //for when a player leaves and this needs to be handled
    [RPC]
    public void rebuildLabelLobby()
    {
        DeleteLabels();
        DeleteEmblems();
		numberInLabel = 0;
        createLabelForPlayer();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //Making the drop down menu
    GameObject dropMenu;
    public void createDropDownMenu()
    {
        dropMenu = PhotonNetwork.InstantiateSceneObject("DropDownButtonMenu", ConnectingRoomWindow.transform.position, Quaternion.identity, 0, null);
        photonView.RPC("fixDropDown", PhotonTargets.AllBuffered);
    }
    //Fixing the drop down menu for everyone
    [RPC]
    void fixDropDown()
    {
        dropMenu = GameObject.Find("DropDownButtonMenu(Clone)");
        dropMenu.transform.SetParent(ConnectingRoomWindow.transform);
        dropMenu.transform.localScale = new Vector3(1, 1, 1);
        dropMenu.transform.rotation = new Quaternion(0, 0, 0, 0);
        dropMenu.GetComponent<RectTransform>().localPosition = new Vector3(450, 100, 0);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //checking to see if the tag is this------------------------------
    public int checkLabels()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Server_Label");
        int i = 0;
        foreach(GameObject obj in objects){
            i++;
        }
        return i;
    }
    //Clear the join game buttons
    public void ClearButtons()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Server_Button");
        if (objects != null)
        {
            foreach (var i in objects)
            {
                Destroy(i);
            }
        }
    }
    //Deleting player labels
    public void DeleteLabels()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Server_Label");
        if (objects == null)
        {
            return;
        }
        foreach (GameObject obj in objects)
        {
            if(obj.GetComponent<PhotonView>().isMine){
                PhotonNetwork.Destroy(obj);
            }
        }
    }
    //deleting emblems
    public void DeleteEmblems()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Server_Emblem");
        if (objects == null)
        {
            return;
        }
        foreach (GameObject obj in objects)
        {
            if(obj.GetComponent<PhotonView>().isMine){
                PhotonNetwork.Destroy(obj);
            }
        }
    }
    //Represent a buttonclick for the server join game button
    public void ServerButtonClick(RoomInfo roomInfo)
    {
        if(roomInfo.playerCount < roomInfo.maxPlayers && roomInfo.open){
            PhotonNetwork.JoinRoom(roomInfo.name);
            pm.CloseCurrent();
        }
        else
        {
            this.ErrorDialog = "Room is full. Please wait for a player to leave.";
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //For Failure to join PhotonNetwork. 
    public void FailurePushButton()
    {
        this.connectFailed = false;
        PhotonNetwork.ConnectUsingSettings("v1.0");
        FailureButton.SetActive(false);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //Changing the players name
    IEnumerator  ChangeName()
    {
        yield return new WaitForSeconds(1.0f);
        // generate a name for this player, if none is assigned yet
        if (String.IsNullOrEmpty(PhotonNetwork.playerName))
        {
            PhotonNetwork.playerName = PlayerPrefs.GetString("playerName", PhotonNetwork.playerName);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //Create a dummy pod after game has started
    GameObject dummyPod;
    IEnumerator ShootOffPods()
    {
        int i = 0;
        Quaternion pieceRotation = Quaternion.AngleAxis(270, Vector3.left);
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            Vector3 position = new Vector3(Camera.main.transform.position.x - 5 + (i * 3), Camera.main.transform.position.y, Camera.main.transform.position.z + 20);
            dummyPod = PhotonNetwork.Instantiate("DummyPod", position, pieceRotation, 0);
            i++;
        }
        //yield return new WaitForSeconds(13.0f);
        yield return new WaitForSeconds(2.0f);
        //PhotonNetwork.DestroyAll();
        PhotonNetwork.LoadLevel(SceneNameGame);

    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //creating a fade out for when the game starts
    public void fadeOut()
    {
        StartCoroutine(fadeScreen());
    }

    IEnumerator fadeScreen()
    {
        yield return new WaitForSeconds(1.0f);
        GameObject screenFade = PhotonNetwork.InstantiateSceneObject("FadeScreen", canvas.transform.position, canvas.transform.rotation, 0,null);
        photonView.RPC("FixScreen", PhotonTargets.All);
    }

    [RPC]
    public void FixScreen()
    {
        GameObject screenFade = GameObject.Find("FadeScreen(Clone)");
        //screenFade.transform.parent = canvas.transform;
        screenFade.transform.SetParent(canvas.transform);
        screenFade.transform.localScale = new Vector3(1, 1, 1);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //This is only for omega tanks. We need to split the teams up. Or we could use it for FFO. it doesnt matter
    public List<string> teams;
    public void setUpTeams_FOF(PhotonView buttonView)
    {
        //setup list and hash
        teams = new List<string>() { "Eagles", "Exorcist", "Wolves", "Angel" };
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        if (button.GetComponentInChildren<Text>().text == "Dark Eagles")
        {
            hash.Add("Team", teams[0]);
        }
        else if (button.GetComponentInChildren<Text>().text == "The Exorcist")
        {
            hash.Add("Team", teams[1]);
        }
        else if (button.GetComponentInChildren<Text>().text == "Emperor's Wolves")
        {
            hash.Add("Team", teams[2]);
        }
        else if (button.GetComponentInChildren<Text>().text == "Blood Angels")
        {
            hash.Add("Team", teams[3]);
        }
        PhotonNetwork.player.SetCustomProperties(hash);
        createTeamEmlbem();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //We are making the drop down menu for our players choice of who to play
    PhotonView button;
    [RPC]
    public void ChangeColor(int myButtonViewID, int LabelView)
    {
        Color buttonColor = Color.blue;

        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach (PhotonView vie in views)
        {
            if (vie.viewID == myButtonViewID)
            {
                buttonColor = vie.GetComponentInChildren<Image>().color;
                button = vie.GetComponent<PhotonView>();
                if((string)PhotonNetwork.room.customProperties["GameType"].ToString() == "Free For All"){
                    button.GetComponent<Button>().interactable = false;
                    button.GetComponentInChildren<Image>().color = Color.grey;
                }
            }
        }

        foreach (PhotonView vie in views)
        {
            if (vie.viewID == LabelView)
            {
                vie.GetComponentInChildren<Image>().color = buttonColor;
                if(vie.isMine){
                    setUpTeams_FOF(button);
                }
            }
        }
    }

    public void AlertColorChange(GameObject buttonView,int LabelView)
    {
        photonView.RPC("ChangeColor", PhotonTargets.AllBuffered, buttonView.GetComponent<PhotonView>().viewID, LabelView);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //Select a random photon player to become master
    public PhotonPlayer chooseRandomPlayer()
    {
        foreach(PhotonPlayer player in PhotonNetwork.playerList){
            if(player != PhotonNetwork.masterClient){
                return player;
            }
        }
        return null;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    GameObject emblem;
    public void createTeamEmlbem()
    {
        if(PhotonNetwork.player.customProperties["Team"].ToString () == teams[0]){
            emblem = PhotonNetwork.Instantiate("Dark_Eagles_Emblem", ConnectingRoomWindow.transform.position, Quaternion.identity, 0);
        }
		else if (PhotonNetwork.player.customProperties["Team"].ToString () == teams[1])
        {
            emblem = PhotonNetwork.Instantiate("Exorcist_Emblem", ConnectingRoomWindow.transform.position, Quaternion.identity, 0);
        }
		else if (PhotonNetwork.player.customProperties["Team"].ToString () == teams[2])
        {
            emblem = PhotonNetwork.Instantiate("Wolves_Emblem", ConnectingRoomWindow.transform.position, Quaternion.identity, 0);
        }
		else if (PhotonNetwork.player.customProperties["Team"].ToString () == teams[3])
        {
            emblem = PhotonNetwork.Instantiate("Blood_Angel_Emblem", ConnectingRoomWindow.transform.position, Quaternion.identity, 0);
        }
        photonView.RPC("fixEmblem", PhotonTargets.AllBuffered, emblem.GetComponent<PhotonView>().viewID,myLabelViewID);
    }
    GameObject lab;
    [RPC]
    public void fixEmblem(int gameObjectID,int viewID)
    {
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach(PhotonView vie in views){
            if(vie.viewID == viewID){
                lab = vie.gameObject;
            }
            if(vie.viewID == gameObjectID){
                emblem = vie.gameObject;
            }
        }
        emblem.transform.SetParent(lab.transform);
        emblem.transform.rotation = new Quaternion(0, 0, 0, 0);
        emblem.transform.localScale = new Vector3(1, 1, 1);
        emblem.GetComponentInChildren<RectTransform>().localPosition = new Vector3(150,0,0);
        lab.GetComponentInParent<VerticalLayoutGroup>().enabled = false;
        lab.GetComponentInParent<VerticalLayoutGroup>().enabled = true;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    bool CheckFactions()
    {
        foreach(PhotonPlayer player in PhotonNetwork.playerList){
            if(player.customProperties["Team"] == null){
                if(PhotonNetwork.room.customProperties["GameType"].ToString() != "Omega Tank"){
                    photonView.RPC("noFaction",player);
                    return false;
                }
            }
        }
        return true;
    }
    [RPC]
    void noFaction()
    {
        this.ErrorDialog = "Choose a Faction";
    }
}