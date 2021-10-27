using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// TextMeshPro text can't be animated (because it's a string), this fixes that
/// </summary>
public class SetTextMeshProText : MonoBehaviour
{
    public TMP_Text text;
    public string[] strings;
    public int stringIndex;

    void Update()
    {
        text.text = strings[stringIndex];
    }
}
