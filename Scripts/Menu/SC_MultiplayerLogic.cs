using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using G = SC_Globals;


/*********************************
* Project: 	Unipoly      		 *
* Multyplayer Logic Script       *
*********************************/
public class SC_MultiplayerLogic : MonoBehaviour
{
    //Keys from My app: apphq.shephertz.com
    private string APIKey = "0ed719f3c73ac85cfd67ba61b9f3c870ed907f33bc0826977c538c39ad174319";
    private string APISecretKey = "0a73ff44d75badb58fdb0e73eecf860e06b7ea45ff95a6be63c7999c8c6ee198";
    private Listener listener;
    private Dictionary<string, GameObject> unityObjects;

    private Dictionary<string, object> passedParams;

    private List<string> roomIDs;
    private int roomIndex;

    #region Singleton
    static SC_MultiplayerLogic instance;
    public static SC_MultiplayerLogic Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_MultiplayerLogic").GetComponent<SC_MultiplayerLogic>();
            return instance;
        }
    }
    #endregion

    #region MonoBehaviour
    private void OnEnable()
    {
        Listener.OnConnect += OnConnect;
        Listener.OnDisconnect += OnDisconnect;
        Listener.OnRoomsInRange += OnRoomsInRange;
        Listener.OnCreateRoom += OnCreateRoom;
        Listener.OnJoinRoom += OnJoinRoom;
        Listener.OnUserJoinRoom += OnUserJoinRoom;
        Listener.OnGetLiveRoomInfo += OnGetLiveRoomInfo;
        Listener.OnGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        Listener.OnConnect -= OnConnect;
        Listener.OnDisconnect -= OnDisconnect;
        Listener.OnRoomsInRange -= OnRoomsInRange;
        Listener.OnCreateRoom -= OnCreateRoom;
        Listener.OnJoinRoom -= OnJoinRoom;
        Listener.OnUserJoinRoom -= OnUserJoinRoom;
        Listener.OnGetLiveRoomInfo -= OnGetLiveRoomInfo;
        Listener.OnGameStarted -= OnGameStarted;
    }
    void Awake()
    {
        Init();
    }
    #endregion

    #region Logic
    public void Init()
    {

        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _unityObjects = GameObject.FindGameObjectsWithTag("UnityObjects");
        foreach (GameObject g in _unityObjects)
            unityObjects.Add(g.name, g);

        if (listener == null)
            listener = new Listener();

        //Warp Client Connetion
        WarpClient.initialize(APIKey, APISecretKey);
        WarpClient.GetInstance().AddConnectionRequestListener(listener);
        WarpClient.GetInstance().AddChatRequestListener(listener);
        WarpClient.GetInstance().AddUpdateRequestListener(listener);
        WarpClient.GetInstance().AddLobbyRequestListener(listener);
        WarpClient.GetInstance().AddNotificationListener(listener);
        WarpClient.GetInstance().AddRoomRequestListener(listener);
        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(listener);
        WarpClient.GetInstance().AddZoneRequestListener(listener);

        UpdateStatus("Waiting for user submission");
    }

    private void UpdateStatus(string _Message)
    {
        unityObjects["TextStatus"].GetComponent<Text>().text = _Message;
    }

    //For Disconnect - Because there are situations where the connection to the server hangs
    public void WarpClient_Disconnect()
    {
        WarpClient.GetInstance().Disconnect();
    }

    private void DoRoomsSearchLogic()
    {

        //Check if there are rooms to lookup
        if (roomIDs != null && roomIndex < roomIDs.Count)
        {
            UpdateStatus("Bring room Info (" + roomIDs[roomIndex] + ")");
            WarpClient.GetInstance().GetLiveRoomInfo(roomIDs[roomIndex]);
        }
        else //No rooms create a new room
        {
            UpdateStatus("Creating Room...");
            WarpClient.GetInstance().CreateTurnRoom("GameRoom", G.userID, G.MAX_USRES, passedParams, G.MAX_TURN_TIME);
        }
    }
    #endregion

    #region Controller

    public void ButtonPlayMP()
    {
        if (unityObjects["emailInputField"].GetComponent<InputField>().text != "" &&
            unityObjects["InputFieldNameMP"].GetComponent<InputField>().text != "")
        {
            //Disable coomponents
            unityObjects["emailInputField"].GetComponent<InputField>().interactable = false;
            unityObjects["InputFieldNameMP"].GetComponent<InputField>().interactable = false;
            unityObjects["Slider_MpBet"].GetComponent<Slider>().interactable = false;


            G.userID = unityObjects["emailInputField"].GetComponent<InputField>().text;
            unityObjects["TextUserID"].GetComponent<Text>().text = "User ID: " + G.userID;

            int _value = Mathf.RoundToInt(unityObjects["Slider_MpBet"].GetComponent<Slider>().value);
            string _myName = unityObjects["InputFieldNameMP"].GetComponent<InputField>().text;


            passedParams = new Dictionary<string, object>();
            passedParams.Add("Password", "Unipoly");
            //Match Key & name
            passedParams.Add("MatchKey", _value);
            passedParams.Add("Name", _myName);

            WarpClient.GetInstance().Connect(G.userID);

            UpdateStatus("Open Connection...");

            Debug.Log("Password: " + passedParams["Password"].ToString());
            Debug.Log("MatchKey: " + passedParams["MatchKey"].ToString());
            Debug.Log("Name: " + passedParams["Name"].ToString());

            G.MyName = passedParams["Name"].ToString();

            //Disable the button because we send a single request to the server
            unityObjects["ButtonPlayMP"].GetComponent<Button>().interactable = false;
        }
        else
        {
            unityObjects["emailInputField"].GetComponent<InputField>().text = "user@email.game";
        }
    }
    #endregion
    #region Callbacks
    private void OnConnect(bool _IsSuccess)
    {
        Debug.Log("OnConnect? " + _IsSuccess);
        if (_IsSuccess)
        {
            UpdateStatus("Connected.");
            WarpClient.GetInstance().GetRoomsInRange(1, G.MAX_USRES);
            UpdateStatus("Looking for an available room...");
        }
        else
        {
            UpdateStatus("Connection Faild");
        }
    }

    private void OnRoomsInRange(bool _IsSuccess, MatchedRoomsEvent eventObj)
    {
        Debug.Log("On Rooms In Range? " + _IsSuccess);
        if (_IsSuccess)
        {
            UpdateStatus("Parsing Rooms.");
            roomIDs = new List<string>();
            foreach (var RoomData in eventObj.getRoomsData())
            {
                Debug.Log("Room ID: " + RoomData.getId());
                Debug.Log("Room Owner: " + RoomData.getRoomOwner());
                roomIDs.Add(RoomData.getId());
            }
        }
        roomIndex = 0;
        DoRoomsSearchLogic();
    }

    //Call if i'm the Owner
    private void OnCreateRoom(bool _IsSuccess, string _RoomId)
    {
        Debug.Log("OnCreateRoom: " + _IsSuccess + ", RoomId: " + _RoomId);
        if (_IsSuccess)
        {
            G.roomID = _RoomId;
            unityObjects["TextRoomID"].GetComponent<Text>().text = "RoomId: " + G.roomID;
            UpdateStatus("Room have been created, RoomId: " + G.roomID);
            WarpClient.GetInstance().JoinRoom(G.roomID);
            WarpClient.GetInstance().SubscribeRoom(G.roomID);
        }
    }

    private void OnJoinRoom(bool _IsSuccess, string _RoomId)
    {
        Debug.Log("OnJoinRoom: " + _IsSuccess + ", RoomId: " + _RoomId);
        if (_IsSuccess)
            UpdateStatus("Joined to Room " + _RoomId + "\nWaiting for an opponent...");
        else UpdateStatus("Failed to join room: " + _RoomId);
    }


    private void OnDisconnect(bool _IsSuccess)
    {
        Debug.Log("Disconnect");
    }

    private bool CheckRoomPasswords(LiveRoomInfoEvent eventObj)
    {
        //Properties = passedParams Dictionary
        if (eventObj != null && eventObj.getProperties() != null &&
            eventObj.getProperties().ContainsKey("Password") &&
            eventObj.getProperties()["Password"].ToString() == passedParams["Password"].ToString() &&
            eventObj.getProperties().ContainsKey("MatchKey") &&
            eventObj.getProperties()["MatchKey"].ToString() == passedParams["MatchKey"].ToString())

            return true;
        else
            return false;
    }

    private void OnGetLiveRoomInfo(LiveRoomInfoEvent eventObj)
    {
        //getProperties = passedParams Dictionary

        //Call if the room exist, and i'm the guest (not owner)
        if (CheckRoomPasswords(eventObj))
        {
            Debug.Log("Matched Room!");
            G.roomID = eventObj.getData().getId();
            UpdateStatus("Recived Room info, joining room... (" + G.roomID + ")");
            unityObjects["TextRoomID"].GetComponent<Text>().text = "RoomId: " + G.roomID;
            WarpClient.GetInstance().JoinRoom(G.roomID);
            WarpClient.GetInstance().SubscribeRoom(G.roomID);

            
            //G.GusetName = eventObj.getProperties()["Name"].ToString();
        }
        else
        {
            roomIndex++;
            DoRoomsSearchLogic();
        }
    }

    //Two users entered to room
    //Call if i create the room
    private void OnUserJoinRoom(RoomData eventObj, string _UserName)
    {
        //Details of the guest who joined the room
        Debug.Log("User joined room: " + _UserName);
        UpdateStatus("User joined room: " + _UserName);

        //The owner of the room, starts the game
        //Only one user (in our case the owner) can start the game
        if (eventObj.getRoomOwner() == G.userID && G.userID != _UserName)
        {
            UpdateStatus("Starting game...");
            WarpClient.GetInstance().startGame();
        }
    }

    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        SC_MenuLogic.Instance.Start_Game_MoveToBoard();
        UpdateStatus("The game have started. " + _NextTurn + " is starting the game.");
    }
    #endregion
}
