using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agora_Public_Space : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            Player_Controller_Mobile p = collision.gameObject.GetComponent<Player_Controller_Mobile>();
            
        }
    }
}
