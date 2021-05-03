using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using OpenPose;

/// <summary>
/// 负责启动OpenPose接口，获取角色骨骼关键点.
/// </summary>
public class AIDetectBoneUtils
{   
    public static void AIDetectBone(Texture2D sprite)
    {
        string spritePath = AssetDatabase.GetAssetPath(sprite); //absolute paths
        if(!string.IsNullOrEmpty(spritePath) && File.Exists(spritePath))
        {
            //Debug.Log(Application.dataPath);
            //spritePath = Path.GetDirectoryName(spritePath);
            spritePath = Application.dataPath.Substring(0,Application.dataPath.LastIndexOf("Assets")) + spritePath;
            //spritePath = spritePath.Substring(0, spritePath.Length-6); //ce.png
            spritePath = Path.GetDirectoryName(spritePath); //去掉文件名的绝对路径
            spritePath = spritePath.Replace(@"/", @"\");
            //调用OpenPose接口
            //AutoChaAniUserScript autoChaAniUserScript = new AutoChaAniUserScript();
            //autoChaAniUserScript.inputType = OpenPose.ProducerType.ImageDirectory;
            Debug.Log("角色图片路径名：" + spritePath);
            AutoChaAniUserScript.folderSaveKeyPoints = spritePath + "\\keypoints";
            AutoChaAniUserScript.folderSaveImage = spritePath + "\\keypoints";
            AutoChaAniUserScript.producerString = spritePath;
            PoseRetargetUserScript.ApplyBonesToScript();
            //autoChaAniUserScript.CusStart(true);
        }
    }
}
