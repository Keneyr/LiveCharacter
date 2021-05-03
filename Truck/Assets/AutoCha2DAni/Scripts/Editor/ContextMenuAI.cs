using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ContextMenuAI
{
    [MenuItem("Assets/Create/AIDetectBone")]
    static void AIDetectBone(MenuCommand menuCommand)
    {
        Texture2D sprite = Selection.activeObject as Texture2D;
        if(sprite)
        {
            AIDetectBoneUtils.AIDetectBone(sprite);
        }
        
    }
}
