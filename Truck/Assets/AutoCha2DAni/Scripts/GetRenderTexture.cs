
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Camera)),ExecuteInEditMode]
public class GetRenderTexture : MonoBehaviour
{
	IEnumerator captureScreenshot()
	{

        ScreenCapture.CaptureScreenshot("fuckunity.png", 1);
        yield return null;
  //      print("2");
		//yield return new WaitForEndOfFrame();
  //      print("3");
		//string path = "C:/" + Screen.width + "X" + Screen.height + "hh" + ".png";
		//Texture2D screenImage = new Texture2D(Screen.width, Screen.height);
		////Get Image from screen
		//screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		//screenImage.Apply();
		////Convert to png
		//byte[] imageBytes = screenImage.EncodeToPNG();
		////Save image to file
		//System.IO.File.WriteAllBytes(path, imageBytes);
		//Debug.Log("shot:" + path);
		DestroyImmediate(this);
	}
	void Awake() {
        StartCoroutine(captureScreenshot());
        //string path = "C:/" + Screen.width + "X" + Screen.height + "hh" + ".png";
        //Texture2D screenImage = new Texture2D(Screen.width, Screen.height);
        ////Get Image from screen
        //screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        //screenImage.Apply();
        ////Convert to png
        //byte[] imageBytes = screenImage.EncodeToPNG();
        ////Save image to file
        //System.IO.File.WriteAllBytes(path, imageBytes);
        //Debug.Log("shot:" + path);
        //DestroyImmediate(this);
    }
}
