using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Soccerball : Network_Object
{
    public Vector3 target;
    public bool moving;
    public float rotationDirection, t, lerptime, deltaMultiplier, rotation;
    public float original_rotation, magnitudeStop, rotation_reduction;
    private Rigidbody2D body;

    void Start()
    {
        rotationDirection = 0;
        lerptime = 1;
        t = 0;
        deltaMultiplier = 2f;
        rotation = -12.6f;
        original_rotation = -12.6f;
        magnitudeStop = 2f;
        rotation_reduction = 0.1f;
        body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (moving)
        {
            transform.Rotate(new Vector3(0, 0, rotationDirection * rotation));
            rotation = (rotation + rotation_reduction);

            if (body.velocity.magnitude <= magnitudeStop)
            {
                body.velocity = Vector2.zero;
                body.angularVelocity = 0;
                body.SetRotation(0);
                moving = false;
                rotation = original_rotation;
            }
        }
    }

    public void setRotation()
    {
        float x = GameObject.Find("Soccer_Rotation_Slider").GetComponent<Slider>().value;
        GameObject.Find("Soccer_Rotation_Text").GetComponent<Text>().text = "Rot_Speed: " + x.ToString("F1");
        rotation = -x;
        original_rotation = -x;
    }

    public void setKickPower()
    {
        float x = GameObject.Find("Soccer_Kick_Slider").GetComponent<Slider>().value;
        GameObject.Find("Soccer_Kick_Text").GetComponent<Text>().text = "Kick_Mult: " + x.ToString("F2");
        deltaMultiplier = x;
    }

    public void setRotationReduction()
    {
        float x = GameObject.Find("Soccer_RotReduce_Slider").GetComponent<Slider>().value;
        GameObject.Find("Soccer_RotReduce_Text").GetComponent<Text>().text = "Rot_Reduce: " + x.ToString("F2");
        rotation_reduction = x;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = 0;
            
            moving = true;

            body.velocity = collision.relativeVelocity * deltaMultiplier;

            if ((collision.relativeVelocity * deltaMultiplier)[0] < 0)
            {
                rotationDirection = -1;
            }
            else
            {
                rotationDirection = 1;
            }

            body.angularVelocity = 0;
        }
        
    }

    Vector3 startPos = new Vector3(3.13f, -3.45f, 0);
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("goal"))
        {
            StartCoroutine(goalScored());
        }
    }
    public IEnumerator goalScored()
    {
        yield return new WaitForSeconds(1f);
        gameObject.transform.position = startPos;
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        gameObject.GetComponent<Rigidbody2D>().angularVelocity = 0;
    }
}
