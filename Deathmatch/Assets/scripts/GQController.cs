using UnityEngine;
using System.Collections;

public class GQController : MonoBehaviour {
	public Transform firePoint;
	public Transform rayCaster;
	public GameObject DefaultProjectile;
	public AnimationClip idleAnimation;
	public AnimationClip walkAnimation;
	public AnimationClip runAnimation;
	public AnimationClip shootAnimation;
	public AnimationClip jumpPoseAnimation;
	public AnimationClip deadAnimation;
	public bool isDead = false;
	public float walkMaxAnimationSpeed = 0.75f;
	public float trotMaxAnimationSpeed = 1.0f;
	public float runMaxAnimationSpeed = 1.0f;
	public float jumpAnimationSpeed = 1.15f;
	public float landAnimationSpeed = 1.0f;
	//public var characterCam : Camera;
	private Animation _animation;
	public float coolDown = 0f;
	private bool deadAnimationPlayed = false;
	public enum CharacterState {
		Idle = 0,
		Walking = 1,
		Trotting = 2,
		Running = 3,
		Jumping = 4,
		Shoot = 5,
		Dead = 6,
	}
	
	public CharacterState _characterState;
	
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
	
	// The camera doesnt start following the target immediately but waits for a split second to avoid too much waving around.
	private float lockCameraTimer = 0.0f;
	
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
	public bool shooting = false;
	
	// the height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
	private float lastJumpStartHeight = 0.0f;
	
	
	private Vector3 inAirVelocity = Vector3.zero;
	
	private float lastGroundedTime = 0.0f;
	
	
	public bool isControllable = false;
	
