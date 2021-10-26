using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this for basic UI popups.
/// Accesses them by name in hierarchy.
/// </summary>
public class PopupManager : MonoBehaviour
{
    [Tooltip("Empty string means no popup")]
    public string curPopup;


    /// <summary>
    /// Transparent BG panel which can be used to close some popups
    /// </summary>
    GameObject _closePanel;

	private void Start()
	{
		_closePanel = transform.Find("close").gameObject;
	}


	public void ShowPopup(string popup_name)
    {
        //Set all children to hidden...
        foreach( Transform c in transform )
            c.gameObject.SetActive(false);


        
        if( !string.IsNullOrEmpty(popup_name) )
        {
            // Show by name
            transform.Find(popup_name).gameObject.SetActive(true);

            // Show the BG raycaster
            _closePanel.SetActive(true);
        }


        //Remember what panel we're on.
        curPopup = popup_name;
    }


    /// <summary>
    /// Convenience method for when "Leave game" is pressed in a popup
    /// </summary>
    public void OnLeaveButton()
    {
        Dateland_Network.playerDidLeaveGame();
    }

}
