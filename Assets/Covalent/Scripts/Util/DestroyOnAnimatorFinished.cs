using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys object by checking normalizedTime.
/// Note that you likely need to turn off looping on the animation clip.
/// </summary>
public class DestroyOnAnimatorFinished : MonoBehaviour
{
    [Tooltip("If null we'll destroy ourselves (parent might be useful here).")]
    public GameObject toDestroy;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if( animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f )
        {
            if( toDestroy )
                Destroy( toDestroy );
            else
                Destroy(gameObject);
        }
    }
}
