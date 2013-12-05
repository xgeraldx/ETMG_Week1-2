using UnityEngine;
using System.Collections;

public class Networking : MonoBehaviour {
	private string playerName ="Player";
	private bool joinedLobby = false;
	private bool joinedRoom = false;
	private Rect windowRect = new Rect(Screen.width/2-60, Screen.height/2-75, 150, 150);
	void Awake()
	{

	}

	void OnGUI()
	{
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
		if(joinedLobby && !joinedRoom)
		{
			windowRect = GUI.Window(0, windowRect, DoMyWindow, "Player Name");
		}

	}
	void DoMyWindow(int windowID) {
		playerName = GUI.TextField(new Rect(10,20,130,25),playerName);
		if (GUI.Button(new Rect(10, 50, 130, 20), "Submit"))
		{
			PhotonNetwork.playerName = playerName;
			PhotonNetwork.JoinRandomRoom();
		}
		
	}
	// Use this for initialization
	void Start () {
		joinedLobby = false;
		joinedRoom = false;
		if(!PhotonNetwork.connected)
		{
			PhotonNetwork.ConnectUsingSettings("1.0");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnJoinedLobby()
	{
		joinedLobby = true;
		//PhotonNetwork.JoinRandomRoom();
	}

	void OnPhotonRandomJoinFailed()
	{
		PhotonNetwork.CreateRoom(null);
	}

	void OnJoinedRoom()
	{
		joinedRoom = true;
		GameObject player = PhotonNetwork.Instantiate("PlayerControl",new Vector3(0f,2f,0f), Quaternion.identity, 0);
		GQController characterControl = player.GetComponentInChildren<GQController>();
		//characterControl.enabled = true;
		characterControl.isControllable = true;
		Player p = player.GetComponentInChildren<Player>();
		p.enabled = true;
		TextMesh tm = player.GetComponentInChildren<TextMesh>();
		tm.text = PhotonNetwork.playerName;

		RPCFunctions rpc = player.GetComponent<RPCFunctions>();
		rpc.enabled = true;
		//CharacterController cc = player.GetComponentInChildren<CharacterController>();

		//cc.enabled = true;


		Camera camera = player.GetComponentInChildren<Camera>();

		camera.enabled = true;
		ThirdCam tc = player.GetComponentInChildren<ThirdCam>();
		tc.enabled = true;
		//NetworkCharacter nc = player.GetComponentInChildren<NetworkCharacter>();
		//nc.enabled = true;
		//PhotonView pv = player.GetPhotonView();
		//pv.observed = nc;
		//Debug.Log(pv.observed.name);
		//MecanimTest mecanim = player.GetComponent<MecanimTest>();
		//mecanim.enabled = true;
		//Camera camera = player.GetComponentInChildren<Camera>();
		//camera.enabled = true;
		//CharacterControl controller = monster.GetComponent<CharacterControl>();
		//controller.enabled = true;
		//CharacterCamera camera = monster.GetComponent<CharacterCamera>();
		//camera.enabled = true;
	}
}
