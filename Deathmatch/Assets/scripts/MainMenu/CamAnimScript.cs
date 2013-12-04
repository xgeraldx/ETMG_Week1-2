using UnityEngine;
using System.Collections;

public class CamAnimScript : MonoBehaviour {
	public Transform startLoc;
	public Transform endLoc;
	//public GameObject wayPoint1;
	public float speed = 3f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		 
	}

	void FixedUpdate()
	{
		transform.position = Vector3.Lerp(transform.position,endLoc.position,speed * Time.deltaTime);
		transform.rotation = Quaternion.Lerp(transform.rotation,endLoc.rotation,speed * Time.deltaTime);
	}
}
