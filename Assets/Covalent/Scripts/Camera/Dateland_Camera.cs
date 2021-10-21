using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * This class is called Dateland Camera though it probably should be called DatelandView.
 * It handles moving the camera to the player since it inherts from Camera Follow.
 * It also handles various UI elements that need to be displayed to the player (Settings, Character_Creator)
 * 
 */


public class Dateland_Camera : MonoBehaviour  // : Camera_Follow
{
    Camera cameraMain;
    public int cameraSize;
    public CanvasGroup Settings, Character_Creator, Player_UI, Controls;
    public Image[] Zoom_Level_Buttons;
    public Sprite[] Zoom_Level_Sprites;

    public Image[] Graphics_Level_Buttons;
    public Sprite[] Graphics_Level_Sprites;
    // Start is called before the first frame update
    void Start()
    {
        //9:19.5 = 0.46153846
        //9:16 = 0.5625
        //3:4 = 0.75

        //This part needs some work most likely. Perhaps instead of using aspect ratio, we should make decisions
        //based on the device generation (iPhoneX, iPhoneXR, iPhone12, etc.)

        //Set clamps based on aspect ratio
        cameraMain = Camera.main;
        Debug.Log(cameraMain.aspect);
        setClamps();
    }

    public void Disable_Controls()
    {
        Controls.interactable = false;
        Controls.alpha = 0;
        Controls.blocksRaycasts = false;

        Player_UI.interactable = false;
        Player_UI.alpha = 0;
        Player_UI.blocksRaycasts = false;
    }

    public void Enable_Controls()
    {
        Controls.interactable = true;
        Controls.alpha = 1;
        Controls.blocksRaycasts = true;

        Player_UI.interactable = true;
        Player_UI.alpha = 1;
        Player_UI.blocksRaycasts = true;
    }

    public void showSettings()
    {
        Settings.interactable = true;
        Settings.alpha = 1;
        Settings.blocksRaycasts = true;
        Disable_Controls();
        EventManager.TriggerEvent("disable_joystick");
    }

    public void showCharacter_Creator()
    {
        Character_Creator.interactable = true;
        Character_Creator.alpha = 1;
        Character_Creator.blocksRaycasts = true;
        Disable_Controls();
        Camera.main.GetComponent<Camera_Follow>().Disable_Follow();
        EventManager.TriggerEvent("init_char_creator");
        EventManager.TriggerEvent("disable_joystick");
    }

    public void zoomOut()
    {
        cameraSize = 22;
        Zoom_Level_Buttons[0].sprite = Zoom_Level_Sprites[0];
        Zoom_Level_Buttons[1].sprite = Zoom_Level_Sprites[1];
        Zoom_Level_Buttons[2].sprite = Zoom_Level_Sprites[5];
    }
    public void zoomIn()
    {
        cameraSize = 10;
        Zoom_Level_Buttons[0].sprite = Zoom_Level_Sprites[3];
        Zoom_Level_Buttons[1].sprite = Zoom_Level_Sprites[1];
        Zoom_Level_Buttons[2].sprite = Zoom_Level_Sprites[2];
    }
    public void zoomNormal()
    {
        cameraSize = 16;
        Zoom_Level_Buttons[0].sprite = Zoom_Level_Sprites[0];
        Zoom_Level_Buttons[1].sprite = Zoom_Level_Sprites[4];
        Zoom_Level_Buttons[2].sprite = Zoom_Level_Sprites[2];
    }

    public void setBestGraphics()
    {
        Graphics_Level_Buttons[0].sprite = Graphics_Level_Sprites[0];
        Graphics_Level_Buttons[1].sprite = Graphics_Level_Sprites[3];
    }

    public void setLowGraphics()
    {
        Graphics_Level_Buttons[0].sprite = Graphics_Level_Sprites[2];
        Graphics_Level_Buttons[1].sprite = Graphics_Level_Sprites[1];
    }

    public void Save_Character_Creator()
    {
        Character_Creator.interactable = false;
        Character_Creator.alpha = 0;
        Character_Creator.blocksRaycasts = false;
        Enable_Controls();
        Camera.main.GetComponent<Camera_Follow>().Enable_Follow();
        EventManager.TriggerEvent("enable_joystick");
        EventManager.TriggerEvent("end_char_creator");
    }

    public void Cancel_Character_Creator()
    {
        Character_Creator.interactable = false;
        Character_Creator.alpha = 0;
        Character_Creator.blocksRaycasts = false;
        Enable_Controls();
        Camera.main.GetComponent<Camera_Follow>().Enable_Follow();
        EventManager.TriggerEvent("enable_joystick");
        EventManager.TriggerEvent("end_char_creator");
    }

