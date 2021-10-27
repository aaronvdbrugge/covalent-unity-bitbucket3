using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Makes a skewed grid of obejcts for isometric perspectives.
/// For use in editor
/// </summary>
public class MakeSkewedGrid : MonoBehaviour
{
	public GameObject prefab;

	public int gridWide = 5;
	public int gridHight = 5;

	public Vector2 offsetX;
	public Vector2 offsetY;


	#if UNITY_EDITOR

	[ContextMenu("Make Grid")]
	public void MakeGrid()
	{
		for( int x=0; x<gridWide; x++)
			for( int y=0; y<gridHight; y++)
			{
				GameObject go =  PrefabUtility.InstantiatePrefab(prefab) as GameObject;  //Instantiate( cardPrefab, transform );   // we pre-instantiate the cards now
				go.transform.SetParent(transform, false);
				go.transform.localPosition = offsetX * x + offsetY * y;
			}
	}



	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		// Show where the objects will be created
		for( int x=0; x<gridWide; x++)
			for( int y=0; y<gridHight; y++)
				Gizmos.DrawWireSphere( transform.localToWorldMatrix.MultiplyPoint( offsetX * x + offsetY * y ), 0.25f );
	}

	#endif
}
