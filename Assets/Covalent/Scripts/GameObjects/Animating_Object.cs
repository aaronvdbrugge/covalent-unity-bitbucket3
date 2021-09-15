using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class used to animate an object with a single animation
//Use overrideRandomTime boolean to toggle whether a random time should be waited
//or if the requested time should be waited

public class Animating_Object : MonoBehaviour
{
    Animator anim;
    public bool overrideRandomTime;
    public bool triggerAnimate = false;

    private bool animateDownTime = false;
    public float timeToWait;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        if(!triggerAnimate)
        {
            if (overrideRandomTime)
            {
                InvokeRepeating("animateObj", timeToWait, timeToWait);
            }
            else if (timeToWait > 0)
            {
                int randomRange = Random.Range(7, 14);
                InvokeRepeating("animateObj", randomRange, randomRange);
            }
        }
    }

    public void animateObj()
    {
        anim.Play("animation", -1, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (animateDownTime == false)
        {
            animateDownTime = true;
            animateObj();
            StartCoroutine(resetDownTime());
        }
        
    }
    public IEnumerator resetDownTime()
    {
        yield return new WaitForSeconds(timeToWait);
        animateDownTime = false;
    }

}
