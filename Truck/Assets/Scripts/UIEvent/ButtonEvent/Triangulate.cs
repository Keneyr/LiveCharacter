using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriangulateResult
{
    Success = 0,
    ContourError = 1,
    SpriteError = 2
}

public class Triangulate : MonoBehaviour
{
    public void TriangulateContour()
    {
        Console.Log("Triangulating...");
        TriangulateResult code = CharacterPreprocessing.Triangulation();
        switch(code)
        {
            case TriangulateResult.Success:
                Console.Log("Triangulate Complete.");
                break;
            case TriangulateResult.ContourError:
                Console.LogError("Triangulate Contour Error.");
                break;
            case TriangulateResult.SpriteError:
                Console.LogError("Triangulate Sprite Error.");
                break;
            default:
                Console.LogError("Error:Triangulate detect end with an Unknown resultCode");
                break;
        }

    }
}
