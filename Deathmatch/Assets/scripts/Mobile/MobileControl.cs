using UnityEngine;
using System.Collections;

public class MobileControl : MonoBehaviour {

	public Joystick moveJoystick;
	public Joystick rotateJoystick;
	public Transform rayCaster;
	public Transform firePoint;
	public Transform runFirePoint;
	public Transform cameraPivot;						// The transform used for camera rotation
	public Transform cameraTransform;					// The actual transform of the camera
	public GameObject defaultProjectile;

	public float speed = 5f;								// Ground speed
	public float jumpSpeed = 8f;
	public float inAirMultiplier = 0.25f; 				// Limiter for ground speed while jumping
	public Vector2 rotationSpeed = new Vector2( 50f, 25f );	// Camera rotation speed for each axis
	
	private Transform thisTransform;
	private CharacterController character;
	private Vector3 velocity;						// Used for continuing momentum while in air
	public bool canJump = false;

	private Animation _animation;
	float targetSpeed;
	private float shootCoolDown = 0f;
	enum CharacterState {
		Idle = 0,
		Walking = 1,
		Trotting = 2,
		Running = 3,
		Jumping = 4,
		Shoot = 5,
		Dead = 6,
	}

	private CharacterState _characterState;

	void Start()
	{
		// Cache component lookup at startup instead of doing this every frame	
		thisTransform = GetComponent<Transform>();
		character = GetComponent<CharacterController>();	
		_animation = GetComponentInChildren<Animation>();
		_characterState = CharacterState.Idle;
		// Move the character to the correct start position in the level, if one exists
		GameObject spawn = GameObject.Find( "PlayerSpawn" );
		if ( spawn )
			thisTransform.position = spawn.transform.position;	
	}
	
	void FaceMovementDirection()
	{	
		Vector3 horizontalVelocity = character.velocity;
		horizontalVelocity.y = 0f; // Ignore vertical movement
		
		// If moving significantly in a new direction, point that character in that direction
		if ( horizontalVelocity.magnitude > 0.1f )
			thisTransform.forward = horizontalVelocity.normalized;
	}
	
	void OnEndGame()
	{
		// Disable joystick when the game ends	
		moveJoystick.Disable();
		rotateJoystick.Disable();
		
		// Don't allow any more control changes when the game ends
		this.enabled = false;
	}

	void CheckShoot()
	{
		Vector3 offset = new Vector3(0f,-.5f,0f);
		//Vector3 offset2 = new Vector3(1f,-.5f,0f);
		Debug.DrawRay(rayCaster.transform.position,rayCaster.transform.forward,Color.red);
		
		Ray r = new Ray(rayCaster.transform.position,rayCaster.transform.forward);
		
		
		RaycastHit hitInfo;
		
		if(Physics.Raycast(r, out hitInfo,500f))
		{
			if(hitInfo.collider.gameObject.name == "Troop")
			{
				//if(shootCooldown <= 0f)
				//{
				//shoot = true;
				if(_characterState == CharacterState.Idle)
				{
					
					if(!_animation.IsPlaying("stand attack"))
					{
						_animation.Stop();
						_animation.Play("stand attack");
					}
					//PhotonNetwork.Instantiate("PurpleBullet",firePoint.transform.position,firePoint.transform.rotation,99);
					Instantiate(defaultProjectile,firePoint.position ,character.transform.rotation);
				}else
				{
					if(!_animation.IsPlaying("run attack"))
					{
						_animation.Stop();
						_animation.CrossFade("run attack");
					}
					//_characterState = CharacterState.Shoot;
					//PhotonNetwork.Instantiate("PurpleBullet",firePoint.transform.position,firePoint.transform.rotation,99);
					Instantiate(defaultProjectile,runFirePoint.position,character.transform.rotation);
				}
				
				Debug.DrawLine(firePoint.transform.position,hitInfo.point);
				//shootCooldown = .5f;
				//}
			}
			
		}
		
	}
	void Update()
	{
		if(shootCoolDown <= 0f)
		{
			CheckShoot();
			shootCoolDown = .5f;
		}else
		{
			shootCoolDown -= Time.deltaTime;
		}
		Vector3 movement = cameraTransform.TransformDirection( new Vector3( moveJoystick.position.x, 0f, moveJoystick.position.y ) );
		// We only want the camera-space horizontal direction
		movement.y = 0f;
		movement.Normalize(); // Adjust magnitude after ignoring vertical movement
		
		// Let's use the largest component of the joystick position for the speed.
		Vector2 absJoyPos = new Vector2( Mathf.Abs( moveJoystick.position.x ), Mathf.Abs( moveJoystick.position.y ) );
		movement *= speed * ( ( absJoyPos.x > absJoyPos.y ) ? absJoyPos.x : absJoyPos.y );
		
		// Check for jump
		if ( character.isGrounded )
		{
			if ( !rotateJoystick.IsFingerDown() )
				canJump = true;
			
			if ( canJump && rotateJoystick.tapCount == 2 )
			{
				// Apply the current movement to launch velocity
				velocity = character.velocity;
				velocity.y = jumpSpeed;
				_animation.Play ("forward roll");
				canJump = false;
			}

		}
		else
		{			
			// Apply gravity to our velocity to diminish it over time
			velocity.y += Physics.gravity.y * Time.deltaTime;
			
			// Adjust additional movement while in-air
			movement.x *= inAirMultiplier;
			movement.z *= inAirMultiplier;
		}
		var targetSpeed = Mathf.Min(velocity.magnitude, 1.0f);
		if(character.velocity.magnitude > 0f)
		{

			_characterState = CharacterState.Running;
		}
		else if(character.velocity.magnitude <=0f)
		{
			_characterState = CharacterState.Idle;
		}
		movement += velocity;
		movement += Physics.gravity;
		movement *= Time.deltaTime;
		
		// Actually move the character
		character.Move( movement );
		
		if ( character.isGrounded )
			// Remove any persistent velocity after landing
			velocity = Vector3.zero;
		
		// Face the character to match with where she is moving
		FaceMovementDirection();	
		
		// Scale joystick input with rotation speed
		Vector3 camRotation = rotateJoystick.position;
		camRotation.x *= rotationSpeed.x;
		camRotation.y *= rotationSpeed.y;
		camRotation *= Time.deltaTime;
		
		// Rotate around the character horizontally in world, but use local space
		// for vertical rotation
		cameraPivot.Rotate( 0f, camRotation.x, 0f, Space.World );
		cameraPivot.Rotate( camRotation.y, 0f, 0f );

		UpdateAnimations();
	}

	void UpdateAnimations()
	{


		if(_characterState == CharacterState.Idle)
		{
			_animation.Play("idle");
		}
		else if(_characterState == CharacterState.Running)
		{
			//_animation.Play ("run");
			if(!_animation.IsPlaying("forward roll"))
			{
				_animation.CrossFade("run");
			}
		}

	}
}
