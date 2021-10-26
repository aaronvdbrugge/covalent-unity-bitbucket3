using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Attached to the main Camera, this just receives ReadSignText messages
/// and shows the appropriate UI.
/// </summary>
public class SignReader : MonoBehaviour
{
    [Tooltip("So we can open the popup")]
    public PopupManager popupManager;

    [Tooltip("Sign text displayed here")]
    public TMP_Text uiText;

    public void ReadSignText(string sign_text)
    {
        uiText.text = sign_text;
        popupManager.ShowPopup( "sign_read" );
    }
}
