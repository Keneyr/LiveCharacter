using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContourDetect : MonoBehaviour
{
    public void DetectContour()
    {
        Console.Log("DetectContour...");
        CharacterPreprocessing.DetectContour();
    }
}
