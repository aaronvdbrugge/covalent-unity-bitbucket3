using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys this object if we're not in Debug mode.
/// </summary>
public class DestroyIfNotDebug : MonoBehaviour
{
    public DebugSettings debugSettings;

    [Tooltip("Should we display this in SRDebuggerOnly mode? (used for DEVELOPMENT MODE warnings)")]
    public bool srDebuggerOnlyOk = false;

    void Awake()
    {
        if( debugSettings.mode != DebugSettings.BuildMode.Debug && !(srDebuggerOnlyOk && debugSettings.mode == DebugSettings.BuildMode.SRDebuggerOnly) )
            Destroy( gameObject );
    }
}
