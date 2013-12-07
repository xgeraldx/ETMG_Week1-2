using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour {

	public float impulseForce =10;
	public GameObject muzzleFire;
	public GameObject explosion;
	public float damage;
	public GameObject[] detachOnDeath;
	
	void Start () {
		if (muzzleFire)
		{
			Instantiate(muzzleFire, transform.position, transform.rotation);
			//PhotonNetwork.Instantiate("purpleMuzzleFire",transform.position,transform.rotation,97);
		}
		rigidbody.AddForce(transform.forward * impulseForce, ForceMode.Impulse);
		
	}
	
	void Update () {
		
	}
	
	void OnCollisionEnter(Collision collision) {
		
		Instantiate(explosion, transform.position, transform.rotation);

		if (detachOnDeath.Length > 0) {
			for(var i=0;i < detachOnDeath.Length; i++)
			{
				detachOnDeath[i].transform.parent=null;
				ParticleSystem PS;  
				PS = detachOnDeath[i].GetComponent<ParticleSystem>();
				PS.enableEmission=false;

				//	PhotonNetwork.Destroy(detachOnDeath[i]);

				//detachOnDeath[0].GetPhotonView().isMine
				if(detachOnDeath[i] != null)
				{

					//PhotonNetwork.Destroy(detachOnDeath[i]);

					Destroy(detachOnDeath[i]);

				}

			}
		}

		Destroy(this.gameObject);
			//PhotonNetwork.Destroy(this.gameObject);
			
		
	}
}
