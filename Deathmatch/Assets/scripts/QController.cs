

using UnityEngine;
using System.Collections;



public class QController : MonoBehaviour {
	public AnimationClip idleAnimation;
	public AnimationClip walkAnimation;
	public AnimationClip runAnimation;
	public AnimationClip standAttackSingleShot;
	public AnimationClip standAttackRapidFire;
	public AnimationClip runAttackSingleShot;
	public AnimationClip runAttackRapidFire;
	public AnimationClip forwardRoll;
	public AnimationClip death;
	public AnimationClip takeDamage;

	public float walkMaxAnimationSpeed = 0.75f;
	public float trotMaxAnimationSpeed = 1.0f;
	public float runMaxAnimationSpeed  = 1.0f;
	public float jumpAnimationSpeed  = 1.15f;
	public float landAnimationSpeed  = 1.0f;
	
	private Animation _animation;

	enum CharacterState {
		Idle = 0,
		Walking = 1,
		Trotting = 2,
		Running = 3,
		Jumping = 4,
		RunShootSingle = 5,
		RunShootRapid = 6,
		StandShootSingle = 7,
		StandShootRapid = 8,
		TakeDamage = 9,
		Death = 10,
		ForwardRoll = 11
	}

	private CharacterState _characterState;

	// The speed when walking
	public float walkSpeed = 2.0f;
	// after trotAfterSeconds of walking we trot with trotSpeed
	public float trotSpeed = 4.0f;
	// when pressing "Fire3" button (cmd) we start running
	public float runSpeed = 6.0f;
	
	public float inAirControlAcceleration = 3.0f;
	
	// How high do we jump when pressing jump and letting go immediately
	public float jumpHeight = 0.5f;
	
	// The gravity for the character
	public float gravity = 20.0f;
	// The gravity in controlled descent mode
	public float speedSmoothing = 10.0f;
	public float rotateSpeed = 500.0f;
	public float trotAfterSeconds = 3.0f;
	
	public bool canJump = true;

	private float jumpRepeatTime = 0.05f;
	private float jumpTimeout = 0.15f;
	private float groundedTimeout = 0.25f;

	#region Non-Android
	// The current move direction in x-z
	private Vector3 moveDirection = Vector3.zero;
	// The current vertical speed
	private float verticalSpeed = 0.0f;
	// The current x-z move speed
	private float moveSpeed = 0.0f;
	// The last collision flags returned from controller.Move
	private CollisionFlags collisionFlags; 
	
	// Are we jumping? (Initiated with jump button and not grounded yet)
	private bool jumping = false;
	private bool jumpingReachedApex = false;
	
	// Are we moving backwards (This locks the camera to not do a 180 degree spin)
	private bool movingBack = false;
	// Is the user pressing any keys?
	private bool isMoving = false;
	// When did the user start walking (Used for going into trot after a while)
	private float walkTimeStart = 0.0f;
	// Last time the jump button was clicked down
	private float lastJumpButtonTime = -10.0f;
	// Last time we performed a jump
	private float lastJumpTime = -1.0f;
	
	
	// the height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
	private float lastJumpStartHeight = 0.0f;
	
	
	private Vector3 inAirVelocity = Vector3.zero;
	
	private float lastGroundedTime = 0.0f;
	
	
	private bool isControllable = true;

	#endregion
	public Joystick moveJoystick;
	public Joystick rotateJoystick;

	public Transform cameraPivot;

	public float forwardSpeed = 4f;
	public float backwardSpeed = 1f;
	public float sidestepSpeed = 1f;
	public float jumpSpeed = 8f;
	public float inAirMultiplier = .25f;
	Vector2 rotationSpeed = new Vector2(50,25);

	private Transform thisTransform;
	private CharacterController character;
	private Vector3 cameraVelocity;
	private Vector3 velocity;

	// Use this for initialization
	void Start () {
		thisTransform = GetComponent<Transform>();
		character = GetComponent<CharacterController>();
#if UNITY_EDITOR
		moveJoystick.Disable();
		rotateJoystick.Disable();
#endif

	}

