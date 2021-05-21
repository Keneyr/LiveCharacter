using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoneGenerateResult
{
    Success = 0,
    Bone2DError = 1,
    jsonError = 2
}

public class BoneGenerate : MonoBehaviour
{
    public void GenerateSkeleton()
    {
        Console.Log("Generate Character Skeleton...");
        BoneGenerateResult code = CharacterPreprocessing.GenerateSkeleton();
        switch (code)
        {
            case BoneGenerateResult.Success:
                Console.Log("BoneGeneration Complete");
                break;
            case BoneGenerateResult.Bone2DError:
                Console.LogError("Bone2D Error");
                break;
            case BoneGenerateResult.jsonError:
                Console.LogError("PoseExtract json Error");
                break;
            default:
                Console.LogError("Error:PoseExtract end with an Unknown resultCode");
                break;
        }
    }
}
