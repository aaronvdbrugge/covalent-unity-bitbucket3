using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Debug Settings")]
public class DebugSettings: ScriptableObject
{
	[Serializable]
	public enum DebugMode
	{
		Release,
		Debug
	}

	public DebugMode mode = DebugMode.Release; 
}