using UnityEngine;
using System.Collections;

public class RPCFunctions : MonoBehaviour {
	Player p;
	// Use this for initialization
	void Start () {
		p = GetComponentInChildren<Player>();
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
			GQController gq = GetComponentInChildren<GQController>();
			//gq.isDead = true;
			gq.Dead();
		}
	}

	[RPC]
	void GameOver()
	{}


}
