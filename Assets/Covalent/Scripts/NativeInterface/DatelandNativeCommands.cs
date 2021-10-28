using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Any native-to-unity commands needed in the Dateland scene can just go here.
/// 
/// Can call them like the following:
/// unity.sendMessageToGO(withName: "SceneLoader", functionName: "functionName", message: parameter)
/// </summary>
public class DatelandNativeCommands : MonoBehaviour
{
    /// <summary>
    /// Will go back to the loading screen.
    /// unity.sendMessageToGO(withName: "SceneLoader", functionName: "quitGame")
    /// </summary>
    public void quitGame()
    {
        // Will go back to the loading screen
        Dateland_Network.playerDidLeaveGame();
    }
}
