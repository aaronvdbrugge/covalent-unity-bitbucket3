using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// There were a lot of Edge Collider 2D components in scenes where
/// Polygon Collider 2D components would have worked better (harder
/// for players to get stuck inside of).
/// You can drop this component on an object to do the conversion 
/// in editor automatically.
/// </summary>
[ExecuteInEditMode]
public class EdgeCollider2DToPolygonCollider2D : MonoBehaviour
{
    void Start()
    {
        EdgeCollider2D edge = GetComponent<EdgeCollider2D>();
        if( edge )
        {
            PolygonCollider2D poly = gameObject.AddComponent( typeof( PolygonCollider2D ) ) as PolygonCollider2D;

            poly.pathCount = 1;
            poly.SetPath(0, edge.points );   //Initialize polygon collider with edge collider data...

            poly.offset = edge.offset;
            poly.isTrigger = edge.isTrigger;

            // We have a polygon collider now, destroy old edge collider...
            DestroyImmediate(edge);

            // May as well destroy ourselves too. We're just a utility script
            DestroyImmediate(this);
        }
    }
}
