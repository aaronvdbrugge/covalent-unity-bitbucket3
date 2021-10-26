using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows us to have an "off" graphic in a Toggle. Connect toggle change to OnChange
/// </summary>
public class HideObjectIfToggleOn : MonoBehaviour
{
    public GameObject hideObject;

    public void OnChange( Toggle t )
    {
        hideObject.SetActive(!t.isOn);
    }
}
