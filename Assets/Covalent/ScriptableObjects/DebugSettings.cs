using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Debug Settings")]
public class DebugSettings: ScriptableObject
{
	[Serializable]
	public enum BuildMode
	{
		Release,

		/// <summary>
		/// In this mode, we hide all debug interface, and behave as if we are in Release, EXCEPT
		/// for enabling SRDebugger, so we can trace exceptions.
		/// </summary>
		SRDebuggerOnly,  
		Debug
	}

	public BuildMode mode = BuildMode.Release; 
}