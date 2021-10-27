using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentUtil 
{
	/// <summary>
	/// Like GetComponentsInChildren, but ignores self
	/// </summary>
    public static T[] GetComponentsInChildrenOnly<T>( this Component c )
	{
		List<T> ret = new List<T>();

		foreach( Transform child in c.transform )
		{
			T child_comp = child.GetComponent<T>();
			if( child_comp != null )
				ret.Add(child_comp);
		}

		return ret.ToArray();
	}

    
}
