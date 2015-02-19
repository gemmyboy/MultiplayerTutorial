using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.Chat;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ChatSystem : Photon.MonoBehaviour, IChatClientListener
{
    public string ChatAppId;                    // set in inspector. Your Chat AppId (don't mix it with Realtime/Turnbased Apps).
    public string[] ChannelsToJoinOnConnect;    // set in inspector. Demo channels to join automatically.
    public int HistoryLengthToFetch;            // set in inspector. Up to a certain degree, previously sent messages can be fetched for context.
    public bool DemoPublishOnSubscribe;         // set in inspector. For demo purposes, we publish a default text to any subscribed channel.

    public string UserName { get; set; }

    private ChatChannel selectedChannel;
    public string selectedChannelName;     // mainly used for GUI/input
    public int selectedChannelIndex = 0;   // mainly used for GUI/input
    bool doingPrivateChat;


    public ChatClient chatClient;

    // GUI stuff:
    public Rect GuiRect = new Rect(0, 0, 250, 300);
    public bool IsVisible = true;

    public string inputLine = "";
    public string userIdInput = "";
    private Vector2 scrollPos = Vector2.zero;
    public static string WelcomeText = "Welcome to chat.\\help lists commands.";
    public static string HelpText = "\n\\subscribe <list of channelnames> subscribes channels.\n\\unsubscribe <list of channelnames> leaves channels.\n\\msg <username> <message> send private message to user.\n\\clear clears the current chat tab. private chats get closed.\n\\help gets this help message.";

    public EventSystem eventSystem;

    public InputField chatInput;

    public GameObject chatSystem;
    public GameObject chatSystemContent;

    public Animator chatAnimator;
    public void Start()
    {
        //Set event system
        eventSystem = FindObjectOfType<EventSystem>();

        // this must run in background or it will drop connection if not focussed.
        Application.runInBackground = true; 
        //Set username
        if (string.IsNullOrEmpty(this.UserName))
        {
            this.UserName = PhotonNetwork.player.name;
        }

        chatClient = new ChatClient(this);
        chatClient.Connect(ChatAppId, "1.0", this.UserName, null);

        Debug.Log(this.UserName);
    }

    public void Update()
    {
        if (this.chatClient != null)
        {
            this.chatClient.Service();  // make sure to call this regularly! it limits effort internally, so calling often is ok!
        }

        if (!this.IsVisible)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (eventSystem.currentSelectedGameObject == chatInput.gameObject)
            {
                // focus on input -> submit it
                GuiSendsMsg();
                ChangeAlphaOfChatInput();
                return; // showing the now modified list would result in an error. to avoid this, we just skip this single frame
            }
            else
            {
                // assign focus to input
                ChangeAlphaOfChatInput();
                chatInput.Select();
            }
        }

        inputLine = chatInput.GetComponentInChildren<Text>().text;
    }
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void GuiSendsMsg()
    {
        if (string.IsNullOrEmpty(this.inputLine))
        {

            //GUI.FocusControl("");
            return;
        }

        if (this.inputLine[0].Equals('\\'))
        {
            string[] tokens = this.inputLine.Split(new char[] { ' ' }, 2);
            if (tokens[0].Equals("\\help"))
            {
                this.PostHelpToCurrentChannel();
            }
            if (tokens[0].Equals("\\state"))
            {
                int newState = int.Parse(tokens[1]);
                this.chatClient.SetOnlineStatus(newState, new string[] { "i am state " + newState });  // this is how you set your own state and (any) message
            }
            else if (tokens[0].Equals("\\subscribe") && !string.IsNullOrEmpty(tokens[1]))
            {
                this.chatClient.Subscribe(tokens[1].Split(new char[] { ' ', ',' }));
            }
            else if (tokens[0].Equals("\\unsubscribe") && !string.IsNullOrEmpty(tokens[1]))
            {
                this.chatClient.Unsubscribe(tokens[1].Split(new char[] { ' ', ',' }));
            }
            else if (tokens[0].Equals("\\clear"))
            {
                if (this.doingPrivateChat)
                {
                    this.chatClient.PrivateChannels.Remove(this.selectedChannelName);
                }
                else
                {
                    ChatChannel channel;
                    if (this.chatClient.TryGetChannel(this.selectedChannelName, this.doingPrivateChat, out channel))
                    {
                        channel.ClearMessages();
                    }
                }
            }
            else if (tokens[0].Equals("\\msg") && !string.IsNullOrEmpty(tokens[1]))
            {
                string[] subtokens = tokens[1].Split(new char[] { ' ', ',' }, 2);
                string targetUser = subtokens[0];
                string message = subtokens[1];
                this.chatClient.SendPrivateMessage(targetUser, message);
            }
        }
        else
        {
            if (this.doingPrivateChat)
            {
                this.chatClient.SendPrivateMessage(this.userIdInput, this.inputLine);
            }
            else
            {
                Debug.Log("Herro");
                //this.chatClient.PublishMessage(this.selectedChannelName, this.inputLine);
                publishMessage(this.inputLine);
            }
        }

        this.inputLine = "";
        chatInput.GetComponent<InputField>().text = "";
        //chatInput.GetComponentInChildren<Text>().text = "";
    }
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
    public void OnApplicationQuit()
    {
        if (this.chatClient != null)
        {
            this.chatClient.Disconnect();
        }
    }

    public void PostHelpToCurrentChannel()
    {
        ChatChannel channelForHelp = this.selectedChannel;
        if (channelForHelp != null)
        {
            channelForHelp.Add("info", ChatSystem.HelpText);
        }
        else
        {
            Debug.LogError("no channel for help");
        }
    }

    public void OnConnected()
    {
        if (this.ChannelsToJoinOnConnect != null && this.ChannelsToJoinOnConnect.Length > 0)
        {
            this.chatClient.Subscribe(this.ChannelsToJoinOnConnect, this.HistoryLengthToFetch);
        }
        this.chatClient.SetOnlineStatus(ChatUserStatus.Online);             // You can set your online state (without a mesage).
    }

    public void OnDisconnected()
    {
    }

    public void OnChatStateChange(ChatState state)
    {
        // use OnConnected() and OnDisconnected()
        // this method might become more useful in the future, when more complex states are being used.
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {

        // this demo can automatically send a "hi" to subscribed channels. in a game you usually only send user's input!!
        if (this.DemoPublishOnSubscribe)
        {
            foreach (string channel in channels)
            {
                this.chatClient.PublishMessage(channel, "says 'hi' in OnSubscribed(). "); // you don't HAVE to send a msg on join but you could.
            }
        }
    }

    public void OnUnsubscribed(string[] channels)
    {
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (channelName.Equals(this.selectedChannelName))
        {
            this.scrollPos.y = float.MaxValue;
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        // as the ChatClient is buffering the messages for you, this GUI doesn't need to do anything here
        // you also get messages that you sent yourself. in that case, the channelName is determinded by the target of your msg
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        // this is how you get status updates of friends.
        // this demo simply adds status updates to the currently shown chat.
        // you could buffer them or use them any other way, too.

        ChatChannel activeChannel = this.selectedChannel;
        if (activeChannel != null)
        {
            activeChannel.Add("info", string.Format("{0} is {1}. Msg:{2}", user, status, message));
        }

        Debug.LogWarning("status: " + string.Format("{0} is {1}. Msg:{2}", user, status, message));
    }
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    GameObject message;
    public void publishMessage(string inputLine)
    {
        message = PhotonNetwork.Instantiate("message", chatSystem.transform.position, chatSystem.transform.rotation, 0);
        int viewID = message.GetComponent<PhotonView>().viewID;
        photonView.RPC("fixChatMessage",PhotonTargets.All,viewID,inputLine,PhotonNetwork.player);
    }

    [RPC]
    public void fixChatMessage(int viewID,string inputLine,PhotonPlayer player)
    {
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach(PhotonView view in views){
            if(view.viewID == viewID){
                message = view.gameObject;
            }
        }

        message.transform.SetParent(chatSystemContent.transform);
        message.GetComponentInChildren<Text>().text = player.name + ": " + inputLine;
        message.GetComponent<RectTransform>().localPosition = new Vector3(message.GetComponent<RectTransform>().localPosition.x, message.GetComponent<RectTransform>().localPosition.y + 10, message.GetComponent<RectTransform>().localPosition.z);
    }
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ChangeAlphaOfChatInput()
    {
        if (chatAnimator.GetBool("Fade"))
        {
            chatAnimator.SetBool("Fade",false);
        }
        else
        {
            chatAnimator.SetBool("Fade", true);
        }
    }
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void buttonSend()
    {
        GuiSendsMsg();
    }
}
