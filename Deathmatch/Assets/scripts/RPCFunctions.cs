using UnityEngine;
using System.Collections;

public class RPCFunctions : MonoBehaviour {
	Player p;
	GQController gq;
	private Rect windowRect = new Rect(Screen.width/2-125, Screen.height/2-125, 250, 250);
	// Use this for initialization
	void Start () {
		p = GetComponentInChildren<Player>();
		gq = GetComponentInChildren<GQController>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}
	[RPC]
	void TakeDamage()
	{
		p.health -= 1.0f;
		if(p.health <= 0)
		{
			//GQController gq = GetComponentInChildren<GQController>();
			//gq.isDead = true;
			gq.Dead();
		}
	}

	[RPC]
	void GameOver()
	{

		GameState.gameState = GameState.State.GameOver;
	}

	[RPC]
	void RemotePlayerShoot(Vector3 pos, Quaternion rot)
	{
		Debug.Log("Shoot Called");
		//Tranfrom pos = new Transform();

		if(!gq.animation.IsPlaying(gq.shootAnimation.name) && gq.animation.isPlaying)
		{
			gq.animation.Stop();
			gq.animation[gq.shootAnimation.name].speed = .25f;
			gq.animation.Play(gq.shootAnimation.name);
		}

		if(gq.coolDown <=0.0f && PhotonNetwork.player.isLocal)
			Instantiate(gq.DefaultProjectile,pos,rot);
	}
	void OnGUI()
	{
		if(GameState.gameState == GameState.State.GameOver)
		{
			windowRect = GUI.Window(0, windowRect, GameOverWindow, "Game Over");
		}
	}
	void GameOverWindow(int windowID)
	{
		GUI.Label(new Rect(10f,10f,230f,25f),p.health > 0.0f?"You Win":"You Lose");
		if(GUI.Button(new Rect(87.5f,200f,75f,25f),"Quit"))
			{
				
				GameState.gameState = GameState.State.Running;
				if(PhotonNetwork.isMasterClient)
				{
					PhotonNetwork.Destroy(p.transform.parent.gameObject);
					
				}
				PhotonNetwork.Disconnect();
				Application.LoadLevel("MainMenu");
			}
	}

}
