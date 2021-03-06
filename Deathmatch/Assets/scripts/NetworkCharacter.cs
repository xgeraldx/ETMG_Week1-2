﻿using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour
{
	private Vector3 correctPlayerPos;
	private Quaternion correctPlayerRot;
	private Vector3 projectilePosition;
	private Quaternion projectileRotation;

	void Start()
	{

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

	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			// We own this player: send the others our data
			stream.SendNext(transform.GetChild(1).transform.position);
			stream.SendNext(transform.GetChild (1).transform.rotation);
			GQController gq = GetComponentInChildren<GQController>();

			stream.SendNext((int)gq._characterState);
			TextMesh tm = GetComponentInChildren<TextMesh>();
			stream.SendNext((string)tm.text);
			if(gq.shooting)
			{
				stream.SendNext((int)1);
			}else
			{
				stream.SendNext((int)0);
			}

			stream.SendNext(gq.firePoint.transform.position);

			stream.SendNext(gq.firePoint.transform.rotation);

		}
		else
		{
			// Network player, receive data
			this.correctPlayerPos = (Vector3) stream.ReceiveNext();
			this.correctPlayerRot = (Quaternion) stream.ReceiveNext();
			GQController gq = GetComponentInChildren<GQController>();
			PhotonView pv = GetComponent<PhotonView>();
			gq._characterState = (GQController.CharacterState)stream.ReceiveNext();

			TextMesh tm = GetComponentInChildren<TextMesh>();
			tm.text = (string)stream.ReceiveNext();
			if((int)stream.ReceiveNext() == 0)
			{
				gq.shooting = false;
			}else
			{
				gq.shooting = true;
			}

			projectilePosition = (Vector3)stream.ReceiveNext();
			projectileRotation = (Quaternion)stream.ReceiveNext();

			if(gq.shooting)
				photonView.RPC("RemotePlayerShoot",PhotonNetwork.player,projectilePosition,projectileRotation);
		}
	}
}