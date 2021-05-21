using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class DrawSkeleton : MonoBehaviour
{

    static Camera SkeletonCamera;

    RenderTexture renderTargetTexture;

    static string renderImagePath = null;

    static Texture2D poseTexture;

    static RawImage rawImage;

    static RectTransform rt;

    static List<Anima2D.Bone2D> Bones = new List<Anima2D.Bone2D>();

    private void Start()
    {
        poseTexture = new Texture2D(100,100);
        renderTargetTexture = new RenderTexture(256, 256, 24);

        SkeletonCamera = GameObject.Find("SkeletonCamera").GetComponent<Camera>();
        SkeletonCamera.targetTexture = renderTargetTexture;

        rawImage = GetComponent<RawImage>();
        rawImage.texture = renderTargetTexture;

        rt = GetComponent<RectTransform>();
    }
    public static void ResetPastCharacterInfo()
    {
        renderImagePath = null;
        Bones.Clear();
    }

    public void OnRenderObject()
    {
        //DrawPoseImageByOpenPose();
        DrawSkeletonBone2D();
        
    }
    public static void DrawPoseImageByOpenPose()
    {
        if (renderImagePath == null)
            return;
        //读取本地渲染好的Pose图像，显示
        poseTexture.LoadImage(Extra.GetImageByte(renderImagePath));
        rawImage.texture = poseTexture;
        Extra.AutoFill(poseTexture, rt);
    }
    
    public static void DrawSkeletonBone2D()
    {
        if (Bones.Count == 0)
            return;
        for(int i=0;i < Bones.Count;i++)
        {
            Anima2D.Bone2D bone = Bones[i];
            if(bone)
            {
                Extra.DrawBoneBody(bone);
                Color innerColor = bone.color * 0.25f;

                if (bone.attachedIK && bone.attachedIK.isActiveAndEnabled)
                {
                    innerColor = new Color(0f, 0.75f, 0.75f, 1f);
                }

                innerColor.a = bone.color.a;
                Extra.DrawBoneCap(bone, innerColor);
            }
        }
        //set camera
        Extra.SetInnerCamera(SkeletonCamera, poseTexture.width/100, poseTexture.height/100, rt);

        //reset
        //Bones.Clear();
    }
    public static void InitRenderImage(string _renderImagePath)
    {
        renderImagePath = null;
        if(!string.IsNullOrEmpty(_renderImagePath) && File.Exists(_renderImagePath))
        {
            renderImagePath = _renderImagePath;
        }
        DrawPoseImageByOpenPose();

    }
    public static void InitSkeletonBone2D(GameObject go)
    {
        if (go == null)
            return;
        //判断Bone下是否有子骨骼
        if (go.transform.childCount == 0)
            return;
        Bones.Clear();
        Bones = FindComponentsOfType<Anima2D.Bone2D>().ToList(); //全部物体中是Bone2D的

    } 
    static T[] FindComponentsOfType<T>() where T: Component
    {
        return GameObject.FindObjectsOfType<T>();
    }
}
