using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place for any various GameObject utilities / extensions
/// </summary>
public static class GameObjectUtil 
{
	/// <summary>
	/// Sets all immediate children either active or inactive.
	/// </summary>
	public static void SetChildrenActive( this GameObject go, bool active )
	{
		foreach( Transform child in go.transform )
			child.gameObject.SetActive( active );
	}

	public static T[] GetComponentsInChildrenOnly<T>( this GameObject go )
	{
		return go.transform.GetComponentsInChildrenOnly<T>();
	}
}
