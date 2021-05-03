using UnityEngine;
using UnityEditor;
using System.IO;
using System;


public class CaptureScreen : MonoBehaviour {
    [UnityEditor.MenuItem("Tools/CaptureScreen")]
    static void Capture()
    {
        //print("1");
        Camera.main.gameObject.AddComponent<GetRenderTexture>();
    }

}
