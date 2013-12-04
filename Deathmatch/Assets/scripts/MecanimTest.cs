using UnityEngine;
using System.Collections;

public class MecanimTest : MonoBehaviour {

	Animator _animator;

	public bool takeDamage = false;
	public float speed = 0f;
	public bool dead = false;
	public bool shoot = false;
	public bool shootRapid = false;
	public bool roll = false;
	public float strafe = 0f;
	public GameObject DefaultProjectile;
	public GameObject firePoint;
	public float shootCooldown = 0f;
	private float gravity = 20.0f;
	private Transform thisTransform;
	private float h;
	private float v;
	private CharacterController character;

	private Vector3 cameraVelocity;
	public Transform cameraPivot;
	public float rotateSpeed = 500.0f;
	Vector2 rotationSpeed = new Vector2(50,25);
	//Mobile Joysticks
	public Joystick moveJoystick;
	public Joystick rotateJoystick;
	private Vector3 pos;
	// Use this for initialization
	void Start () {
		_animator = GetComponent<Animator>();
		thisTransform = GetComponent<Transform>();
		character = GetComponent<CharacterController>();
	//Disable Joysticks if running in editor
	#if UNITY_EDITOR
		moveJoystick.Disable();
		rotateJoystick.Disable();
	#endif
	#if UNITY_STANDALONE_OSX
		moveJoystick.Disable();
		rotateJoystick.Disable();
	#endif

	}
	
	// Update is called once per frame
	void Update () {
		_animator.SetBool("TakeDamage",takeDamage);
		_animator.SetFloat("Speed",speed);
		_animator.SetBool("Dead",dead);
		_animator.SetBool("Shoot",shoot);
		_animator.SetBool("Roll",roll);
		_animator.SetBool("ShootRapid",shootRapid);
		_animator.SetFloat("Strafe",strafe);
		//Set Axis variables for Keyboard and Mouse Movement if in editor
		#if UNITY_EDITOR
			v = Input.GetAxis("Vertical");
			h = Input.GetAxis("Horizontal");
			UpdateMovement();
		#endif
		#if UNITY_STANDALONE_OSX
			v = Input.GetAxis("Vertical");
			h = Input.GetAxis("Horizontal");
			UpdateMovement();
		#endif
		//#if UNITY_ANDROID
		//	UpdateMobileMovement();
		//#endif
		CheckShoot();
		if(shootCooldown >0f)
		{
			shootCooldown -= Time.deltaTime;
			shoot = false;
		}
		Debug.Log(shootCooldown);
	}

	void CheckShoot()
	{
		//GameObject firePoint = GameObject.FindGameObjectWithTag("FirePoint");
		//GameObject gun = GameObject.Find("Cyborg Ninja Gun");

		Ray r = new Ray(firePoint.transform.position,firePoint.transform.forward);

		RaycastHit hitInfo;

		if(Physics.Raycast(r,out hitInfo,200f))
		{
			if(hitInfo.collider.gameObject.name == "Cube")
			{
				if(shootCooldown <= 0f)
				{
					shoot = true;
					Instantiate(DefaultProjectile,firePoint.transform.position,firePoint.transform.rotation);
					Debug.DrawLine(firePoint.transform.position,hitInfo.point);
					shootCooldown = .5f;
				}
			}

		}else
		{

			shoot = false;
		}
		Debug.DrawRay(firePoint.transform.position,firePoint.transform.forward,Color.red);
	}

