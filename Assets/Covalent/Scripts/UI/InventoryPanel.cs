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



	List<InventoryCell> _skinCells = new List<InventoryCell>();

	int _skinWhenEnabled = 0;


	private void Start()
	{
		for( int i=0; i<PlayerSkinManager.fullSkins.Count; i++ )
		{
			GameObject go = Instantiate( inventoryCellPrefab, cellLayout );
			go.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);   // Ensure pivot is on the bottom, this is necessary for proper mesh showing / hiding
			go.GetComponent<HideChildAboveTransform>().target = transform;    // hide meshes when they go above the top of this panel
			InventoryCell cell = go.GetComponent<InventoryCell>();
			cell.SetSkin(PlayerSkinManager.fullSkins[i]);    // Set the skin of the mesh!
			cell.selected = Player_Controller_Mobile.mine.spinePlayerController.characterSkinSlot == i;   // Select the skin we're currently using

			cell.inventoryPanel = this;
			cell.skinIndex = i;
			cell.cellIndex = i;   // for now these are one and the same, but if we start storing non-skin items here in the future, cellIndex may differ from skinIndex

			_skinCells.Add(cell);   // note that if this was an item, we wouldn't add it here
		}
	}


	private void OnEnable()
	{
		_skinWhenEnabled = Player_Controller_Mobile.mine.spinePlayerController.characterSkinSlot;  // in case they cancel
	}


	/// <summary>
	/// Called from InventoryCell
	/// </summary>
	public void CellTapped(InventoryCell cell)
	{
		//Deselect currently selected skin
		_skinCells[ Player_Controller_Mobile.mine.spinePlayerController.characterSkinSlot ].selected = false;

		// Set the player skin to the index of the tapped cell.
		Player_Controller_Mobile.mine.spinePlayerController.SetFullSkin( cell.skinIndex );

		//Select newly selected skin
		_skinCells[ Player_Controller_Mobile.mine.spinePlayerController.characterSkinSlot ].selected = true;
	}


	/// <summary>
	/// Goes back to the skin we had when we were enabled
	/// </summary>
	public void Cancel()
	{
		CellTapped( _skinCells[ _skinWhenEnabled ] );
	}


}
