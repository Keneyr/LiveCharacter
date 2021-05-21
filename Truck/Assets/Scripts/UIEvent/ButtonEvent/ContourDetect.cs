using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ContourDetectResult {
    Success      = 0,
    TextureError = 1,
    SpriteError = 2
}

public class ContourDetect : MonoBehaviour
{
    public void DetectContour()
    {
        Console.Log("DetectContour...");
        ContourDetectResult code = CharacterPreprocessing.DetectContour();
        switch (code) {
            case ContourDetectResult.Success:
                Console.Log("DetectContour Complete.");
                break;
            case ContourDetectResult.TextureError:
                Console.LogWarning("DetectContour Failed: Texture is null, Try import texture first?");
                break;
            case ContourDetectResult.SpriteError:
                Console.LogError("DetectContour Failed: Sprite is null.");
                break;
            default:
                Console.LogError("Error:Contour detect end with an Unknown resultCode.");
                break;
        }
    }
}
