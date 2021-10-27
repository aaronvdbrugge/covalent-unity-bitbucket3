using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// NOTE: currently, the Native side sends the user's JSON profile to Unity via the following:
///     unity.sendMessageToGO(withName: "PhotonMono", functionName: "createPlayer", message: jsonString)
/// This object must be placed on an object called PhotonMono to receive that message, and it must be placed
/// in at least the entry scene of the project (probably LoadingScreen.unity) to ensure we get that 
/// function call.
/// </summary>
public class CreatePlayerReceiver : MonoBehaviour
{
    void Start()
    {
        if( gameObject.name != "PhotonMono" || transform.parent != null )
            Debug.LogError("CreatePlayerReceiver must be a root-level game object named \"PhotonMono\" to properly receive native function call!");
    }


    /// <summary>
    /// NOTE: This function gets called from the native side, and is given
    /// a payload of the user's real profile info.
    /// 
    /// In addition, the  "partyId" key will tell us what user we've been paired with,
    /// by including both userIDs in the string. E.g. "12:74" means users 12 and 74 are paired
    /// together. This would also mean that "user"->"id" should be either 12 or 74.
    /// </summary>
    private void createPlayer(string json_string)
    {
        Debug.Log("FROM NATIVE: createPlayer(" + json_string.Substring(0, 100) + "...)");

        Dateland_Network.realUserJson = json_string;
    }

}
