using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using System;
using ExitGames.Client.Photon;
public class Start_Menu_Server_Check : Photon.MonoBehaviour
{
    private bool isRefreshingHostList = false;
    public HostData[] hostList;

    public string gameName = "";
    public int port = 0;
    public int connections;
    public string passwordToServer = "";

    public GameObject StartButton;
    public GameObject RefreshButton;
    public GameObject preFabButton;
    public GameObject ErrorButton;
    public GameObject GameLabel;

    public GameObject refreshWindow;
    public GameObject ConnectingRoomWindow;
    public GameObject canvas;
    public GameObject connectingText;
    public GameObject FailureButton;

    //Used to open windows via scripting
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
    //---------------------------------------------------------------------------------------
    //Used for Connections
    private bool connectFailed = false;
    public static readonly string SceneNameMenu = "start_menu";
    public static readonly string SceneNameGame = "testing";

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
                timeToClearDialog = Time.time + 4.0f;
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
                connectingText.GetComponent<Text>().text = String.Format("Server: {0}", new object[] { PhotonNetwork.ServerAddress });
                connectingText.GetComponent<Text>().text = "AppId: " + PhotonNetwork.PhotonServerSettings.AppID;
                FailureButton.SetActive(true);
            }
            return;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        if (!connected)
        {
            pm.OpenPanel(MainMenu);
            connected = true;
        }
        //Lets generate a random name...gotta wait a second. takes some time for the connection to go through to register other players
        if(PhotonNetwork.connected){
            StartCoroutine(ChangeName());
        }
        //Change the PlaceHolder to the Randomly Generated Players name for the menu
        inputNamePlaceHolder.GetComponent<Text>().text = PhotonNetwork.playerName;
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

    public void ChangeRoomName(GameObject RoomNameField)
    {
        gameName = RoomNameField.GetComponent<InputField>().text;
    }

    public void ChangePort(GameObject portField)
    {
        port = int.Parse(portField.GetComponent<InputField>().text);
    }
    public void ChangeConnections(int value)
    {
        connections = value;
    }

    public void ChangePasswordToServer(GameObject passwordField)
    {
        passwordToServer = passwordField.GetComponent<InputField>().text;
    }
    //--------------------------------------------------------------------------------------------------
    //Starting the server
    public void StartServer()
    {
        if(gameName == ""){
            this.ErrorDialog = "Please Enter a Room Name";
        }else{
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
    }


    public void OnPhotonCreateRoomFailed()
    {
        this.ErrorDialog = "Error: Can't create room (room name maybe already used).";
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonJoinRoomFailed()
    {
        this.ErrorDialog = "Error: Can't join room (full or unknown room name).";
        Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }
    //Room Created
    public void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        //PhotonNetwork.LoadLevel(SceneNameGame);
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
        PhotonNetwork.LeaveRoom();
    }

    public void StartTheGame()
    {
        PhotonNetwork.LoadLevel(SceneNameGame);
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
                ServerButton.GetComponentInChildren<Text>().text = roomInfo.name + " " + roomInfo.playerCount + "/" + roomInfo.maxPlayers;
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
    public int checkLabels()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Server_Label");
        int i = 0;
        foreach(GameObject obj in objects){
            i++;
        }
        return i;
    }
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
    public void ServerButtonClick(RoomInfo roomInfo)
    {
        PhotonNetwork.JoinRoom(roomInfo.name);
        pm.CloseCurrent();
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
}