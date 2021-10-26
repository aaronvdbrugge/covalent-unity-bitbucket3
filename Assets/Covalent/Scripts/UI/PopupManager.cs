using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this for basic UI popups.
/// Accesses them by name in hierarchy.
/// </summary>
public class PopupManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("If non empty, ShowPopup on Start")]
    public string startWithPopup = "";  


    [Header("Runtime")]
    [Tooltip("Empty string means no popup")]
    public string curPopup;

    [Tooltip("Popups aren't required to have this component. Can be null")]
    public PopupWindow curPopupWindow;


    /// <summary>
    /// Transparent BG panel which can be used to close some popups
    /// </summary>
    GameObject _closePanel;

	private void Start()
	{
		_closePanel = transform.Find("close").gameObject;

        if( !string.IsNullOrEmpty(startWithPopup))
            ShowPopup(startWithPopup);
	}


	public void ShowPopup(string popup_name)
    {
        //Set all children to hidden...
        foreach( Transform c in transform )
            c.gameObject.SetActive(false);


        
        if( !string.IsNullOrEmpty(popup_name) )
        {
            // Show by name
            GameObject popup = transform.Find(popup_name).gameObject;
            popup.SetActive(true);

            // Does it have anby special settings?
            curPopupWindow = popup.GetComponent<PopupWindow>();

            // Show the BG raycaster
            _closePanel.SetActive(true);
        }


        //Remember what panel we're on.
        curPopup = popup_name;
    }


    public void OnTapBackground()
    {
        //Only close the popup if it's "tap outtable"
        if( curPopupWindow != null && !curPopupWindow.tapOuttable )
            return;
        ShowPopup("");   // close current popup
    }


    /// <summary>
    /// Convenience method for when "Leave game" is pressed in a popup
    /// </summary>
    public void OnLeaveButton()
    {
        Dateland_Network.playerDidLeaveGame();
    }

}
