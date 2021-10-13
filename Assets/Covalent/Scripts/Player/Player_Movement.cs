using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class is intended to hold logic only related to the player's actual movement.
/// No voice chat code, emojis, etc., even animation! Just movement.
/// Should be only be enabled if photonView.isMine
/// </summary>

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    public float maxSpeed = 5f;

	public bool useAcceleration = false;
	public float acceleration = 1.0f;

	

	[HideInInspector]public bool isMine = false;  //Set this from Player Controller Mobile. It affects how we interpret the directions we're "facing"


    [Tooltip("In an isometric world, y velocity needs to be scaled down.")]
    public float yVelocityScale = 0.5f;


	[Tooltip("Multiply maxSpeed by this when you do WASD in the editor")]
	public float editorSpeedMultiple = 4;

	//You can turn this off to disable control.
	public bool movementEnabled
	{
		get
		{
			return _movementEnabled;
		}
		set
		{
			_movementEnabled = value;
			if( !_movementEnabled )
				body.velocity = Vector3.zero;   // on set off, zero the velocity
		}
	}
	bool _movementEnabled = true;






	/// <summary>
	/// Last reading from the joystick. Reading this might be handy for kicking soccer balls,
	/// for example.
	/// </summary>
	Vector2 lastMovementInput;

	/// <summary>
	/// If this is "my" player, we'll set this directly from input; if not, we'll infer it from
	/// the player's velocity.
	/// </summary>
	public Vector2 GetLastMovementInput()
	{
		if( isMine )
			return lastMovementInput;
		else
		{
			Vector2 flat_velocity = body.velocity;

			// undo any isometric skewing on the velocity
			flat_velocity.y /= yVelocityScale;

			return flat_velocity / maxSpeed;   //fair assumption that this non-owned player is holding the joystick in this amount
		}
	}



	/// <summary>
	/// Last direction they pushed (useful for things like "heading" soccer balls at max speed despite not actually moving)
	/// If this is "my" player, we'll set this directly from input; if not, we'll infer it from
	/// the player's velocity.
	/// </summary>
	public Vector2 lastDirection = Vector2.one;



    /// <summary>
    /// Auto-generated from Unity's input action importer, only if we're controlled by user. May be overcomplicating things?
    /// </summary>
    public Rigidbody2D body {get; private set; }
	

	private void Awake()
	{
		body = GetComponent<Rigidbody2D>();
	}


	private void Start()
	{
        // Change to dynamic rigidbody
		if( isMine )
			body.bodyType = RigidbodyType2D.Dynamic;
	}


	/// <summary>
	/// Connect this to MyOnScreenStick.onJoystickValues.
	/// 
	/// We only do this for "my" player
	/// </summary>
	public void SetJoystick(Vector2 joyval)
	{
		lastMovementInput = joyval;
	}


	private void FixedUpdate()
	{
		if( movementEnabled  )
		{
			if( isMine )
			{
				Vector2 new_vel = lastMovementInput * maxSpeed;

				if( lastMovementInput.x != 0 || lastMovementInput.y != 0 )
					lastDirection = lastMovementInput.normalized;

				// Allow super-speed WASD in editor
				if ( Application.isEditor )
				{
					Vector2 editor_vel = Vector2.zero;
					if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
						editor_vel.y += 1;
					if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
						editor_vel.y -= 1;
					if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
						editor_vel.x -= 1;
					if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
						editor_vel.x += 1;

					if( Input.GetKey(KeyCode.LeftShift) )
						editor_vel *= editorSpeedMultiple;

					new_vel += editor_vel * maxSpeed;
				}

				new_vel.y *= yVelocityScale;  // isometric movement

				body.velocity = new_vel;
			}
			// Else, it's not mine... 
			else if( body.velocity != Vector2.zero )  //just update lastDirection based on velocity.
				lastDirection = body.velocity.normalized;
		}
	}


	/// <summary>
	/// Can be used for animation etc.
	/// </summary>
	public bool IsWalking()
	{
		return body.velocity.magnitude > 0.01f;
	}

	public Vector2 GetVelocity()
	{
		return body.velocity;
	}

}
