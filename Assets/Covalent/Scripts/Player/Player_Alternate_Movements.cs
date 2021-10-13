using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Switches Player_Movement settings aronud for Go Karts and Ice.
/// Saves original Player_Movement settings on Awake so we can go back to them.
/// 
/// Just switch currentMovement to -1 for normal, or to an index in otherMovementStyles, and it should happen automatically.
/// </summary>
public class Player_Alternate_Movements : MonoBehaviour
{
	[Serializable]
	public struct MovementStyle
	{
		public float maxSpeed;
		public bool useAcceleration;
		public float acceleration;
		public float drag;    // rigidbody drag
		public int layer;   // some movements, like Go Karts, require moving to a different layer
		public Collider2D collider;   // some movements, like Go Karts, require a larger collision bound

		public MovementStyle( float maxSpeed, bool useAcceleration, float acceleration, float drag, int layer, Collider2D collider )
		{
			this.maxSpeed = maxSpeed;
			this.useAcceleration = useAcceleration;
			this.acceleration = acceleration;
			this.drag = drag;
			this.layer = layer;
			this.collider = collider;
		}
	}

	public Player_Movement playerMovement;
	public MovementStyle[] otherMovementStyles;

	MovementStyle _originalStyle;


	public int currentMovement
	{
		get => _currentMovement;
		set
		{
			_currentMovement = value;
			if( _currentMovement == -1 )   // -1 is original style
				ApplyStyle( _originalStyle );
			else   //index in otherMovementStyles
				ApplyStyle( otherMovementStyles[_currentMovement] );
		}
	}
	int _currentMovement = -1;


	private void Awake()
	{
		// Find the enabled collider, use this for original movement style.
		var colliders = playerMovement.GetComponents<Collider2D>();
		Collider2D enabled_collider = null;
		foreach( Collider2D c in colliders )
			if(c.enabled)
				enabled_collider = c;

		_originalStyle = new MovementStyle( playerMovement.maxSpeed, playerMovement.useAcceleration, playerMovement.acceleration, playerMovement.body.drag, playerMovement.gameObject.layer, enabled_collider );
	}


	public void ApplyStyle( MovementStyle style )
	{
		playerMovement.maxSpeed = style.maxSpeed;
		playerMovement.useAcceleration = style.useAcceleration;
		playerMovement.acceleration = style.acceleration;
		playerMovement.body.drag = style.drag;
		playerMovement.gameObject.layer = style.layer;

		// Don't change a collider's "enabled" status unless you need to, otherwise might cause duplicate "OnTrigger" calls.
		// In addition, enable the new one before disabling the old one
		style.collider.enabled = true;

		if( _originalStyle.collider.enabled && _originalStyle.collider != style.collider )
			_originalStyle.collider.enabled = false;
		foreach( var s in otherMovementStyles )
			if( s.collider.enabled && s.collider != style.collider )
				s.collider.enabled = false;
	}
}
