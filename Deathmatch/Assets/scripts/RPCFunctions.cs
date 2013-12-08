using UnityEngine;
using System.Collections;

public class RPCFunctions : Photon.MonoBehaviour {

	private Rect windowRect = new Rect(Screen.width/2-125, Screen.height/2-125, 250, 250);


	[RPC]
	void GameOver()
	{

		GameState.gameState = GameState.State.GameOver;
	}

	[RPC]
	void RemotePlayerShoot(Vector3 pos, Quaternion rot)
	{

		GQController gq = photonView.gameObject.GetComponentInChildren<GQController>();

		if(!gq.animation.IsPlaying(gq.shootAnimation.name) && gq.animation.isPlaying)
		{
			gq.animation.Stop();
			gq.animation[gq.shootAnimation.name].speed = 1f;
			gq.animation.Play(gq.shootAnimation.name);
		}

		if(gq.coolDown <=0.0f && PhotonNetwork.player.isLocal)
			Instantiate(gq.DefaultProjectile,pos,rot);
	}
	void OnGUI()
	{
		/*if(gq.gameObject.transform.parent.gameObject.GetPhotonView().isMine)
		{
			if(gq.isDead && GameState.gameState != GameState.State.GameOver)
			{
				GUI.Box(new Rect(Screen.width/2 -75,Screen.height/2-75,150f,75f),"Respawn");
				if(GUI.Button(new Rect(Screen.width/2 -65,Screen.height/2 -65,50f,25f),"Respawn"))
				{
					GameState.NextRound();
					PhotonView pv = gq.gameObject.transform.parent.gameObject.GetPhotonView();
					Networking.Instance.DestroyAndRespawn(pv.gameObject);
				}
			}
		}*/
		if(GameState.gameState == GameState.State.GameOver)
		{
			windowRect = GUI.Window(0, windowRect, GameOverWindow, "Game Over");
		}
	}
	[RPC]
	void PlayShootEffect()
	{
		AudioSource audio = GetComponentInChildren<AudioSource>();
		audio.Play();
	}
	[RPC]
	void TakeDamage()
	{
		if(!photonView.isMine)
			return;

		GQController gq = photonView.gameObject.GetComponentInChildren<GQController>();
		Player p = photonView.gameObject.GetComponentInChildren<Player>();
		if(!gq.isDead)
		{
			p.health -= 1f;
			if(p.health <= 0f)
			{
				
				gq.Dead();
			}
		}
	}
	void GameOverWindow(int windowID)
	{
		Player p = photonView.gameObject.GetComponentInChildren<Player>();

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
