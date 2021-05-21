using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VideoMotionRetrivalResult
{
    Success = 0,
    jsonError = 1,

}

public class VideoMotionRetrival : MonoBehaviour
{
    public void RetrivalVideoMotion()
    {
        Console.Log("Retrieve VideoMotion...");
        VideoMotionRetrivalResult code = VideoPreprocessing.RetrivalVideoMotion();
        switch (code)
        {
            case VideoMotionRetrivalResult.Success:
                Console.Log("Retrieve VideoMotion Complete.");
                break;
            case VideoMotionRetrivalResult.jsonError:
                Console.LogError("Retrieve VideoMotion jsonError");
                break;
            default:
                Console.LogError("Error:Retrieve VideoMotion end with an Unknown resultCode.");
                break;

        }
    }
}