	void OnEndGame()
	{
		moveJoystick.Disable();
		rotateJoystick.Disable();
		this.enabled = false;
	}
	// Update is called once per frame
	void Update () 
	{

	#if UNITY_ANDROID_API
		Debug.Log("Android");
		UpdateMobileMovement();
	#endif

	#if UNITY_EDITOR
		Debug.Log("Editor");
		UpdateComputerMovement();
	#endif
	
	#if UNITY_STANDALONE_WIN
		Debug.log("Win");
		UpdateComputerMovement();
	#endif
		#if UNITY_EDITOR
		if (!isControllable)
		{
			// kill all inputs if not controllable.
			Input.ResetInputAxes();
		}
		
		
		// Calculate actual motion
		Vector3 movement = moveDirection * moveSpeed;
		movement *= Time.deltaTime;
		
		// Move the controller
		//var controller : CharacterController = GetComponent(CharacterController);
		//collisionFlags = controller.Move(movement);
		character.Move(movement);
		
		#endif

		GameObject firePoint = GameObject.Find("FirePoint");
		Ray aim = new Ray(firePoint.transform.position,Vector3.forward);
		RaycastHit hitInfo;
		if(Physics.Raycast(aim,out hitInfo,200f))
		{
			//Debug.Log(hitInfo.ToString());
			_characterState = CharacterState.RunShootRapid;
			Debug.Log("Bang Bang");
		}

		if(_animation) {
			if(_characterState == CharacterState.Jumping) 
			{
				/*if(!jumpingReachedApex) {
					_animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;
					_animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
					_animation.CrossFade(jumpPoseAnimation.name);
				} else {
					_animation[jumpPoseAnimation.name].speed = -landAnimationSpeed;
					_animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
					_animation.CrossFade(jumpPoseAnimation.name);				
				}*/
			} 
			else 
			{
				if(character.velocity.sqrMagnitude < 0.1f) {
					_animation.CrossFade(idleAnimation.name);
				}
				else 
				{
					if(_characterState == CharacterState.Running) {
						_animation[runAnimation.name].speed = Mathf.Clamp(character.velocity.magnitude, 0.0f, runMaxAnimationSpeed);
						_animation.CrossFade(runAnimation.name);	
					}
					else if(_characterState == CharacterState.Trotting) {
						_animation[walkAnimation.name].speed = Mathf.Clamp(character.velocity.magnitude, 0.0f, trotMaxAnimationSpeed);
						_animation.CrossFade(walkAnimation.name);	
					}
					else if(_characterState == CharacterState.Walking) {
						_animation[walkAnimation.name].speed = Mathf.Clamp(character.velocity.magnitude, 0.0f, walkMaxAnimationSpeed);
						_animation.CrossFade(walkAnimation.name);	
					}
					else if(_characterState == CharacterState.RunShootRapid)
					{
						//_animation[runAttackRapidFire.name].speed = Mathf.Clamp(character.velocity.magnitude, 0.0f, walkMaxAnimationSpeed);
						_animation[runAttackRapidFire.name].speed = 1f;
						//_animation.CrossFade(runAttackRapidFire.name);
						_animation.Play(runAttackRapidFire.name);
					}
					
				}
			}
		}


	}

