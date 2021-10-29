using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys this object if we're not in Debug mode.
/// </summary>
public class DestroyIfNotDebug : MonoBehaviour
{
    public DebugSettings debugSettings;

    void Awake()
    {
        if( debugSettings.mode != DebugSettings.DebugMode.Debug )
            Destroy( gameObject );
    }
}
