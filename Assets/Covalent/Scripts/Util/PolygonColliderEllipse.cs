using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Makes a perfectly elliptical polygon collider (meant for in-editor use)
/// </summary>
[ExecuteInEditMode]
public class PolygonColliderEllipse : MonoBehaviour
{
    public float xRadius;
    public float yRadius;
    public int points = 16;

    
    [ContextMenu("MakeEllipse")]
    public void MakeEllipse()
    {
        PolygonCollider2D polycol = GetComponent<PolygonCollider2D>();

        Vector2[] path = new Vector2[points];

        for( int i=0; i<points; i++ )
        {
            float angle = (MyMath.TAU * i) / points;
            float xdiff = Mathf.Cos( angle );
            float ydiff = Mathf.Sin( angle );

            path[i] = new Vector2(xdiff * xRadius, ydiff * yRadius);    // Apply ellipse transform to the bare cos/sin pair
        }


        polycol.pathCount = 1;
        polycol.SetPath(0, path); 
    }
}
