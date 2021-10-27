
using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    public Transform target;
    public bool disable_follow;
    public float smoothSpeed = 0.1f;
    public float offset_x;
    public bool follow_x = false;
    public float offset_y;
    public bool follow_y = false;

    public float clamp_x_left, clamp_x_right, clamp_y_down, clamp_y_up;


    protected virtual void Start()
    {
        //9:19.5 = 0.46153846
        //9:16 = 0.5625
        //3:4 = 0.75
        EventManager.StartListening("stop_follow", Disable_Follow);
        EventManager.StartListening("start_follow", Enable_Follow);
        //This part needs some work most likely. Perhaps inst
        //ead of using aspect ratio, we should make decisions
        //based on the device generation (iPhoneX, iPhoneXR, iPhone12, etc.)

        //Set clamps based on aspect ratio
        Camera cameraMain = Camera.main;
        disable_follow = false;
        Debug.Log(cameraMain.aspect);
        if (cameraMain.aspect >= 0.75)
        {
            clamp_x_left = -50f;
            clamp_x_right = 48f;
            clamp_y_down = -22.5f;
            clamp_y_up = 22.5f;
        }
        else if (cameraMain.aspect >= 0.5625)
        {
            clamp_x_left = -34.5f;
            clamp_x_right = 34.5f;
            clamp_y_down = -25;
            clamp_y_up = 25f;
        }
        else if (cameraMain.aspect >= 0.46153846)
        {
            clamp_x_left = -36.4f;
            clamp_x_right = 36.4f;
            clamp_y_down = -22.2f;
            clamp_y_up = 22.2f;
        }
        else
        {
            clamp_x_left = -36.4f;
            clamp_x_right = 36.4f;
            clamp_y_down = -22.2f;
            clamp_y_up = 22.2f;
        }
        
    }
    public void Disable_Follow()
    {
        disable_follow = true;
    }
    public void Enable_Follow()
    {
        disable_follow = false;
    }
    protected virtual void Update()
    {
        // This script isn't working well, just follow the target for now.
        if( target != null )
            transform.position = new Vector3(target.position.x, target.position.y, -10 );

        /*if (target != null)
        {
            Vector3 desiredPosition = target.position + new Vector3(0, 0, -10);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed*Time.deltaTime);
            transform.position = smoothedPosition;
        }
        */
/*
        if (disable_follow == false)
        {
            //If following target then update position and clamp
            if (target != null && follow_x)
            {
                Vector3 desiredPosition = new Vector3(target.position.x + offset_x, transform.position.y, -10);
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, clamp_x_left, clamp_x_right);
                transform.position = smoothedPosition;
            }
            if (target != null && follow_y)
            {
                Vector3 desiredPosition = new Vector3(transform.position.x, target.position.y + offset_y, -10);
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, clamp_y_down, clamp_y_up);
                transform.position = smoothedPosition;
            }
        }
        */
        
    }
}
