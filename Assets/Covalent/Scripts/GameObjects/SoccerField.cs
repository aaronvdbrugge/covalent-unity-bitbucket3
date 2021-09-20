using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerField : MonoBehaviour
{
    public Transform spawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("soccerball"))
        {
            var ball = collision.gameObject.GetComponent<BouncyBall>();
            ball.Respawn( BouncyBall.RespawnType.OutOfBounds );
        }
    }
}
