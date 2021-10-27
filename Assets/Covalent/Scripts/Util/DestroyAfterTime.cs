using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys object after destroyTime has passed
/// </summary>
public class DestroyAfterTime: MonoBehaviour
{
    public float destroyTime;

    float _destroyTimer;
    

    void FixedUpdate()
    {
        if( _destroyTimer >= destroyTime )
            Destroy( gameObject );

        _destroyTimer += Time.fixedDeltaTime;
    }
}
