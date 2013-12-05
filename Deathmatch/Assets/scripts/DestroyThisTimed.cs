using UnityEngine;
using System.Collections;

public class DestroyThisTimed : MonoBehaviour {

	//this very basic script destroys its gameobject in a pre-set amount of time. I use it all the time
	//so effect holders and various stuffs do not remain in the scene.
	public float destroyTime=5f;
	
	void Start () {

		StartCoroutine("DestroyDelay");
	}
	void Update () {
	}
	IEnumerator DestroyDelay()
	{
		yield return new WaitForSeconds(destroyTime);
		if(this.gameObject.GetPhotonView().isMine)
		{
			if(gameObject != null)
				if(PhotonNetwork.isMasterClient)
					PhotonNetwork.Destroy(this.gameObject);
				//PhotonNetwork.Destroy(gameObject);
		}
		//yield return 0;
	}

}
