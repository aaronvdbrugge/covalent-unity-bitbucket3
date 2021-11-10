using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manually calls SRDebug.Init().
/// Make sure SRDebugger settings aren't set to Automatic loading.
/// </summary>
public class EnableSRDebuggerOnlyInDebug : MonoBehaviour
{
    public DebugSettings debugSettings;

    void Start()
    {
        if( debugSettings.mode == DebugSettings.BuildMode.Debug || debugSettings.mode == DebugSettings.BuildMode.SRDebuggerOnly)
            SRDebug.Init();
    }
}
