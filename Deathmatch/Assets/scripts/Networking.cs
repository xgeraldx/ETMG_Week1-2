using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Networking : MonoBehaviour {

	private string playerName ="";
	private bool joinedLobby = false;
	private bool joinedRoom = false;
	private Rect windowRect = new Rect(Screen.width/2-60, Screen.height/2-75, 150, 150);

	private Quaternion spawnRot;
    List<SpawnPoint> spawnPoints;


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
		if (GUI.Button(new Rect(10, 120, 130, 20), "Submit"))
		{
			if(playerName == "")
				playerName = "Player" + Random.Range(0,20);
			PhotonNetwork.playerName = playerName;
			PhotonNetwork.JoinRandomRoom();
		}
		
	}

	static Networking _instance;
	static public Networking Instance {
		get { 
			if(_instance==null) {
				_instance = (Networking)FindObjectOfType(typeof(Networking));
			}
			return _instance;
		}
	}
	// Use this for initialization
	void Start () {
		_instance = this;
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

	}

	void OnPhotonRandomJoinFailed()
	{
		PhotonNetwork.CreateRoom(null);
	}

	void OnJoinedRoom()
	{
		joinedRoom = true;
		SpawnPlayer();

	}
	void SpawnPlayer()
	{
		Quaternion spawnRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
		GameObject player = PhotonNetwork.Instantiate("PlayerControl",GetSpawnLocation(), spawnRot, 0);
		GQController characterControl = player.GetComponentInChildren<GQController>();

		characterControl.isControllable = true;
		Player p = player.GetComponentInChildren<Player>();

		TextMesh tm = player.GetComponentInChildren<TextMesh>();
		tm.text = PhotonNetwork.playerName;
		

		Camera camera = player.GetComponentInChildren<Camera>();
		
		camera.enabled = true;
		ThirdCam tc = player.GetComponentInChildren<ThirdCam>();
		tc.enabled = true;
	}

	public void RegisterSpawnPoint(SpawnPoint sp) {
		if(spawnPoints == null) {
			spawnPoints = new List<SpawnPoint>();
		}
		spawnPoints.Add(sp);
	}
	Vector3 GetSpawnLocation() {
		SpawnPoint sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
		while(!sp.IsClear()) {
			sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
		}
		Vector3 pos = sp.transform.position;
		return pos;
	}
	IEnumerator DelayedPlayerSpawn(float delay) {


		yield return new WaitForSeconds(delay);	// wait one frame

		SpawnPlayer();
	}
	public void DestroyAndRespawn(GameObject go) {
		// Only gets called by the owner the character (player or bot)
		//Debug.Log("DestroyAndRespawn");

		PhotonNetwork.Destroy(go);

		StopCoroutine("DelayedPlayerSpawn");
		StartCoroutine("DelayedPlayerSpawn", 3f );

	}
}
