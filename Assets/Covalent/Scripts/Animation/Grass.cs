using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple class that plays an animation when something enters its collider.
//For now its just Grass and maybe in the future this script should be a parent class
//WalkThroughTriggerObject

public class Grass : MonoBehaviour
{
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("animation"))
        {
            anim.Play("animation", -1, 0f);
        }
        
    }
}
