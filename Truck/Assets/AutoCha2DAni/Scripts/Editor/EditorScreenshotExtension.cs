using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public static class EditorScreenshotExtension
{
    [MenuItem("Screenshot/Take Screenshot %#k")]
    private static void Screenshot()
    {
        // Get actvive EditorWindow
        var activeWindow = EditorWindow.focusedWindow;

        // Get screen position and sizes
        var vec2Position = activeWindow.position.position;
        var sizeX = activeWindow.position.width;
        var sizeY = activeWindow.position.height;

        // Take Screenshot at given position sizes
        var colors = InternalEditorUtility.ReadScreenPixel(vec2Position, (int)sizeX, (int)sizeY);

        // write result Color[] data into a temporal Texture2D
        var result = new Texture2D((int)sizeX, (int)sizeY);
        result.SetPixels(colors);

        // encode the Texture2D to a PNG
        // you might want to change this to JPG for way less file size but slightly worse quality
        // if you do don't forget to also change the file extension below
        var bytes = result.EncodeToPNG();

        // In order to avoid bloading Texture2D into memory destroy it
        Object.DestroyImmediate(result);

        // finally write the file e.g. to the StreamingAssets folder
        var timestamp = System.DateTime.Now;
        var stampString = string.Format("_{0}-{1:00}-{2:00}_{3:00}-{4:00}-{5:00}", timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, timestamp.Second);
        File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, "Screenshot" + stampString + ".png"), bytes);

        // Refresh the AssetsDatabase so the file actually appears in Unity
        AssetDatabase.Refresh();

        Debug.Log("New Screenshot taken");
    }
}