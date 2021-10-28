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

    public CanvasGroup[] fadeTheseForPopups;
    public float canvasGroupsFadeTime = 0.25f;



    [Header("Runtime")]
    [Tooltip("Empty string means no popup")]
    public string curPopup;

    [Tooltip("Popups aren't required to have this component. Can be null")]
    public PopupWindow curPopupWindow;


    /// <summary>
    /// Transparent BG panel which can be used to close some popups
    /// </summary>
    GameObject _closePanel;

    float _canvasGroupsFadeState = 0;   // 0 to 1. 0 is fully invisible


	private void Start()
	{
		_closePanel = transform.Find("close").gameObject;

        _canvasGroupsFadeState = Time.fixedDeltaTime / canvasGroupsFadeTime; // ensures the canvas groups will get their alpha set at least once after start

        if( !string.IsNullOrEmpty(startWithPopup))
            ShowPopup(startWithPopup);
	}


	public void ShowPopup(string popup_name)
    {
        if( popup_name == curPopup )
            return;

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



	private void FixedUpdate()
	{
		if( curPopup != "" && _canvasGroupsFadeState > 0 )  // need to fade out canvas groups
        {
            _canvasGroupsFadeState = Mathf.Max(0, _canvasGroupsFadeState - Time.fixedDeltaTime / canvasGroupsFadeTime);
            foreach( var cg in fadeTheseForPopups )
                cg.alpha = _canvasGroupsFadeState;
        }
        else if( curPopup == "" && _canvasGroupsFadeState < 1 )  // need to fade in canvas groups
        {
            _canvasGroupsFadeState = Mathf.Min(1, _canvasGroupsFadeState + Time.fixedDeltaTime / canvasGroupsFadeTime);
            foreach( var cg in fadeTheseForPopups )
                cg.alpha = _canvasGroupsFadeState;
        }

        // There is a bug here where the canvas groups flash on for a frame before doing the fade in.
        // Not sure what's going on there, I'll just brute force the bugfix by setting their scale to 0
        foreach( var cg in fadeTheseForPopups )
            cg.transform.localScale = _canvasGroupsFadeState < 0.1f ? Vector3.zero : Vector3.one;
	}
}
