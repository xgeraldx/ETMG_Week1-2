using UnityEngine;
using System.Collections;

public class GameLogic : Photon.MonoBehaviour {
	public static PhotonView ScenePhotonView;

	// Use this for initialization
	void Start () {
		ScenePhotonView = GetComponent<PhotonView>();
	}
	
	// Update is called once per frame
	void Update () {

	}
	public static void TakeDamage(PhotonPlayer player, int id)
	{
		ScenePhotonView.RPC("TakeDamage",PhotonTargets.Others,null);
	}
}
