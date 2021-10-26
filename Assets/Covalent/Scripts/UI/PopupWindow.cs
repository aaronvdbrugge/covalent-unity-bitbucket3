using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Right now, attaching this is optional... it just lets you say if a popup
/// isn't "tap outtable" (you can tap outside it to close it).
/// If you don't attach this script, that'll default to true
/// </summary>
public class PopupWindow : MonoBehaviour
{
    public bool tapOuttable = true;
}
