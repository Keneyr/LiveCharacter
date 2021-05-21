using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoneSkinningResult
{
    Success = 0,
    BoneError = 1,
    MeshError = 2,
}

public class BoneBinding : MonoBehaviour
{
    public void BindSkeleton()
    {
        Console.Log("Character Bone Skinning... ");
        BoneSkinningResult code = CharacterPreprocessing.BoneSkinning();
        switch(code)
        {
            case BoneSkinningResult.Success:
                Console.Log("BoneSkinning Complete");
                break;
            default:
                Console.LogError("Error:BoneSkinning end with an Unknown resultCode");
                break;
        }
    }
}
