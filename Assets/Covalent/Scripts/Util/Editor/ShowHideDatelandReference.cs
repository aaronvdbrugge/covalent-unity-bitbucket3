using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/// <summary>
/// Keyboard shortcuts to switch the DATELAND REFERENCE sprite in scene
/// from 0 to 50 to 100% opacity.
/// </summary>
public class ShowHideDatelandReference : MonoBehaviour
{
    [MenuItem("My Utilities/Show hide Dateland reference %g")]
    static void ShowHide()
    {
        DatelandReference dr = FindObjectOfType<DatelandReference>(true);
        GameObject go = dr ? dr.gameObject : null;
        SpriteRenderer sr = go ? go.GetComponent<SpriteRenderer>() : null;
        if( go && sr )
        {

            // Alternate between 0, 50, and 100 percent opacity
            if( !go.activeInHierarchy )
            {
                go.SetActive(true);
                sr.color = new Color(1,1,1, 0.5f);
            }
            else
            {
                if( sr.color.a == 0.5f )
                    sr.color = new Color(1,1,1,1);
                else
                    go.SetActive(false);
            }

        }
    }
}
