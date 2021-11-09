using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used for penguin toy to keep it from blasting through the walls.
/// Unity's continuous collision detection is supposed to prevent this,
/// but I did see the penguin escape its bounds. Hopefully this
/// will be enough to contain it.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class LimitRigidbodySpeed : MonoBehaviour
{
    public float speedLimit = 1.0f;

    [Tooltip("Enabling this will just force it to go fast so we can tweak max speed")]
    public bool testLimit = false;

    Rigidbody2D _rigidbody2D;
	private void Start()
	{
		_rigidbody2D = GetComponent<Rigidbody2D>();
	}


	void FixedUpdate()
    {
        float spd_sqr = _rigidbody2D.velocity.sqrMagnitude;
        if( spd_sqr > (speedLimit*speedLimit) || testLimit )
        {
            if( spd_sqr == 0 )  // special case when testing limit
                _rigidbody2D.velocity = Vector2.right * speedLimit;
            else   // cap speed at speedLimit
            {
                float spd = Mathf.Sqrt(spd_sqr);
                _rigidbody2D.velocity /= spd;
                _rigidbody2D.velocity *= speedLimit;
            }
        }
    }
}