	#region Mobile Joystick Movement
	void UpdateMobileMovement()
	{
		//character.isGrounded = true;
		Vector3 movement = thisTransform.TransformDirection( new Vector3( moveJoystick.position.x, 0, moveJoystick.position.y ) );
		speed = moveJoystick.position.y;
		strafe = moveJoystick.position.x;
		// We only want horizontal movement
		movement.y = 0f;
		movement.Normalize();
		
		Vector3 cameraTarget = Vector3.zero;
		
		// Apply movement from move joystick
		Vector2 absJoyPos = new Vector2( Mathf.Abs( moveJoystick.position.x ), Mathf.Abs( moveJoystick.position.y ) );	
		if ( absJoyPos.y > absJoyPos.x )
		{
			if ( moveJoystick.position.y > 0f )
			{

				/*movement *= forwardSpeed * absJoyPos.y;
				_characterState = CharacterState.Running;
				Debug.Log(_characterState);*/
			}
			else
			{
				//movement *= backwardSpeed * absJoyPos.y;
				//cameraTarget.z = moveJoystick.position.y * 0.75f;
			}
		}
		else
		{
			//movement *= sidestepSpeed * absJoyPos.x;
			
			// Let's move the camera a bit, so the character isn't stuck under our thumb
			cameraTarget.x = -moveJoystick.position.x * 0.5f;
		}
		
		// Check for jump
		if ( character.isGrounded )
		{
			/*if ( rotateJoystick.tapCount == 2 )
			{
				// Apply the current movement to launch velocity		
				velocity = character.velocity;
				velocity.y = jumpSpeed;			
			}*/
		}
		else
		{			
			// Apply gravity to our velocity to diminish it over time
			//velocity.y += Physics.gravity.y * Time.deltaTime;
			
			// Move the camera back from the character when we jump
			//cameraTarget.z = -jumpSpeed * 0.25f;
			
			// Adjust additional movement while in-air
			//movement.x *= inAirMultiplier;
			//movement.z *= inAirMultiplier;
		}
		
		//movement += velocity;	
		movement += Physics.gravity;
		movement *= Time.deltaTime;
		//_characterState = CharacterState.Running;
		//Debug.Log(_characterState);
		// Actually move the character	
		//character.Move( movement );
		
		//if ( character.isGrounded )
			// Remove any persistent velocity after landing	
			//velocity = Vector3.zero;
		
		// Seek camera towards target position

	    //Vector3 pos;
		pos = cameraPivot.localPosition;
		pos.x = Mathf.SmoothDamp( pos.x, cameraTarget.x, ref cameraVelocity.x, 0.3f );
		pos.z = Mathf.SmoothDamp( pos.z, cameraTarget.z, ref cameraVelocity.z, 0.5f );
		cameraPivot.localPosition = pos;

		// Apply rotation from rotation joystick
		//if ( character.isGrounded )
		//{
			Vector3 camRotation = rotateJoystick.position;
			camRotation.x *= rotationSpeed.x;
			//camRotation.y *= rotationSpeed.y;
			camRotation *= Time.deltaTime;
			
			// Rotate the character around world-y using x-axis of joystick
			thisTransform.Rotate( 0f, camRotation.x, 0f, Space.World );
			
			// Rotate only the camera with y-axis input
			cameraPivot.Rotate( camRotation.y, 0f, 0f );
		//}
		character.transform.position = gameObject.transform.position;
	}

#endregion 

	#region Editor/Keyboard & Mouse Movement
	void UpdateMovement()
	{
		speed = v;
		strafe = h;
		// Generate a plane that intersects the transform's position with an upwards normal.
		Plane playerPlane = new Plane(Vector3.up, thisTransform.position);
		float rotSpeed = .01f;
		// Generate a ray from the cursor position
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		
		// Determine the point where the cursor ray intersects the plane.
		// This will be the point that the object must look towards to be looking at the mouse.
		// Raycasting to a Plane object only gives us a distance, so we'll have to take the distance,
		//   then find the point along that ray that meets that distance.  This will be the point
		//   to look at.
		float hitdist = 0.0f;
		// If the ray is parallel to the plane, Raycast will return false.
		/*if (playerPlane.Raycast (ray, out hitdist)) 
		{
			// Get the point along the ray that hits the calculated distance.
			Vector3 targetPoint = ray.GetPoint(hitdist);
			
			// Determine the target rotation.  This is the rotation if the transform looks at the target point.
			Quaternion targetRotation = Quaternion.LookRotation(targetPoint - thisTransform.position);
			
			// Smoothly rotate towards the target point.
			thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, targetRotation, rotSpeed * Time.time);
		}*/
	}
	#endregion
	void FixedUpdate()
	{

	}

}
