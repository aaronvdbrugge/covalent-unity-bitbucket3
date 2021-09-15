using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    Vector3 startPos = new Vector3(3.13f, -3.45f, 0);
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("soccerball"))
        {
            //This coroutine should handle the goal celebration
            StartCoroutine("goalScored");
            collision.gameObject.transform.position = startPos;
            collision.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            collision.gameObject.GetComponent<Rigidbody2D>().angularVelocity = 0;
        }
    }
    public IEnumerator goalScored()
    {
        yield return new WaitForSeconds(1f);
    }
}
