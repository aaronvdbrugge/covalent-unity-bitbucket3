using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_Controls : MonoBehaviour
{
    bool debug;
    CanvasGroup player, soccer;

    void Start()
    {
        debug = true;
        player = GameObject.Find("Player_Debug_Console").GetComponent<CanvasGroup>();
        soccer = GameObject.Find("Soccer_Debug_Console").GetComponent<CanvasGroup>();
    }

    public void debugSwitch()
    {
        if (debug)
        {
            debug = false;
            player.alpha = 0; player.interactable = false;
            soccer.alpha = 0; soccer.interactable = false;
        }
        else
        {
            debug = true;
            player.alpha = 1; player.interactable = true;
            soccer.alpha = 1; soccer.interactable = true;
        }
    }
}
