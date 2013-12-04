using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public float health = 5f;
	public GameObject playerrName;
	public PhotonView myView;
	// Use this for initialization
	void Start () {
		//gq = GetComponent<GQController>();
		//myView = transform.parent.GetComponent<PhotonView>();
		//Debug.Log(myView.viewID);
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnCollisionEnter(Collision collision)
	{
	    GQController gq = GetComponent<GQController>();
		Debug.Log(collision.gameObject.name);
		if(collision.gameObject.name.Contains("Bullet"))
		{
			if(!gq.isDead)
			{
				//myView.RPC ("TakeDamage",PhotonNetwork.player,null);
				health-=1.0f;
				if(health <= 0)
				{
					gq.Dead();
				}
			}
			//GameLogic.ScenePhotonView.RPC ("TakeDamage",PhotonNetwork.player,null);
		}
	}

	void OnGUI()
	{

			GUILayout.BeginArea(new Rect(Screen.width - 100, 0, 100, 25));
			GUILayout.Label("Health: " + health.ToString());
			GUILayout.EndArea();

	}
	
}
