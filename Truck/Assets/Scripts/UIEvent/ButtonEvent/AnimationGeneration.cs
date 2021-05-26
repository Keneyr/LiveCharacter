using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationGenerationResult
{
    Success = 0,
    SpriteMeshDataError = 1,
    SpriteMeshInstanceError = 2,
    VideoDataError = 3,
}

public class AnimationGeneration : MonoBehaviour
{
    public void GenerateCharacterAnimation()
    {
        Console.Log("Character Bone Skinning... ");
        AnimationGenerationResult code = CharacterAnimationGeneration.GenerateAnimation();
        switch (code)
        {
            case AnimationGenerationResult.Success:
                Console.Log("Animation Generation Complete");
                break;
            case AnimationGenerationResult.SpriteMeshDataError:
                Console.LogError("Animation Generation SpriteMeshData Error");
                break;
            case AnimationGenerationResult.SpriteMeshInstanceError:
                Console.LogError("Animation Generation SpriteMeshInstance Error");
                break;
            case AnimationGenerationResult.VideoDataError:
                Console.LogError("Animation Generation VideoData Error");
                break;
            default:
                Console.LogError("Error:BoneSkinning end with an Unknown resultCode");
                break;
        }

    }
}
