using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoBehaviourUtil
{
    public static T[] GetComponentsInChildrenOnly<T>( this MonoBehaviour mb )
	{
		return mb.transform.GetComponentsInChildrenOnly<T>();
	}
}
