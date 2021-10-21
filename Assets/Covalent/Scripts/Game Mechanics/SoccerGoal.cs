using UnityEngine;


public class SoccerGoal : MonoBehaviour
{
    [Tooltip("Won't count as a goal if the ball is too high up.")]
    public float maxBallHeight = 1.0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("soccerball"))
        {
            var ball = collision.gameObject.GetComponent<BouncyBall>();
            if( ball.zPos <= maxBallHeight )
                ball.Respawn( BouncyBall.RespawnType.Goal );
        }
    }


    void OnDrawGizmos()
    {
        // Visualize max ball height
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3( 0, maxBallHeight, 0 ) );
    }
}
