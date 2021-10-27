using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnFirstUpdate : MonoBehaviour
{
    bool _didDisable = false;

    void FixedUpdate()
    {
        if( !_didDisable )
        {
            _didDisable = true;
            gameObject.SetActive(false);
        }
    }
}
