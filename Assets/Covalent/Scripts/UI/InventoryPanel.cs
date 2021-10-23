using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Right now, this just creates an entry for every skin the player is allowed to select.
/// In the future, we might put actual obtainable items up top?
/// </summary>
public class InventoryPanel : MonoBehaviour
{
    public GameObject inventoryCellPrefab;

	[Tooltip("Will Instantiate cells here")]
	public Transform cellLayout;


	private void Start()
	{
		for( int i=0; i<PlayerSkinManager.fullSkins.Count; i++ )
		{
			GameObject go = Instantiate( inventoryCellPrefab, cellLayout );
			go.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);   // Ensure pivot is on the bottom, this is necessary for proper mesh showing / hiding
			go.GetComponent<HideChildAboveTransform>().target = transform;    // hide meshes when they go above the top of this panel
			SkeletonMecanim sm = go.GetComponentInChildren<SkeletonMecanim>();
			sm.skeleton.SetSkin(PlayerSkinManager.fullSkins[i]);    // Set the skin of the mesh!
			sm.skeleton.SetToSetupPose();
		}
	}


}
