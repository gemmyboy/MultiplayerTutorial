using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using System;
using ExitGames.Client.Photon;
using System.Collections.Generic;
public class Start_Menu_Server_Check : Photon.MonoBehaviour
{
    private bool isRefreshingHostList = false;
    public HostData[] hostList;

    public string gameName = "";
    public int port = 0;
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
    public LobbyNetworkManager lobbyManager;
    //Displaying labels and placeholders via PhotonNetworking
    public GameObject inputNamePlaceHolder;
    public GameObject displayPlayersOnlineLabel;
    public GameObject scrollingText;

    //GameObjects for The RefreshMenu
    public GameObject NoListLabel;
    public GameObject ListLabel;

    //bools for checking connection and list
    public bool checkedList = false;
    public bool connected = false;

    //Used to control if the game can only be started by the Server hoster
    public GameObject StartTheGameButton;
    //used for the start of the game. which mode to use
    public string gameMode = "";
    //Rebuilding the lobby when host leaves
    public List<int> currentPlayerLabels;
    //---------------------------------------------------------------------------------------
    //Used for Connections
    private bool connectFailed = false;
    public static readonly string SceneNameMenu = "start_menu";
    public static readonly string SceneNameGame = "OmegaTankScene";

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
            PhotonNetwork.ConnectUsingSettings("v0.1");
        }

        // if you wanted more debug out, turn this on:
        // PhotonNetwork.logLevel = NetworkLogLevel.Full;
        currentPlayerLabels = new List<int>();
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
        //Dont ALLow non server owners to see the play button in the matchmaking lobby
        if (ConnectingRoomWindow)
        {
            if(PhotonNetwork.isMasterClient){
                StartTheGameButton.SetActive(true);
            }
        }

    }
    //--------------------------------------------------------------------------------------------------
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
        if (gameMode == "OmegaTank")
        {
            this.gameMode = "OmegaTank";
            Start_Menu_Server_Check.SceneNameGame.Replace(SceneNameGame,"OmegaTankScene");
        }
        else
        {
            this.gameMode = "testing";
            Start_Menu_Server_Check.SceneNameGame.Replace(SceneNameGame, "testing");
        }
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
            //Hashtable customRoomProperties = new Hashtable() { { "map", 1 } };
            PhotonNetwork.CreateRoom(this.gameName, new RoomOptions() { maxPlayers = connections }, null); 
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // We have two options here: we either joined(by title, list or random) or created a room.
    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
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
        lobbyManager.previousPlayerAmount = 0;
        pm.OpenPanel(NetworkAnimator);
    }

    public void DisconnectFromRoom()
    {
        i = 0;
        currentPlayerLabels.Clear();
        if(PhotonNetwork.playerList.Length > 1){
            PhotonNetwork.SetMasterClient(chooseRandomPlayer());
            photonView.RPC("rebuildLabelLobby", PhotonTargets.Others);
        }
        else
        {
            Destroy(dropMenu);
        }
        PhotonNetwork.LeaveRoom();
    }
    //START THE GAMMMMMME
    public void StartTheGame()
    {
        photonView.RPC("closePanelForPlayers",PhotonTargets.All);
        PhotonNetwork.room.open = false;
        setUpTeams();
        StartCoroutine(ShootOffPods());
    }
    [RPC]
    void closePanelForPlayers()
    {
        pm.CloseCurrent();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    GameObject ServerButton;
    public void RefreshServerList()
    {
        if (PhotonNetwork.GetRoomList().Length == 0)
        {
            NoListLabel.SetActive(true);
            ListLabel.SetActive(false);
        }
        else
        {
            NoListLabel.SetActive(false);
            ListLabel.SetActive(true);

            int i = 0;
            foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
            {
                ServerButton = Instantiate(preFabButton, refreshWindow.transform.position, refreshWindow.transform.rotation) as GameObject;
                ServerButton.name = "ServerButton";
                if(roomInfo.open == false){
                    ServerButton.GetComponentInChildren<Text>().text = roomInfo.name + " " + roomInfo.playerCount + "/" + roomInfo.maxPlayers + "(Starting)";
                }
                else
                {
                    ServerButton.GetComponentInChildren<Text>().text = roomInfo.name + " " + roomInfo.playerCount + "/" + roomInfo.maxPlayers;
                }
            //    //Fix Button Position
                fixButton(ServerButton,i);
            //    //Adding an Event Trigger
                ServerButton.GetComponent<Button>().onClick.AddListener(() => ServerButtonClick(roomInfo));
                i++;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //For adding game buttons to server list window
    public void fixButton(GameObject button,int i)
    {
        button.transform.parent = refreshWindow.transform;
        button.transform.localScale = new Vector3(1, 1, 1);
        button.GetComponent<RectTransform>().localPosition = new Vector3(0, (-80 * i) + 30, 0);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //Creating the label and assigning it the right spot
    public void createLabelForPlayer(){
        GameObject label = PhotonNetwork.Instantiate("GameLabel", ConnectingRoomWindow.transform.position, Quaternion.identity, 0);
        int view = label.GetPhotonView().viewID;
        photonView.RPC("fixLabel", PhotonTargets.AllBuffered, new object[] { PhotonNetwork.player, view });
        if (PhotonNetwork.player.isMasterClient)
        {
            createDropDownMenu();
        }
    }
    //number to represent where the label is placed
    int i = 0;
    //Fixing the label so everyone can see
    [RPC]
    public void fixLabel(PhotonPlayer player, int view)
    {
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach (PhotonView vie in views)
        {
            if (vie.viewID == view)
            {
                currentPlayerLabels.Add(vie.viewID);
                GameObject obj = vie.gameObject;
                obj.transform.parent = ConnectingRoomWindow.transform;
                obj.transform.localScale = new Vector3(1, 1, 1);
                obj.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, (-50 * i) + 120, 0);
                obj.transform.rotation = new Quaternion(0, 0, 0, 0);
                if (player.isMasterClient)
                {
                    obj.GetComponentInChildren<Text>().text = (i + 1) + ". " + vie.owner.name + " - Master";
                }
                else
                {
                    obj.GetComponentInChildren<Text>().text = (i + 1) + ". " + vie.owner.name + " - Client";
                }
                i++;
            }
        }
    }
    //for when a player leaves and this needs to be handled
    [RPC]
    public void rebuildLabelLobby()
    {
        Debug.Log(PhotonNetwork.isMasterClient);
        int j = 0;
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach (PhotonView vie in views)
        {
            if (vie.viewID == currentPlayerLabels[j])
            {
                Debug.Log(vie.owner);
                photonView.RPC("fixLabel", PhotonTargets.AllBuffered, new object[] { vie.owner, vie.viewID });
                j++;
            }
        }
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
        dropMenu.transform.parent = ConnectingRoomWindow.transform;
        dropMenu.transform.localScale = new Vector3(1, 1, 1);
        dropMenu.transform.rotation = new Quaternion(0, 0, 0, 0);
        dropMenu.GetComponentInChildren<RectTransform>().localPosition = new Vector3(450, 100, 0);
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
        i = 0;
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }
    //Represent a buttonclick for the server join game button
    public void ServerButtonClick(RoomInfo roomInfo)
    {
        if(roomInfo.playerCount < roomInfo.maxPlayers){
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
            Debug.Log(PlayerPrefs.GetString("playerName", PhotonNetwork.playerName));
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
        yield return new WaitForSeconds(13.0f);
        PhotonNetwork.room.open = false;
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
        yield return new WaitForSeconds(10.0f);
        GameObject screenFade = PhotonNetwork.InstantiateSceneObject("FadeScreen", canvas.transform.position, canvas.transform.rotation, 0,null);
        photonView.RPC("FixScreen", PhotonTargets.All);
    }

    [RPC]
    public void FixScreen()
    {
        GameObject screenFade = GameObject.Find("FadeScreen(Clone)");
        screenFade.transform.parent = canvas.transform;
        screenFade.transform.localScale = new Vector3(1, 1, 1);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //This is only for omega tanks. We need to split the teams up. Or we could use it for FFO. it doesnt matter
    public List<string> teams;
    public void setUpTeams()
    {
        teams = new List<string>() { "Red", "Blue", "Green", "Yellow" };
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        int j = 4;
        int rndTeam;
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            rndTeam = Random.Range(0, j);
            hash.Add("Team", teams[rndTeam]);
            teams.RemoveAt(rndTeam);
            PhotonNetwork.player.SetCustomProperties(hash);
            j--;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //We are making the drop down menu for our players choice of who to play
    GameObject[] labels;
    [RPC]
    public void ChangeColor(GameObject button)
    {

        labels = GameObject.FindGameObjectsWithTag("Server_Label");
        foreach(GameObject obj in labels){
            if(obj.GetComponent<PhotonView>().isMine)
            {
                Debug.Log(PhotonNetwork.player + obj.name);
                //Get the string we have to change its value every press
                string oldText = obj.GetComponentInChildren<Text>().text;
                //get the last index
                int lastChar = oldText.LastIndexOf(oldText);
                //make new string and replace
                obj.GetComponentInChildren<Text>().text.Replace(oldText,oldText + button.GetComponentInChildren<Text>().text);
                //change color of the label
                obj.GetComponentInChildren<Image>().color = button.GetComponentInChildren<Image>().color;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //Select a random photon player to become master
    public PhotonPlayer chooseRandomPlayer()
    {
        int randCounter = 0;
        int randNum = Random.Range(0,PhotonNetwork.playerList.Length);
        foreach(PhotonPlayer player in PhotonNetwork.playerList){
            if(player != PhotonNetwork.masterClient && randNum == randCounter){
                Debug.Log(player);
                return player;
            }
            randCounter++;
        }
        return null;
    }
}