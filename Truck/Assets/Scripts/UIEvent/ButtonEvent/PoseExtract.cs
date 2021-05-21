using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PoseExtractResult
{
    OpenPoseStart = 0,
    OpenPoseError = 1,
    ImageError = 2,
    Success = 3,
}

public class PoseExtract : MonoBehaviour
{
    public void ExtractPoseByOpenPose()
    {
        PoseExtractResult code = CharacterPreprocessing.ExtractPose();
        switch(code)
        {
            case PoseExtractResult.OpenPoseStart:
                Console.Log("PoseExtract Starting...");
                break;
            case PoseExtractResult.OpenPoseError:
                Console.LogError("PoseExtract OpenPose Error");
                break;
            case PoseExtractResult.ImageError:
                Console.LogError("PoseExtract Image Error");
                break;
            case PoseExtractResult.Success:
                Console.Log("PoseExtract Complete");
                break;
            default:
                Console.LogError("Error:PoseExtract end with an Unknown resultCode");
                break;
        }
    }
}
