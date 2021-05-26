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

    static RawImage rawImageBG;
    static RawImage rawImageSK;

    static RectTransform rt;

    static List<Bone2D> Bones = new List<Bone2D>();

    static Material skeletonMaterial;
    static Material capMaterial;
    static float layer = 2;
    private void Start()
    {
        CreateSkeletonMaterial();
        poseTexture = new Texture2D(100,100);
        renderTargetTexture = new RenderTexture(256, 256, 24);

        SkeletonCamera = GameObject.Find("SkeletonCamera").GetComponent<Camera>();
        Extra.InitCamera(SkeletonCamera, layer);
        SkeletonCamera.backgroundColor = Color.clear;
        SkeletonCamera.targetTexture = renderTargetTexture;

        rawImageBG = GetComponent<RawImage>();
        rawImageSK = transform.parent.Find("Skeleton").GetComponent<RawImage>();
        rawImageSK.texture = renderTargetTexture;

        rt = GetComponent<RectTransform>();
    }
    void CreateSkeletonMaterial()
    {
        if(!skeletonMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            skeletonMaterial = new Material(shader);
            skeletonMaterial.hideFlags = HideFlags.HideAndDontSave;

            //cyan color
            skeletonMaterial.SetColor("_Color", Color.white);

            // Turn on alpha blending
            skeletonMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            skeletonMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            skeletonMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            skeletonMaterial.SetInt("_ZWrite", 0);
        }

        if(!capMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            capMaterial = new Material(shader);
            capMaterial.hideFlags = HideFlags.HideAndDontSave;

            //cyan color
            capMaterial.SetColor("_Color", Color.black);

            // Turn on alpha blending
            capMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            capMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            capMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            capMaterial.SetInt("_ZWrite", 0);
        }
    }
    public static void ResetPastCharacterInfo()
    {
        renderImagePath = null;
        Bones.Clear();

    }

    public void OnRenderObject()
    {
        DrawSkeletonBone2D();
        
    }
    public static void DrawPoseImageByOpenPose()
    {
        if (renderImagePath == null)
            return;
        //读取本地渲染好的Pose图像，显示
        poseTexture.LoadImage(Extra.GetImageByte(renderImagePath));
        rawImageBG.texture = poseTexture;
        Extra.AutoFill(poseTexture, rt);
    }
    
    public static void DrawSkeletonBone2D()
    {
        if (Bones.Count == 0)
            return;
        for(int i = Bones.Count-1; i >=0 ;i--)
        {
            Bone2D bone = Bones[i];
            if(bone)
            {
                Extra.DrawBoneBody(bone, Color.white,skeletonMaterial, layer) ;

                //Color innerColor = bone.color * 0.25f;
                //innerColor.a = bone.color.a;
                Extra.DrawBoneCap(bone, capMaterial,layer);
            }
        }
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
    public static void InitSkeletonBone2D(GameObject go,List<Bone2D> bonesInHierarchy)
    {
        if (go == null || bonesInHierarchy.Count <= 0)
            return;
        //判断Bone下是否有子骨骼
        if (go.transform.childCount == 0 || go.transform.childCount != bonesInHierarchy.Count)
            return;
        Bones.Clear();
        Bones = FindComponentsOfType<Bone2D>().ToList(); //全部物体中是Bone2D的


        //set camera
        Extra.SetInnerCamera(SkeletonCamera, layer, Extra.rect, rt);
    } 
    static T[] FindComponentsOfType<T>() where T: Component
    {
        return GameObject.FindObjectsOfType<T>();
    }
}
