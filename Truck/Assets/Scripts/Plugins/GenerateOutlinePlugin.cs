using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class GenerateOutlinePlugin
{
    [DllImport("Anima2D")]
    public static extern void GenerateOutline(
        Texture2D texture,Rect rect,float detail,
        byte alphaTolerance,bool holeDetection,Vector2[][] paths
        );
}
