using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VideoMotionExtractResult
{
    Success = 0,
    VideoError = 1,
    OpenPoseStart = 2,

}

public class VideoMotionExtract : MonoBehaviour
{
    public void ExtractVideoMotion()
    {
        Console.Log("Extract Video PoseMotion，建议使用初始化UI参数值进行运动提取...");
        VideoMotionExtractResult code = VideoPreprocessing.ExtractVideoMotion();
        switch(code)
        {
            case VideoMotionExtractResult.Success:
                Console.Log("Extract Video PoseMotion Complete.");
                break;
            case VideoMotionExtractResult.VideoError:
                Console.LogError("Video Path Error");
                break;
            case VideoMotionExtractResult.OpenPoseStart:
                Console.Log("OpenPose start");
                break;
            default:
                Console.LogError("Error:Extract Video PoseMotion end with an Unknown resultCode.");
                break;

        }
    }
}
