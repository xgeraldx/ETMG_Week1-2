using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour
{
	private Vector3 correctPlayerPos;
	private Quaternion correctPlayerRot;


	void Start()
	{
		//pv = transform.parent.GetComponent<PhotonView>();
	}
	// Update is called once per frame
	void Update()
	{
		if (!photonView.isMine)
		{
			Debug.Log("not me");

			transform.GetChild(1).transform.position = Vector3.Lerp(transform.GetChild(1).transform.position, this.correctPlayerPos, Time.deltaTime * 2);
			transform.GetChild(1).transform.rotation = Quaternion.Lerp(transform.GetChild(1).transform.rotation, this.correctPlayerRot, Time.deltaTime * 2);
		}
		//Debug.Log("me");
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			// We own this player: send the others our data
			stream.SendNext(transform.GetChild(1).transform.position);
			stream.SendNext(transform.GetChild (1).transform.rotation);
			GQController gq = GetComponentInChildren<GQController>();
			Debug.Log("Player send: " + gq._characterState.ToString());
			stream.SendNext((int)gq._characterState);
			TextMesh tm = GetComponentInChildren<TextMesh>();
			stream.SendNext((string)tm.text);
			//MecanimTest mt = GetComponent<MecanimTest>();
			//stream.SendNext((float)mt.speed);
			//stream.SendNext((float)mt.strafe);
			//Debug.Log("Write: " + gq._characterState);
		}
		else
		{
			// Network player, receive data
			this.correctPlayerPos = (Vector3) stream.ReceiveNext();
			this.correctPlayerRot = (Quaternion) stream.ReceiveNext();
			GQController gq = GetComponentInChildren<GQController>();

			gq._characterState = (GQController.CharacterState)stream.ReceiveNext();
			//Debug.Log("Network Client: " + gq.isControllable.ToString());
			TextMesh tm = GetComponentInChildren<TextMesh>();
			tm.text = (string)stream.ReceiveNext();
			//MecanimTest mt = GetComponent<MecanimTest>();
			//mt.speed = (float)stream.ReceiveNext();
			//mt.strafe = (float)stream.ReceiveNext();
		}
	}
}