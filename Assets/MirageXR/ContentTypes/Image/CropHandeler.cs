using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class CropHandeler : MonoBehaviour
{
    private static CropHandeler instance;

    private Camera myCamera;
    private bool takeScreenShotOnNextFrame;
    private static string screenShotName;
    private byte[] byteArray;
    private Texture2D RR;


    private void Awake()
    {
        instance = this;
        myCamera = gameObject.GetComponent<Camera>();
    }

    private void OnPostRender()
    {
        // take screenshot on the next fram in order to capture UI elements
        if (takeScreenShotOnNextFrame)
        {
            Debug.Log("taking screenshot next frame");

            takeScreenShotOnNextFrame = false;
            RenderTexture renderTexture = myCamera.targetTexture;

            // creates a texture from the camera view
            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);
            renderResult.Apply();

            RR = renderResult;

            // converts the texture into a JPG
            byteArray = renderResult.EncodeToJPG();

            Console.Out.WriteLine(byteArray.Length);

            // saves the JPG to the given file path
            System.IO.File.WriteAllBytes(screenShotName, byteArray);

            RenderTexture.ReleaseTemporary(renderTexture);
            myCamera.targetTexture = null;
        }


    }

    public void TakeScreenShot(int width, int height, string filePath)
    {
        screenShotName = filePath;
        myCamera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        takeScreenShotOnNextFrame = true;
    }

    public static void TakeScreenshot_static(int width, int height, string filePath)
    {
        instance.TakeScreenShot(width, height, filePath);
    }

    public Texture2D GetCropped()
    {
        return RR;
    }
}
