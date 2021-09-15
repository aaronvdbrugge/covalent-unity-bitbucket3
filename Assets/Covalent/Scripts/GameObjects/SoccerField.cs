using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerField : MonoBehaviour
{
    Vector3 startPos = new Vector3(3.13f, -3.45f, 0);
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("soccerball"))
        {
            collision.gameObject.transform.position = startPos;
            collision.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            collision.gameObject.GetComponent<Rigidbody2D>().angularVelocity = 0;
        }
    }
}
