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
	public Vector2 lastMovementInput{ get; private set; }


	/// <summary>
	/// Last direction they pushed (useful for things like "heading" soccer balls at max speed despite not actuall moving)
	/// </summary>
	public Vector2 lastDirection{ get; private set; } = Vector2.one;


    /// <summary>
    /// Auto-generated from Unity's input action importer, only if we're controlled by user. May be overcomplicating things?
    /// </summary>
    private Rigidbody2D body;
	

	private void Awake()
	{
		body = GetComponent<Rigidbody2D>();
	}


	private void Start()
	{
        // Change to dynamic rigidbody
        body.bodyType = RigidbodyType2D.Dynamic;
	}


	/// <summary>
	/// Connect this to MyOnScreenStick.onJoystickValues
	/// </summary>
	public void SetJoystick(Vector2 joyval)
	{
		lastMovementInput = joyval;
	}


	private void FixedUpdate()
	{
		if( movementEnabled )
		{
			Vector2 new_vel = lastMovementInput * maxSpeed;

			if( lastMovementInput.x != 0 || lastMovementInput.y != 0 )
				lastDirection = lastMovementInput.normalized;

			// Allow super-speed WASD in editor
			if ( Application.isEditor )
			{
				if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
					new_vel.y += editorSpeedMultiple;
				if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
					new_vel.y -= editorSpeedMultiple;
				if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
					new_vel.x -= editorSpeedMultiple;
				if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
					new_vel.x += editorSpeedMultiple;

				new_vel = maxSpeed * new_vel;
			}

			new_vel.y *= yVelocityScale;  // isometric movement

			body.velocity = new_vel;
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