	void Awake ()
	{

		moveDirection = transform.TransformDirection(Vector3.forward);
		Animation[] a= GetComponentsInChildren<Animation>();
		_animation = a[0].animation;
		_characterState = CharacterState.Idle;
		//Debug.Log(_animation.GetClipCount);
		if(!_animation)
			Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
		
		/*
public var idleAnimation : AnimationClip;
public var walkAnimation : AnimationClip;
public var runAnimation : AnimationClip;
public var jumpPoseAnimation : AnimationClip;	
	*/
		if(!idleAnimation) {
			_animation = null;
			Debug.Log("No idle animation found. Turning off animations.");
		}
		if(!walkAnimation) {
			_animation = null;
			Debug.Log("No walk animation found. Turning off animations.");
		}
		if(!runAnimation) {
			_animation = null;
			Debug.Log("No run animation found. Turning off animations.");
		}
		/*if(!jumpPoseAnimation && canJump) {
			_animation = null;
			Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
		}*/
		
	}
	void UpdateMobileMovement()
	{
		Vector3 movement = thisTransform.TransformDirection( new Vector3( moveJoystick.position.x, 0, moveJoystick.position.y ) );

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
				movement *= forwardSpeed * absJoyPos.y;
				_characterState = CharacterState.Running;
				Debug.Log(_characterState);
			}
			else
			{
				movement *= backwardSpeed * absJoyPos.y;
				cameraTarget.z = moveJoystick.position.y * 0.75f;
			}
		}
		else
		{
			movement *= sidestepSpeed * absJoyPos.x;
			
			// Let's move the camera a bit, so the character isn't stuck under our thumb
			cameraTarget.x = -moveJoystick.position.x * 0.5f;
		}
		
		// Check for jump
		if ( character.isGrounded )
		{
			if ( rotateJoystick.tapCount == 2 )
			{
				// Apply the current movement to launch velocity		
				velocity = character.velocity;
				velocity.y = jumpSpeed;			
			}
		}
		else
		{			
			// Apply gravity to our velocity to diminish it over time
			velocity.y += Physics.gravity.y * Time.deltaTime;
			
			// Move the camera back from the character when we jump
			cameraTarget.z = -jumpSpeed * 0.25f;
			
			// Adjust additional movement while in-air
			movement.x *= inAirMultiplier;
			movement.z *= inAirMultiplier;
		}
		
		movement += velocity;	
		movement += Physics.gravity;
		movement *= Time.deltaTime;
		_characterState = CharacterState.Running;
		Debug.Log(_characterState);
		// Actually move the character	
		character.Move( movement );

		if ( character.isGrounded )
			// Remove any persistent velocity after landing	
			velocity = Vector3.zero;
		
		// Seek camera towards target position
		var pos = cameraPivot.localPosition;
		pos.x = Mathf.SmoothDamp( pos.x, cameraTarget.x, ref cameraVelocity.x, 0.3f );
		pos.z = Mathf.SmoothDamp( pos.z, cameraTarget.z, ref cameraVelocity.z, 0.5f );
		cameraPivot.localPosition = pos;
		
		// Apply rotation from rotation joystick
		if ( character.isGrounded )
		{
			var camRotation = rotateJoystick.position;
			camRotation.x *= rotationSpeed.x;
			camRotation.y *= rotationSpeed.y;
			camRotation *= Time.deltaTime;
			
			// Rotate the character around world-y using x-axis of joystick
			thisTransform.Rotate( 0f, camRotation.x, 0f, Space.World );
			
			// Rotate only the camera with y-axis input
			cameraPivot.Rotate( camRotation.y, 0f, 0f );
		}

	}

	void UpdateComputerMovement()
	{
		Transform cameraTransform = Camera.main.transform;
		bool grounded = character.isGrounded;
		
		// Forward vector relative to the camera along the x-z plane	
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = 0f;
		forward = forward.normalized;
		
		// Right vector relative to the camera
		// Always orthogonal to the forward vector
		Vector3 right = new Vector3(forward.z, 0f, -forward.x);
		
		float v = Input.GetAxisRaw("Vertical");
		float h = Input.GetAxisRaw("Horizontal");
		
		// Are we moving backwards or looking backwards
		if (v < -0.2f)
			movingBack = true;
		else
			movingBack = false;
		
		bool wasMoving = isMoving;
		isMoving = Mathf.Abs (h) > 0.1f || Mathf.Abs (v) > 0.1f;
		
		// Target direction relative to the camera
		Vector3 targetDirection = h * right + v * forward;
		
		// Grounded controls
		if (grounded)
		{
			// Lock camera for short period when transitioning moving & standing still
			/*lockCameraTimer += Time.deltaTime;
			if (isMoving != wasMoving)
				lockCameraTimer = 0.0f;
			*/
			// We store speed and direction seperately,
			// so that when the character stands still we still have a valid forward direction
			// moveDirection is always normalized, and we only update it if there is user input.
			if (targetDirection != Vector3.zero)
			{
				// If we are really slow, just snap to the target direction
				if (moveSpeed < walkSpeed * 0.9f && grounded)
				{
					moveDirection = targetDirection.normalized;
				}
				// Otherwise smoothly turn towards it
				else
				{
					moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
					
					moveDirection = moveDirection.normalized;
				}
			}
			
			// Smooth the speed based on the current target direction
			float curSmooth = speedSmoothing * Time.deltaTime;
			
			// Choose target speed
			//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
			float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);

			//_characterState = CharacterState.Idle;
			
			// Pick speed modifier
			if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
			{
				targetSpeed *= runSpeed;
				_characterState = CharacterState.Running;
			}
			else if (Time.time - trotAfterSeconds > walkTimeStart)
			{
				targetSpeed *= trotSpeed;
				_characterState = CharacterState.Running;
			}
			else
			{
				targetSpeed *= walkSpeed;
				_characterState = CharacterState.Running;
			}
			
			moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
			
			// Reset walk time start when we slow down
			if (moveSpeed < walkSpeed * 0.3f)
				walkTimeStart = Time.time;
		}

		// Generate a plane that intersects the transform's position with an upwards normal.
		Plane playerPlane = new Plane(Vector3.up, thisTransform.position);
		float speed = .01f;
		// Generate a ray from the cursor position
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		
		// Determine the point where the cursor ray intersects the plane.
		// This will be the point that the object must look towards to be looking at the mouse.
		// Raycasting to a Plane object only gives us a distance, so we'll have to take the distance,
		//   then find the point along that ray that meets that distance.  This will be the point
		//   to look at.
		float hitdist = 0.0f;
		// If the ray is parallel to the plane, Raycast will return false.
		if (playerPlane.Raycast (ray, out hitdist)) 
		{
			// Get the point along the ray that hits the calculated distance.
			Vector3 targetPoint = ray.GetPoint(hitdist);
			
			// Determine the target rotation.  This is the rotation if the transform looks at the target point.
			Quaternion targetRotation = Quaternion.LookRotation(targetPoint - thisTransform.position);
			
			// Smoothly rotate towards the target point.
			thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, targetRotation, speed * Time.time);
		}
		//Vector3 mRot = Input.mousePosition;
		velocity.y += Physics.gravity.y * Time.deltaTime;
		//Debug.Log(mRot.x);
		//thisTransform.Rotate( Vector3.RotateTowards(thisTransform.position,new Vector3(0f,mRot.x,0f),(rotateSpeed*.001f) * Mathf.Deg2Rad * Time.deltaTime,1000));
		//thisTransform.LookAt(new Vector3(0f,mRot.x,0f));

	}
}
