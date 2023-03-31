using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")] // create a new playerData object by right clicking in the Project Menu then Create/Player/Player Data and drag onto the player
public class PlayerData : MonoBehaviour {
	[Header("Gravity")]
	[HideInInspector] public float gravityScale; // strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D).
	[HideInInspector] public float gravityStrength; // downwards force (gravity) needed for the desired jumpHeight and jumpTimeToApex.
										  // also the value the player's rigidbody2D.gravityScale is set to.
	[Space(5)]
	public float fallGravityMult; // multiplier to the player's gravityScale when falling.
	public float maxFallSpeed; // maximum fall speed (terminal velocity) of the player when falling.
	[Space(5)]
	public float fastFallGravityMult; // larger multiplier to the player's gravityScale when they are falling and a downwards input is pressed.
									  // seen in games such as Celeste, lets the player fall extra fast if they wish.
	public float maxFastFallSpeed; // maximum fall speed(terminal velocity) of the player when performing a faster fall.
	
	// Run and boost
	public float boostAmount = 1f;
	public float fastRunTimer = 0f;
	public float boostSpeedMult = 2f;
	public float fastRunMaxSpeed = 15f;
	
	[Space(20)]

	[Header("Run")]
	public float runMaxSpeed; // target speed we want the player to reach.
	public float runAcceleration; // the speed at which our player accelerates to max speed, can be set to runMaxSpeed for instant acceleration down to 0 for none at all
	[HideInInspector] public float runAccelAmount; // the actual force (multiplied with speedDiff) applied to the player.
	public float runDecceleration; // the speed at which our player decelerates from their current speed, can be set to runMaxSpeed for instant deceleration down to 0 for none at all
	[HideInInspector] public float runDeccelAmount; // actual force (multiplied with speedDiff) applied to the player .
	[Space(5)]
	[Range(0f, 1)] public float accelInAir; // multipliers applied to acceleration rate when airborne.
	[Range(0f, 1)] public float deccelInAir;
	[Space(5)]
	public bool doConserveMomentum = true;

	[Space(20)]

	[Header("Jump")]
	public float jumpHeight; // height of the player's jump
	public float jumpTimeToApex; // time between applying the jump force and reaching the desired jump height. These values also control the player's gravity and jump force.
	[HideInInspector] public float jumpForce; // the actual force applied (upwards) to the player when they jump.

	[Header("Both Jumps")]
	public float jumpCutGravityMult; // multiplier to increase gravity if the player releases thje jump button while still jumping
	[Range(0f, 1)] public float jumpHangGravityMult; // reduces gravity while close to the apex (desired max height) of the jump
	public float jumpHangTimeThreshold; // speeds (close to 0) where the player will experience extra "jump hang". The player's velocity.y is closest to 0 at the jump's apex (think of the gradient of a parabola or quadratic function)
	[Space(.5f)]
	public float jumpHangMaxSpeedMult;		
	public float jumpHangAccelerationMult;

	[Header("Wall Jump")]
	public Vector2 wallJumpForce; // the actual force (this time set by us) applied to the player when wall jumping.
	[Space(5)]
	[Range(0f, 1f)] public float wallJumpRunLerp; // reduces the effect of player's movement while wall jumping.
	[Range(0f, 1.5f)] public float wallJumpTime; // time after wall jumping the player's movement is slowed for.
	public bool doTurnOnWallJump; // player will rotate to face wall jumping direction

	[Space(20)]

	[Header("Slide")]
	public float slideSpeed;
	public float slideAccel;

    [Header("Assists")]
	[Range(.01f, .5f)] public float coyoteTime; // grace period after falling off a platform, where you can still jump
	[Range(.01f, .5f)] public float jumpInputBufferTime; // grace period after pressing jump where a jump will be automatically performed once the requirements (eg. being grounded) are met.
	

	// unity Callback, called when the inspector updates
    private void OnValidate() {
		// calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
		gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
		
		// calculate the rigidbody's gravity scale (ie: gravity strength relative to unity's gravity value, see project settings/Physics2D)
		gravityScale = gravityStrength / Physics2D.gravity.y;

		// calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
		runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
		runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

		// calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
		jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

		#region Variable Ranges
		runAcceleration = Mathf.Clamp(runAcceleration, .01f, runMaxSpeed);
		runDecceleration = Mathf.Clamp(runDecceleration, .01f, runMaxSpeed);
		#endregion
	}
}