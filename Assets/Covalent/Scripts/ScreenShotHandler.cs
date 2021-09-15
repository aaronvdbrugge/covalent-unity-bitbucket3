using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotHandler : MonoBehaviour
{
    private static ScreenShotHandler Instance;

    private Camera myCamera;
    private bool takeScreenShotOnNextFrame;

    private void Awake()
    {
        Instance = this;
        myCamera = gameObject.GetComponent<Camera>();
    }

    private IEnumerator WaitForEffects()
    {
        yield return new WaitForEndOfFrame();

        if (takeScreenShotOnNextFrame)
        {
            takeScreenShotOnNextFrame = false;
            RenderTexture renderTexture = myCamera.targetTexture;

            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            byte[] byteArray = renderResult.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + "/CameraScreenshot.png", byteArray);
            Debug.Log("SAVED SCREENSHOT");

            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }

    private void TakeScreenshot(int width, int height)
    {
        myCamera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        takeScreenShotOnNextFrame = true;
        StartCoroutine(WaitForEffects());
    }

    public static void TakeScreenShot_Static(int width, int height)
    {
        Instance.TakeScreenshot(width, height);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeScreenshot(1125, 2436);
        }
    }
}