    public void Save_Settings()
    {

        // Something's wonky here.
        // "Screen position out of view frustum" exception.
        // Not sure how much of it we're keeping anyway, so I'm just disabling it for now
        Cancel_Settings();

        #if false
        Debug.Log("Pressed Save");
        cameraMain.orthographicSize = cameraSize;
        setClamps();
        Vector3 smooth = transform.position;
        smooth.x = Mathf.Clamp(smooth.x, clamp_x_left, clamp_x_right);
        smooth.y = Mathf.Clamp(smooth.y, clamp_y_down, clamp_y_up);
        transform.position = smooth;
        Settings.interactable = false;
        Settings.alpha = 0;
        Settings.blocksRaycasts = false;
        Enable_Controls();
        EventManager.TriggerEvent("enable_joystick");
        #endif
    }
    public void Cancel_Settings()
    {
        Debug.Log("Pressed Cancel");
        Settings.interactable = false;
        Settings.alpha = 0;
        Settings.blocksRaycasts = false;
        Enable_Controls();
        EventManager.TriggerEvent("enable_joystick");
    }

    

    public void setClamps()
    {
        // This originally set Camera_Follow values.
        // The script wasn't working great anyway (had a kind of laggy unsatisfying camera follow logic) so I'm just disabling this stuff.
        // Any further work should probably be done in the new CameraPanning script.
        /*

        if (cameraMain.orthographicSize == 10)
        {
            if (cameraMain.aspect >= 0.75)
            {
                clamp_x_left = -50f;
                clamp_x_right = 48f;
                clamp_y_down = -32f;
                clamp_y_up = 22.5f;
                offset_x = 5;
                offset_y = 3.5f;
            }
            else if (cameraMain.aspect >= 0.5625)
            {
                clamp_x_left = -53f;
                clamp_x_right = 52f;
                clamp_y_down = -33f;
                clamp_y_up = 22.55f;
                offset_x = 4;
                offset_y = 3.5f;
            }
            else if (cameraMain.aspect >= 0.46153846)
            {
                clamp_x_left = -54.45f;
                clamp_x_right = 54.45f;
                clamp_y_down = -32f;
                clamp_y_up = 22.2f;
                offset_x = 3f;
                offset_y = 3.5f;
            }
            else
            {
                clamp_x_left = -54.45f;
                clamp_x_right = 54.45f;
                clamp_y_down = -32f;
                clamp_y_up = 22.2f;
                offset_x = 3f;
                offset_y = 3.5f;
            }
        }
        else if (cameraMain.orthographicSize == 16)
        {
            if (cameraMain.aspect >= 0.75)
            {
                clamp_x_left = -50f;
                clamp_x_right = 48f;
                clamp_y_down = -32f;
                clamp_y_up = 22.5f;
                offset_x = 8;
                offset_y = 5;
            }
            else if (cameraMain.aspect >= 0.5625)
            {
                clamp_x_left = -53f;
                clamp_x_right = 52f;
                clamp_y_down = -33f;
                clamp_y_up = 22.55f;
                offset_x = 6;
                offset_y = 5.5f;
            }
            else if (cameraMain.aspect >= 0.46153846)
            {
                clamp_x_left = -54.45f;
                clamp_x_right = 54.45f;
                clamp_y_down = -32f;
                clamp_y_up = 22.2f;
                offset_x = 5.1f;
                offset_y = 5.5f;
            }
            else
            {
                clamp_x_left = -54.45f;
                clamp_x_right = 54.45f;
                clamp_y_down = -32f;
                clamp_y_up = 22.2f;
                offset_x = 5.1f;
                offset_y = 5.5f;
            }
        }
        else
        {
            if (cameraMain.aspect >= 0.75)
            {
                clamp_x_left = -45.5f;
                clamp_x_right = 45f;
                clamp_y_down = -27f;
                clamp_y_up = 16.5f;
                offset_x = 11;
                offset_y = 7.5f;
            }
            else if (cameraMain.aspect >= 0.5625)
            {
                clamp_x_left = -49.5f;
                clamp_x_right = 49f;
                clamp_y_down = -27f;
                clamp_y_up = 16.5f;
                offset_x = 8;
                offset_y = 7.5f;
            }
            else if (cameraMain.aspect >= 0.46153846)
            {
                clamp_x_left = -51.7f;
                clamp_x_right = 51.7f;
                clamp_y_down = -27f;
                clamp_y_up = 16.5f;
                offset_x = 7.1f;
                offset_y = 7.3f;

            }
            else
            {
                clamp_x_left = -51.7f;
                clamp_x_right = 51.7f;
                clamp_y_down = -27f;
                clamp_y_up = 16.5f;
                offset_x = 7.1f;
                offset_y = 7.3f;
            }
        }
        */
    }
}
