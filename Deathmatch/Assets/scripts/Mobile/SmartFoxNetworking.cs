using UnityEngine;
using System.Collections;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Entities;
using Sfs2X.Entities.Match;

public class SmartFoxNetworking : MonoBehaviour {
	private string ConfigFile = "Scripts/Networking/connectionConfig.xml";
	public bool UseConfigFile = false;
	public string ServerIP = "127.0.0.1";
	public int ServerPort = 9933;
	public string ZoneName = "Deathmatch";
	public string UserName = "";
	private  string[] _roomNames;
	public string RoomName = "Lobby";

	SmartFox sfs;

	// Use this for initialization
	void Start () {
		sfs = new SmartFox();
		sfs.ThreadSafeMode = true; //Tell server to hold events until asked
		sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
		sfs.AddEventListener(SFSEvent.LOGIN,OnLogin);
		sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
		sfs.AddEventListener(SFSEvent.CONFIG_LOAD_SUCCESS,OnConfigLoad);
		sfs.AddEventListener(SFSEvent.CONFIG_LOAD_FAILURE, OnConfigFail);
		sfs.AddEventListener(SFSEvent.ROOM_JOIN,OnJoinedRoom);
		sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnJoinedRoomError);
		sfs.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		if(UseConfigFile)
		{
			sfs.LoadConfig(Application.dataPath + "/" + ConfigFile);
		}else
		{
			sfs.Connect(ServerIP, ServerPort);
		}
	}
	void OnPublicMessage(BaseEvent e)
	{
		Sfs2X.Entities.Room room = (Sfs2X.Entities.Room)e.Params["room"];
		User sender = (User)e.Params["sender"];
		Debug.Log("[" + room.Name + "]" + sender.Name + ": " + e.Params["message"]);

	}
	void OnConnection(BaseEvent e)
	{
		if((bool)e.Params["success"])
		{
			Debug.Log("Succcessfully Connected!");
			if(UseConfigFile)
				ZoneName  = sfs.Config.Zone;
			sfs.Send(new LoginRequest(UserName,"",ZoneName));
		}else
		{
			Debug.Log("Connection Failed!");
		}

	}
	void OnJoinedRoom(BaseEvent e)
	{
		Debug.Log("Joined room: " + e.Params["room"]);
		sfs.Send(new PublicMessageRequest("Hello World!"));
	}

	void OnJoinedRoomError(BaseEvent e)
	{
		Debug.Log("Join Room Failed!");
	}
	void ShowRooms()
	{

	}
	void OnConfigLoad(BaseEvent e)
	{
		Debug.Log("Config file loaded!");
		sfs.Connect(sfs.Config.Host,sfs.Config.Port);
	}
	void OnConfigFail(BaseEvent e)
	{
		Debug.Log("Failed to load config file! : " + Application.dataPath + "/" + ConfigFile);
	}

	void OnLogin(BaseEvent e)
	{
		Debug.Log("Logged in: " + e.Params["user"]);

		sfs.Send(new JoinRoomRequest(RoomName));
	}
	void OnLoginError(BaseEvent e)
	{
		Debug.Log("Login error (" + e.Params["errorCode"] + ")" + e.Params["errorMessage"]);
	}
	// Update is called once per frame
	void Update () {
		sfs.ProcessEvents(); //Process events
	}

	void OnApplicationQuit()
	{
		if(sfs.IsConnected)
			sfs.Disconnect();
	}
}
