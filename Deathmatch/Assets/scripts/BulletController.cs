using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour {

	public float impulseForce =10;
	public GameObject muzzleFire;
	public GameObject explosion;
	public float damage;
	public GameObject[] detachOnDeath;
	
	void Start () {
		rigidbody.AddForce(transform.forward * impulseForce, ForceMode.Impulse);
		
	}
	
	void Update () {
		
	}
	
	void OnCollisionEnter(Collision collision) {
		
		if (detachOnDeath.Length > 0) {
			for(var i=0;i < detachOnDeath.Length; i++)
			{
				detachOnDeath[i].transform.parent=null;
				ParticleSystem PS;  
				PS = detachOnDeath[i].GetComponent<ParticleSystem>();
				PS.enableEmission=false;

				//	PhotonNetwork.Destroy(detachOnDeath[i]);


				if(detachOnDeath[i] != null)

			}
		}


		
	}
}
