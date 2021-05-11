using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContourDetect : MonoBehaviour
{
    public void DetectContour()
    {
        ConsoleController.instance.ShowMessage("DetectContour...");
        CharacterPreprocessing.DetectContour();
    }
}
