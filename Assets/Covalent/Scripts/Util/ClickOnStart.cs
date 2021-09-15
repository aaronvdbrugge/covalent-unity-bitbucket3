using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clicks the attached Button on start (handy for the "Create player" button)
/// </summary>
public class ClickOnStart : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.Invoke();
    }
}