	void Awake ()
	{
		if(this.isControllable && !isDead)
		{
			moveDirection = transform.TransformDirection(Vector3.forward);
		}
		_animation = GetComponent<Animation>();
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
		if(!jumpPoseAnimation && canJump) {
			_animation = null;
			Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
		}
		
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
				if(_characterState == CharacterState.Idle || !this.IsMoving())
				{

					Debug.Log("shoot");
					//PhotonNetwork.Instantiate("PurpleBullet",firePoint.transform.position,firePoint.transform.rotation,99);

					//Instantiate(DefaultProjectile,firePoint.transform.position+offset ,firePoint.transform.rotation);
				}else
				{
					_animation.Play(shootAnimation.name);
					//_characterState = CharacterState.Shoot;
					//PhotonNetwork.Instantiate("PurpleBullet",firePoint.transform.position,firePoint.transform.rotation,99);
					Instantiate(DefaultProjectile,rayCaster.transform.position+offset,rayCaster.transform.rotation);
				}
				
				Debug.DrawLine(firePoint.transform.position,hitInfo.point);
				//shootCooldown = .5f;
				//}
			}
			
		}
		
	}
	void Shoot()
	{
		CharacterController controller  = GetComponent<CharacterController>();

		if(_characterState == CharacterState.Idle || !this.IsMoving())
		{
			
			Debug.Log("shoot");
			Instantiate(DefaultProjectile,firePoint.transform.position,controller.transform.rotation);

		}else
		{
			if(!_animation.IsPlaying(shootAnimation.name) && _animation.isPlaying)
			{
				_animation.Stop();
				_animation[shootAnimation.name].speed = .25f;
				_animation.Play(shootAnimation.name);
			}
			Instantiate(DefaultProjectile,firePoint.transform.position,controller.transform.rotation);

		}

	}
	void UpdateSmoothedMovementDirection ()
	{
		//var cameraTransform = Camera.main.transform;

			Transform cameraTransform = gameObject.transform.parent.GetComponentInChildren<Camera>().transform;
			bool grounded = IsGrounded();
			
			// Forward vector relative to the camera along the x-z plane	
			var forward = cameraTransform.TransformDirection(Vector3.forward);
			forward.y = 0f;
			forward = forward.normalized;
			
			// Right vector relative to the camera
			// Always orthogonal to the forward vector
			Vector3 right = new Vector3(forward.z, 0f, -forward.x);
			
			float v = 0f;
			float h = 0f;
			
			if(this.isControllable && !isDead)
			{
				v = Input.GetAxisRaw("Vertical");
				h = Input.GetAxisRaw("Horizontal");
			}
			// Are we moving backwards or looking backwards
			if (v < -0.2)
				movingBack = true;
			else
				movingBack = false;

			// Target direction relative to the camera
			var targetDirection = h * right + v * forward;

			// Grounded controls
			if (grounded )
			{
				bool wasMoving = isMoving;
				isMoving = Mathf.Abs (h) > 0.1 || Mathf.Abs (v) > 0.1;
				
				
				// Lock camera for short period when transitioning moving & standing still
				lockCameraTimer += Time.deltaTime;
				if (isMoving != wasMoving)
					lockCameraTimer = 0.0f;
				
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
				
				
				if(_characterState != CharacterState.Dead)
					_characterState = CharacterState.Idle;

					// Smooth the speed based on the current target direction
				var curSmooth = speedSmoothing * Time.deltaTime;
					
					// Choose target speed
					//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
				var targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);
			//var targetSpeed = 1.0f;
					// Pick speed modifier
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
				{
					targetSpeed *= runSpeed;
					_characterState = CharacterState.Running;
				}
				else if (Time.time - trotAfterSeconds > walkTimeStart)
				{
					targetSpeed *= trotSpeed;
					_characterState = CharacterState.Trotting;
				}
				else
				{
					targetSpeed *= walkSpeed;
					_characterState = CharacterState.Walking;
				}
					
				moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
					
				// Reset walk time start when we slow down
				if (moveSpeed < walkSpeed * 0.3f)
					walkTimeStart = Time.time;

				
				
			
		}

		

		
		
	}
	
	
	void ApplyJumping ()
	{
		// Prevent jumping too fast after each other
		if (lastJumpTime + jumpRepeatTime > Time.time)
			return;
		
		if (IsGrounded() && this.isControllable) {
			// Jump
			// - Only when pressing the button down
			// - With a timeout so you can press the button slightly before landing		
			if (canJump && Time.time < lastJumpButtonTime + jumpTimeout) {
				verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
				SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	
	void ApplyGravity ()
	{
		if (this.isControllable)	// don't move player at all if not controllable.
		{
			// Apply gravity
			var jumpButton = Input.GetButton("Jump");
			
			
			// When we reach the apex of the jump we send out a message
			if (jumping && !jumpingReachedApex && verticalSpeed <= 0.0f)
			{
				jumpingReachedApex = true;
				SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
			}
			
			if (IsGrounded ())
				verticalSpeed = 0.0f;
			else
				verticalSpeed -= gravity * Time.deltaTime;
		}
	}
	
	float CalculateJumpVerticalSpeed (float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * targetJumpHeight * gravity);
	}
	
	void DidJump ()
	{
		jumping = true;
		jumpingReachedApex = false;
		lastJumpTime = Time.time;
		lastJumpStartHeight = transform.position.y;
		lastJumpButtonTime = -10;
		
		_characterState = CharacterState.Jumping;
	}
	
	void Update() {

		if(_characterState != CharacterState.Dead)
		{
			if (isControllable && Input.GetButtonDown ("Jump"))
			{
				lastJumpButtonTime = Time.time;
			}

			UpdateSmoothedMovementDirection();

			if(coolDown > 0.0f)
			{
				this.shooting = false;
				coolDown -= Time.deltaTime;
			}
			// Apply gravity
			// - extra power jump modifies gravity
			// - controlledDescent mode modifies gravity


				CharacterController controller  = GetComponent<CharacterController>();
				Vector3 movement = Vector3.zero;
				ApplyGravity ();
				
				// Apply jumping logic
				ApplyJumping ();
				if(isControllable)
			 	{
					// Calculate actual motion
					movement = moveDirection * moveSpeed + new Vector3 (0, verticalSpeed, 0) + inAirVelocity;
					movement *= Time.deltaTime;
				}
				// Move the controller

				collisionFlags = controller.Move(movement);
			if(Input.GetKeyUp(KeyCode.Space))
			{
				this.shooting = false;
				
			}
			if(Input.GetKeyDown(KeyCode.Space))
			{
				if(coolDown <= 0.0f)
				{
					if(this.isControllable)
					{
						this.shooting = true;
						Shoot();
						coolDown = .25f;
					}else
					{
						this.shooting = true;
					}
				}
				
			}

			// ANIMATION sector
			if(_animation) {
				if(_characterState == CharacterState.Jumping) 
				{
					if(!jumpingReachedApex) {
						_animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;
						_animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
						_animation.CrossFade(jumpPoseAnimation.name);
					} else {
						_animation[jumpPoseAnimation.name].speed = -landAnimationSpeed;
						_animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
						_animation.CrossFade(jumpPoseAnimation.name);				
					}
				} 
				else 
				{
					if(this.isControllable && controller.velocity.sqrMagnitude < 0.5f && !isDead) {
							if(_characterState != CharacterState.Shoot)
							{
								_animation.CrossFade(idleAnimation.name);
								this._characterState = CharacterState.Idle;
							}
							
					}
					else 
					{

						if(_characterState == CharacterState.Idle)
						{
							_animation.CrossFade(idleAnimation.name);
						}
						else if(_characterState == CharacterState.Running) {
							if(isControllable) _animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, runMaxAnimationSpeed);
							_animation.CrossFade(runAnimation.name);	
						}

						else if(_characterState == CharacterState.Trotting) {
							if(isControllable) _animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, trotMaxAnimationSpeed);
							_animation.CrossFade(walkAnimation.name);	
						}
						else if(_characterState == CharacterState.Walking) {
							if(isControllable) _animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, walkMaxAnimationSpeed);
							_animation.CrossFade(walkAnimation.name);	
						}
						else if(_characterState == CharacterState.Dead){

							_animation[deadAnimation.name].wrapMode = WrapMode.Once;
							_animation.Play (deadAnimation.name);
						}

					}
				}
			}
			// ANIMATION sector
			
			// Set rotation to the move direction
			if (IsGrounded() && isControllable)
			{
				
				transform.rotation = Quaternion.LookRotation(moveDirection);
				
			}	
			else
			{

					Vector3 xzMove = movement;
					xzMove.y = 0f;
					if (xzMove.sqrMagnitude > 0.001f)
					{
						transform.rotation = Quaternion.LookRotation(xzMove);
					}

			}	
			
			// We are in jump mode but just became grounded
			if (IsGrounded() )
			{
				lastGroundedTime = Time.time;
				inAirVelocity = Vector3.zero;
				if (jumping)
				{
					jumping = false;
					SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
				}
			}
		}else
		{
			Dead();
		}
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit )
	{
		//	Debug.DrawRay(hit.point, hit.normal);
		if (hit.moveDirection.y > 0.01) 
			return;
	}
	
	public float GetSpeed () {
		return moveSpeed;
	}
	
	public bool IsJumping () {
		return jumping;
	}
	
	public bool IsGrounded () {
		return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}
	
	public Vector3 GetDirection () {
		return moveDirection;
	}
	
	public bool IsMovingBackwards () {
		return movingBack;
	}
	
	public float GetLockCameraTimer () 
	{
		return lockCameraTimer;
	}
	
	public bool IsMoving () 
	{
		return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5;
	}
	
	public bool HasJumpReachedApex ()
	{
		return jumpingReachedApex;
	}
	
	public bool IsGroundedWithTimeout ()
	{
		return lastGroundedTime + groundedTimeout > Time.time;
	}
	
	public void Reset ()
	{
		gameObject.tag = "Player";
	}
	public void TakeDamageAnim()
	{

	}

	public void Dead()
	{

		this.isDead = true;
		this._characterState = CharacterState.Dead;
		if(!deadAnimationPlayed)
		{
			_animation.Play(deadAnimation.name);
			deadAnimationPlayed = true;
			PhotonNetwork.RPC( transform.parent.GetComponent<PhotonView>(),"GameOver",PhotonTargets.All,null);
		}

	}
}
