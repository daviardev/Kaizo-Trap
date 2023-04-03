using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {
	public PlayerData Data;

	#region Variables movements
	// components
    public Rigidbody2D rb { get; private set; }

	// variables control the various actions the player can perform at any time.
	// these are fields which can are public allowing for other sctipts to read them
	// but can only be privately written to.
	public bool isSliding { get; private set; }
	public bool isJumping { get; private set; }

	public bool isFacingRight { get; private set; }
	public bool isWallJumping { get; private set; }

	// timers (also all fields, could be private and a method returning a bool could be used)
	public float lastOnWallTime { get; private set; }
	public float lastOnGroundTime { get; private set; }
	public float lastOnWallLeftTime { get; private set; }
	public float lastOnWallRightTime { get; private set; }

	// jump
	private bool isJumpCut;
	private bool isJumpFalling;

	// wall Jump
	private int lastWallJumpDir;
	private float wallJumpStartTime;

	private Vector2 moveInput;
	public float lastPressedJumpTime { get; private set; }

	// set all of these up in the inspector
	[Header("Checks")] 
	[SerializeField] private Transform groundCheckPoint;
	// size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
	[SerializeField] private Vector2 groundCheckSize = new Vector2(.49f, .03f);
	[Space(5)]
	[SerializeField] private Vector2 wallCheckSize = new Vector2(.5f, 1f);
	[SerializeField] private Transform backWallCheckPoint;
	[SerializeField] private Transform frontWallCheckPoint;

    [Header("Layers & Tags")]
	[SerializeField] private LayerMask groundLayer;
	#endregion

    private void Awake() {
		rb = gameObject.GetComponent<Rigidbody2D>();
	}

	private void Start() {
		isFacingRight = true;
		SetGravityScale(Data.gravityScale);
	}

	private void Update() {
        #region Timers
		lastOnWallTime -= Time.deltaTime;
        lastOnGroundTime -= Time.deltaTime;
		lastOnWallLeftTime -= Time.deltaTime;
		lastOnWallRightTime -= Time.deltaTime;
		lastPressedJumpTime -= Time.deltaTime;
		#endregion

		#region Input handler
		moveInput.x = Input.GetAxisRaw("Horizontal");
		moveInput.y = Input.GetAxisRaw("Vertical");

		if (moveInput.x != 0) CheckDirectionToFace(moveInput.x > 0);

		if(Input.GetKeyDown(KeyCode.Space)) OnJumpInput();

		if (Input.GetKeyUp(KeyCode.Space)) OnJumpUpInput();
		#endregion

		#region Collision checks
		if (!isJumping) {
			// ground Check
            // checks if set box overlaps with ground
			if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer) && !isJumping) {
				lastOnGroundTime = Data.coyoteTime; // if so sets the lastGrounded to coyoteTime
            }

			// right Wall Check
			if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer)
                    && isFacingRight)
					|| (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer)
                    && !isFacingRight))
                    && !isWallJumping)
                lastOnWallRightTime = Data.coyoteTime;

			// left Wall Check
			if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer)
                    && !isFacingRight)
				    || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer)
                    && isFacingRight))
                    && !isWallJumping)
                lastOnWallLeftTime = Data.coyoteTime;

			// two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
			lastOnWallTime = Mathf.Max(lastOnWallLeftTime, lastOnWallRightTime);
		}
		#endregion

		#region Jump checks
		if (isJumping && rb.velocity.y < 0) {
			isJumping = false;

			if(!isWallJumping) isJumpFalling = true;
		}

		if (isWallJumping && Time.time - wallJumpStartTime > Data.wallJumpTime) isWallJumping = false;

		if (lastOnGroundTime > 0 && !isJumping && !isWallJumping) {
			isJumpCut = false;

			if(!isJumping) isJumpFalling = false;
		}

		// jump
		if (CanJump() && lastPressedJumpTime > 0) {
			isJumping = true;
			isJumpCut = false;
			isWallJumping = false;
			isJumpFalling = false;

			Jump();
		} else if (CanWallJump() && lastPressedJumpTime > 0) { // wall jump
			isJumping = false;
			isJumpCut = false;
			isWallJumping = true;
			isJumpFalling = false;
			wallJumpStartTime = Time.time;
			lastWallJumpDir = (lastOnWallRightTime > 0) ? -1 : 1;
			
			WallJump(lastWallJumpDir);
		}
		#endregion

		#region Slide checks
		isSliding = CanSlide() && ((lastOnWallLeftTime > 0 && moveInput.x < 0) || (lastOnWallRightTime > 0 && moveInput.x > 0)) ? true : false;

		#endregion

		#region Gravity
		// higher gravity if we've released the jump input or are falling
		if (isSliding) {
			SetGravityScale(0);
		} else if (rb.velocity.y < 0 && moveInput.y < 0) {
			// much higher gravity if holding down
			SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
			// caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFastFallSpeed));
		} else if (isJumpCut) {
			// higher gravity if jump button released
			SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFallSpeed));
		} else if ((isJumping || isWallJumping || isJumpFalling) && Mathf.Abs(rb.velocity.y) < Data.jumpHangTimeThreshold)
		{
			SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
		} else if (rb.velocity.y < 0) {
			// higher gravity if falling
			SetGravityScale(Data.gravityScale * Data.fallGravityMult);
			// caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFallSpeed));
		} else {
			// default gravity if standing on a platform or moving upwards
			SetGravityScale(Data.gravityScale);
		}
		#endregion
    }

    private void FixedUpdate() {
		// handle Run
		Run(isWallJumping ? Data.wallJumpRunLerp : 1);

		// handle Slide
		if (isSliding) Slide();
    }

    #region Input callbacks
	// methods which whandle input detected in Update()
    public void OnJumpInput() {
		lastPressedJumpTime = Data.jumpInputBufferTime;
	}

	public void OnJumpUpInput() {
		if (CanJumpCut() || CanWallJumpCut()) isJumpCut = true;
	}
    #endregion

    #region General methods
    public void SetGravityScale(float scale) {
		rb.gravityScale = scale;
	}
    #endregion

    #region Run methods
    private void Run(float lerpAmount) {
    // calculate the direction we want to move in and our desired velocity
    float targetSpeed = moveInput.x * Data.runMaxSpeed;

    // check if the "X" key is pressed and set the target speed to the fast run max speed if it is
    if (Input.GetKey(KeyCode.X)) {
        Data.fastRunTimer += Time.deltaTime;
        if (Data.fastRunTimer > .3f) { // wait for half a second before increasing speed
            targetSpeed = moveInput.x * Data.fastRunMaxSpeed * 1.5f; // increase speed by 1.5f
		if (Data.fastRunTimer > .7f) {
			targetSpeed = moveInput.x * Data.fastRunMaxSpeed * 1.9f;
		}
        }
    } else {
        Data.fastRunTimer = 0f;
    }

    // lerp to the target speed
    targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount);

    // calculate acceleration rate
    float accelRate;
    if (lastOnGroundTime > 0) {
        accelRate = (Mathf.Abs(targetSpeed) > .01f) ? Data.runAccelAmount : Data.runDeccelAmount;
    } else {
        accelRate = (Mathf.Abs(targetSpeed) > .01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
    }

    // add bonus jump apex acceleration
    if ((isJumping || isWallJumping || isJumpFalling) && Mathf.Abs(rb.velocity.y) < Data.jumpHangTimeThreshold) {
        accelRate *= Data.jumpHangAccelerationMult;
        targetSpeed *= Data.jumpHangMaxSpeedMult;
    }

    // conserve momentum
    if (Data.doConserveMomentum && Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && lastOnGroundTime < 0) {
        accelRate = 0; 
    }

    // calculate difference between current velocity and desired velocity
    float speedDif = targetSpeed - rb.velocity.x;
    float movement = speedDif * accelRate;

    // convert this to a vector and apply to rigidbody
    rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
		/*
		 * For those interested here is what AddForce() will do
		 * rb.velocity = new Vector2(rb.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / rb.mass, rb.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
}

	private void Turn() {
		// stores scale and flips the player along the x axis, 
		Vector3 scale = transform.localScale; 
		scale.x *= -1;
		transform.localScale = scale;

		isFacingRight = !isFacingRight;
	}
    #endregion

    #region Jump methods
    private void Jump() {
		// ensures we can't call Jump multiple times from one press
		lastPressedJumpTime = 0;
		lastOnGroundTime = 0;

		#region Perform jump
		// we increase the force applied if we are falling
		// this means we'll always feel like we jump the same amount 
		// (setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
		float force = Data.jumpForce;
		if (rb.velocity.y < 0)
			force -= rb.velocity.y;

		rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		#endregion
	}

	private void WallJump(int dir) {
		// ensures we can't call Wall Jump multiple times from one press
		lastOnGroundTime = 0;
		lastOnWallLeftTime = 0;
		lastPressedJumpTime = 0;
		lastOnWallRightTime = 0;

		#region Perform wall jump
		Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
		force.x *= dir; // apply force in opposite direction of wall

		if (Mathf.Sign(rb.velocity.x) != Mathf.Sign(force.x)) force.x -= rb.velocity.x;
        
        // checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
		if (rb.velocity.y < 0) force.y -= rb.velocity.y;

		// unlike in the run we want to use the Impulse mode.
		// the default mode will apply are force instantly ignoring masss
		rb.AddForce(force, ForceMode2D.Impulse);
		#endregion
	}
	#endregion

	#region Slide method
	private void Slide() {
		// works the same as the Run but only in the y-axis
		// this seems to work fine, buit maybe you'll find a better way to implement a slide into this system
		float speedDif = Data.slideSpeed - rb.velocity.y;	
		float movement = speedDif * Data.slideAccel;
		// so, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
		// the force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
		movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif)  * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

		rb.AddForce(movement * Vector2.up);
	}
    #endregion

    #region Check methods
    public void CheckDirectionToFace(bool isMovingRight) {
		if (isMovingRight != isFacingRight) Turn();
	}

    private bool CanJump() {
		return lastOnGroundTime > 0 && !isJumping;
    }

	private bool CanWallJump() {
		return lastPressedJumpTime > 0 && lastOnWallTime > 0 && lastOnGroundTime <= 0 && (!isWallJumping ||
			 (lastOnWallRightTime > 0 && lastWallJumpDir == 1) || (lastOnWallLeftTime > 0 && lastWallJumpDir == -1));
	}

	private bool CanJumpCut() {
		return isJumping && rb.velocity.y > 0;
    }

	private bool CanWallJumpCut() {
		return isWallJumping && rb.velocity.y > 0;
	}

	public bool CanSlide() {
		return (lastOnWallTime > 0 && !isJumping && !isWallJumping && lastOnGroundTime <= 0) ? true : false;
	}
    #endregion

    #region Editor methods
    private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
		Gizmos.DrawWireCube(backWallCheckPoint.position, wallCheckSize);
		Gizmos.DrawWireCube(frontWallCheckPoint.position, wallCheckSize);
	}
    #endregion
}