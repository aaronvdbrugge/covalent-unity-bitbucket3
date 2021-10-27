using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCell : MonoBehaviour
{
    [Tooltip("For the skin we'll display")]
    public SkeletonMecanim skeletonMecanim;

    [Tooltip("Show this when we're selected")]
    public GameObject selectedVisual;

    
    [HideInInspector]
    public InventoryPanel inventoryPanel;    // will be set by InventoryPanel at runtime
    public int cellIndex;   // "
    public int skinIndex = -1;    // If we ever store non-skin items in these cells, leave this at -1


    /// <summary>
    /// Right now, setting "selected" state just shows/hides the visual
    /// </summary>
    public bool selected
    {
        get => selectedVisual.activeInHierarchy;
        set => selectedVisual.SetActive(value);
    }

    /// <summary>
    /// Call with a skin ID, and we'll set the skin ID of the mesh.
    /// </summary>
    public void SetSkin(string skin)
    {
		skeletonMecanim.skeleton.SetSkin(skin);    // Set the skin of the mesh!
		skeletonMecanim.skeleton.SetToSetupPose();
    }


    /// <summary>
    /// Attach the button to this
    /// </summary>
    public void TappedMe()
    {
        inventoryPanel.CellTapped(this);
    }


}
