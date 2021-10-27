using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public Vector3 target;
    public Vector3 restartPos;
    public float cloudSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, cloudSpeed);

        if (transform.position.x / target.x >= 0.98f)
        {
            transform.position = restartPos;
        }

        
    }
}
