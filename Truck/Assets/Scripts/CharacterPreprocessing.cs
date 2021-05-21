using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.IO;
using System;
using System.Text;
/// <summary>
/// 对角色图像(png/jpg/psd)预处理合集，包括轮廓检测，姿态提取，三角剖分，骨骼生成，自动蒙皮，一键预处理
/// </summary>
public static class CharacterPreprocessing
{
    static Sprite sprite;
    public static SpriteMeshData spriteMeshData = null;
    static Texture2D texture;

    static Rect rect;
    static float detail;
    static float alphaTolerance;
    static bool holeDetection;
    static Vector2[][] paths;

    static OpenPose.MultiArray<float> keypoints = new OpenPose.MultiArray<float>();
    static string txname = null;

    //static GameObject[] Skeleton = new GameObject[13];

    public static GameObject fbone = new GameObject("Bone");
    public static void ResetPastCharacterInfo()
    {
        DrawEdge.ResetPastCharacterInfo();
        DrawSkeleton.ResetPastCharacterInfo();
        DrawTriangles.ResetPastCharacterInfo();
        DrawSkinning.ResetPastCharacterInfo();
    }
    static void Texture2Sprite()
    {
        Texture2D tx = CharacterManager.instance.tx;
        //sprite = tx as Object as Sprite;
        sprite = Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), Vector2.zero);
    }
    static void SaveAssetsFile()
    {
        string assetPath = "/" + spriteMeshData.name + ".asset";
        AssetDataBase.SaveAsset<SpriteMeshData>(assetPath, spriteMeshData);
    }
    public static ContourDetectResult DetectContour()
    {
        if (CharacterManager.instance.tx == null)
            return ContourDetectResult.TextureError;

        Texture2Sprite();

        if (sprite)
        {
            //spriteMeshData = new SpriteMeshData();
            spriteMeshData = ScriptableObject.CreateInstance<SpriteMeshData>();

            Vector2[] vertices;
            IndexedEdge[] edges;
            GetSpriteContourData(sprite, out vertices, out edges);
            txname = System.IO.Path.GetFileNameWithoutExtension(CharacterManager.instance.tx.name);
            spriteMeshData.name = txname + "_Data";
            spriteMeshData.vertices = vertices;
            spriteMeshData.edges = edges;

            SaveAssetsFile();

            DrawEdge.InitEdges(spriteMeshData);
            return ContourDetectResult.Success;
        }
        return ContourDetectResult.SpriteError;
    }
    static void GetSpriteContourData(Sprite sprite, out Vector2[] vertices, out IndexedEdge[] edges)
    {
        int width = 0;
        int height = 0;

        GetSpriteTextureSize(sprite, ref width, ref height);

        //Vector2[] uvs = SpriteUtility.GetSpriteUVs(sprite, false);
        Vector2[] uvs = sprite.uv;
        vertices = new Vector2[uvs.Length];

        for (int i = 0; i < uvs.Length; ++i)
        {
            vertices[i] = new Vector2(uvs[i].x * width, uvs[i].y * height);
        }
        ushort[] l_indices = sprite.triangles;
        int[] indices = new int[l_indices.Length];
        for(int i = 0; i < l_indices.Length; ++i)
        {
            indices[i] = (int)l_indices[i];
        }
        HashSet<IndexedEdge> edgesSet = new HashSet<IndexedEdge>();
        for (int i = 0; i < indices.Length; i += 3)
        {
            int index1 = indices[i];
            int index2 = indices[i + 1];
            int index3 = indices[i + 2];

            IndexedEdge edge1 = new IndexedEdge(index1, index2);
            IndexedEdge edge2 = new IndexedEdge(index2, index3);
            IndexedEdge edge3 = new IndexedEdge(index1, index3);

            if (edgesSet.Contains(edge1))
            {
                edgesSet.Remove(edge1);
            }
            else
            {
                edgesSet.Add(edge1);
            }

            if (edgesSet.Contains(edge2))
            {
                edgesSet.Remove(edge2);
            }
            else
            {
                edgesSet.Add(edge2);
            }

            if (edgesSet.Contains(edge3))
            {
                edgesSet.Remove(edge3);
            }
            else
            {
                edgesSet.Add(edge3);
            }
        }
        edges = new IndexedEdge[edgesSet.Count];
        int edgeIndex = 0;
        foreach(IndexedEdge edge in edgesSet)
        {
            edges[edgeIndex] = edge;
            ++edgeIndex;
        }
    }
    static void GetSpriteTextureSize(Sprite sprite, ref int width, ref int height)
    {
        if (sprite)
        {
            //Texture2D texture = SpriteUtility.GetSpriteTexture(sprite, false);
            Texture2D texture = sprite.texture;
            width = texture.width;
            height = texture.height;
        }
    }

#if false
    private static Vector2[][] GenerateOutline(Sprite sprite)
    {
        MethodInfo methodInfo = typeof(SpriteUtility).GetMethod("GenerateOutline", BindingFlags.Static | BindingFlags.NonPublic);
        if(methodInfo != null)
        {
            object[] parameters = new object[] { texture, rect, detail, alphaTolerance, holeDetection, null };
            methodInfo.Invoke(null, parameters);

            paths = (Vector2[][])parameters[5];
        }
        return paths;
    }
#endif

    public static PoseExtractResult ExtractPose()
    {
        string imagePath = CharacterManager.instance.tx.name;
        //string imagePath = CharacterManager.instance.tx.name.Substring(0, CharacterManager.instance.tx.name.LastIndexOf("/"));
        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
        {
            //判断此图像文件路径下是否已经存在xx.rendered.png图片，如果已经存在，则不再启动OpenPose，直接读取图片
            string renderImagePath = imagePath.Substring(0,imagePath.LastIndexOf("."));
            renderImagePath += "_rendered.png";
            if(!string.IsNullOrEmpty(renderImagePath) && File.Exists(renderImagePath))
            {
                DrawSkeleton.InitRenderImage(renderImagePath);
                return PoseExtractResult.Success;
            }
            imagePath = System.IO.Path.GetDirectoryName(imagePath);
            UserOpenPoseScript.folderSaveKeyPoints = imagePath;
            UserOpenPoseScript.folderSaveImage = imagePath;
            UserOpenPoseScript.producerString = imagePath;

            imagePath = CharacterManager.instance.tx.name.Substring(0, CharacterManager.instance.tx.name.LastIndexOf("."));
            imagePath = imagePath + "_rendered.png";

            DrawSkeleton.InitRenderImage(imagePath);

            return PoseExtractResult.OpenPoseStart;
        }
        return PoseExtractResult.ImageError;
    }
    public static PoseExtractResult ExtractPoseDatum(OpenPose.OPDatum datum)
    {
        txname = System.IO.Path.GetFileNameWithoutExtension(CharacterManager.instance.tx.name);
        if(datum.name.Equals(txname))
        {
            keypoints = datum.poseKeypoints;
        }
        return PoseExtractResult.Success;
    }
    public static TriangulateResult Triangulation()
    {
        if(sprite)
        {
            if(spriteMeshData.vertices.Length > 0 && spriteMeshData.edges.Length > 0)
            {
                int[] indices;
                GetSpriteMeshData(sprite,out indices);
                spriteMeshData.indices = indices;
                SaveAssetsFile();
                DrawTriangles.InitMeshIndices(spriteMeshData);
                return TriangulateResult.Success;
            }
            return TriangulateResult.ContourError;
        }
        return TriangulateResult.SpriteError;
    }
    static void GetSpriteMeshData(Sprite sprite,out int[] indices)
    {
        ushort[] l_indices = sprite.triangles;
        indices = new int[l_indices.Length];
        for (int i = 0; i < l_indices.Length; ++i)
        {
            indices[i] = (int)l_indices[i];
        }
    }
    public static BoneGenerateResult GenerateSkeleton()
    {
        //如果是刚才的姿态提取算法提取到的数据
        if(keypoints.Count > 0)
        {
            ChangeKeypoints2Bone2D(keypoints);
            DrawSkeleton.InitSkeletonBone2D(fbone);
            return BoneGenerateResult.Success;
        }
        //没有启用OpenPose，本地已经有了json文件
        else
        {
            string jsonPath = CharacterManager.instance.tx.name.Substring(0, CharacterManager.instance.tx.name.LastIndexOf("."));
            jsonPath = jsonPath + "_pose.json";
            if(!string.IsNullOrEmpty(jsonPath) && File.Exists(jsonPath))
            {
                //读取本地json文件，赋值到keypoints
                ParseJson(ReadFile(jsonPath));
                ChangeKeypoints2Bone2D(keypoints);
                DrawSkeleton.InitSkeletonBone2D(fbone);
                return BoneGenerateResult.Success;
            }
        }
        return BoneGenerateResult.jsonError;
    }
    static void ParseJson(string jsonData)
    {
        Dictionary<string, System.Object> pose = Ps2D.MiniJSON.Json.Deserialize(jsonData) as Dictionary<string, System.Object>;
        //Dictionary<string, Object> pose0 = Ps2D.MiniJSON.Json.Deserialize(pose["pose_0"]) as Dictionary<string, Object>;
        Dictionary<string, System.Object> pose0 = pose["pose_0"] as Dictionary<string, System.Object>;
        List<System.Object> keypointsOb = pose0["data"] as List<System.Object>; //不知道这样装箱拆箱各种强制转换会不会丢失数据精度
        keypoints = new OpenPose.MultiArray<float>();
        foreach (var keypoint in keypointsOb)
        {
            //keypoints.Add((float)keypoint);
            float fnum = 0;
            float.TryParse(keypoint.ToString(), out fnum);
            keypoints.Add(fnum);
        }
    }
    static string ReadFile(string fileName)
    {
        StringBuilder str = new StringBuilder();
        using (FileStream fs = File.OpenRead(fileName))
        {
            long left = fs.Length;
            int maxLength = 100;//每次读取的最大长度  
            int start = 0;//起始位置  
            int num = 0;//已读取长度  
            while (left > 0)
            {
                byte[] buffer = new byte[maxLength];//缓存读取结果  
                char[] cbuffer = new char[maxLength];
                fs.Position = start;//读取开始的位置  
                num = 0;
                if (left < maxLength)
                {
                    num = fs.Read(buffer, 0, Convert.ToInt32(left));
                }
                else
                {
                    num = fs.Read(buffer, 0, maxLength);
                }
                if (num == 0)
                {
                    break;
                }
                start += num;
                left -= num;
                str = str.Append(Encoding.UTF8.GetString(buffer));
            }
        }
        return str.ToString();
    }
    static void ChangeKeypoints2Bone2D(OpenPose.MultiArray<float> keypoints)
    {
        //18*3=54个点，(x,y,score)的形式
        //foreach (var t in keypoints)
        //{
        //    Console.Log(t);
        //}
        OpenPose.MultiArray<float> keypointsXY = new OpenPose.MultiArray<float>();

        //1.把54个值去掉score，只保留x，y值，得到36个值
        for (int i = 0; i < keypoints.Count; i++)
        {
            if (i % 3 == 0 || i % 3 == 1)
            {
                keypointsXY.Add(keypoints[i]);
            }
        }

        //2.我们只要前14个有用关键点，对应28个值，将值转化为点
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < keypointsXY.Count - 8; i += 2)
        {
            points.Add(new Vector2(keypointsXY[i] / 100f, -keypointsXY[i + 1] / 100f));//(x,y)
        }

        //3.将点两两配对转化为Bone2D，确定Bone2D的两个端点的位置和length以及rotation
        //（根据OpenPose转化的骨骼图和我们想要的骨骼属性结构）
        // 考虑关键点为0的情况--要不先手动删除空bone物体把...
        int rootpointsindex = 1;
        int[] firstlayerpointsindex = new int[5] { 0, 2, 5, 8, 11 };
        int[] ffirstlayerpointsindex = new int[4] { 2, 5, 8, 11 };
        int[] secondlayerpointsindex = new int[4] { 3, 6, 9, 12 };
        //14个关键点，对应13个骨骼
        //其实应该叫锁骨、上胳膊、下胳膊这样，但是你懂就好
        string[] skeletonsName =
        {
            "neck_bone", //0
            " ",  //1
            "rshoulder_bone", //2
            "relbow_bone", //3
            "rwrist_bone", //4
            "lshoulder_bone", //5
            "lelbow_bone", //6
            "lwrist_bone", //7
            "rhip_bone", //8
            "rknee_bone", //9
            "rankle_bone", //10
            "lhip_bone", //11
            "lknee_bone", //12
            "lankle_bone" //13
        };
        fbone.transform.position = Vector3.zero;
        for (int i = 0; i < points.Count; i++)
        {
            //3.0.root根节点到第一层级关键点的骨骼
            if (i == rootpointsindex)
            {
                for (int j = 0; j < firstlayerpointsindex.Length; j++)
                {
                    //转化为骨骼，零散到hierarchy,因为我是根据两个关键点求Bone2D的，所以Bone2D的长度、旋转角都应该求出来更新其值
                    GameObject bone = new GameObject(skeletonsName[firstlayerpointsindex[j]]);
                    bone.tag = "bone2D";
                    bone.transform.parent = fbone.transform;
                    Anima2D.Bone2D boneComponent = bone.AddComponent<Anima2D.Bone2D>();
                    if (points[i] == Vector2.zero || points[firstlayerpointsindex[j]] == Vector2.zero)
                    {
                        bone.transform.position = Vector3.zero;
                        boneComponent.globalendPosition = Vector3.zero;
                        boneComponent.globalstartPosition = Vector3.zero;
                        ResetZeroBone2DProperty(bone);
                    }
                    else
                    {
                        bone.transform.position = new Vector3(points[i].x, points[i].y, 0); //pixelsToUnit
                        boneComponent.globalstartPosition = bone.transform.position;
                        boneComponent.globalendPosition = new Vector3(points[firstlayerpointsindex[j]].x, points[firstlayerpointsindex[j]].y, 0);
                        UpdateBone2DProperty(bone);
                    }

                }

            }
            //3.1. 第一层级关键点到第二层级关键点的骨骼...第二层级关键点到第三层级关键点的骨骼
            else if (Array.IndexOf(ffirstlayerpointsindex, i) != -1 || Array.IndexOf(secondlayerpointsindex, i) != -1)
            {
                //转化为骨骼，零散到hierarchy,因为我是根据两个关键点求Bone2D的，所以Bone2D的长度、旋转角都应该求出来更新其值
                GameObject bone = new GameObject(skeletonsName[i + 1]);
                bone.tag = "bone2D";
                bone.transform.parent = fbone.transform;
                Anima2D.Bone2D boneComponent = bone.AddComponent<Anima2D.Bone2D>();

                //没有检测到关键点的情况,直接归为坐标原点
                if (points[i] == Vector2.zero || points[i + 1] == Vector2.zero)
                {
                    bone.transform.position = Vector3.zero;
                    boneComponent.globalendPosition = Vector3.zero;
                    boneComponent.globalstartPosition = Vector3.zero;
                    ResetZeroBone2DProperty(bone);
                }
                else
                {
                    bone.transform.position = new Vector3(points[i].x, points[i].y, 0);
                    boneComponent.globalendPosition = new Vector3(points[i + 1].x, points[i + 1].y, 0);
                    boneComponent.globalstartPosition = bone.transform.position;
                    UpdateBone2DProperty(bone);
                }
            }
        }
    }
    static void ResetZeroBone2DProperty(GameObject bone)
    {
        Anima2D.Bone2D boneComponent = bone.GetComponent<Anima2D.Bone2D>();
        boneComponent.localLength = 0;
    }
    //3.2.根据骨骼的两个端点，计算其旋转角(矩阵),长度，更新端点值让其能够在Scene场景中的绘制也随之跟新
    static void UpdateBone2DProperty(GameObject bone)
    {
        Quaternion l_deltaRotation = Quaternion.identity;
        //先看看BoneUtils类中有没有现成的接口--似乎都不太现成，所以先自己写吧
        Anima2D.Bone2D boneComponent = bone.GetComponent<Anima2D.Bone2D>();
        boneComponent.localLength = (boneComponent.globalstartPosition - boneComponent.globalendPosition).magnitude;
        Vector2 localPosition = new Vector2(boneComponent.globalendPosition.x - boneComponent.globalstartPosition.x,
                                            boneComponent.globalendPosition.y - boneComponent.globalstartPosition.y);
        float angle = Mathf.Atan2(localPosition.y, localPosition.x) * Mathf.Rad2Deg;
        l_deltaRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        bone.transform.localRotation *= l_deltaRotation;
    }
    public static BoneSkinningResult BoneSkinning()
    {
        if (!fbone || fbone.transform.childCount == 0)
            return BoneSkinningResult.BoneError;
        if (!spriteMeshData || spriteMeshData.indices.Length == 0)
            return BoneSkinningResult.MeshError;

        //遍历fbone下的所有bone2d子骨骼
        //foreach()
        //{
        //    BindBones();
        //}
        
        CalculateAutomaticWeights();

        DrawSkinning.InitBindingInfo();
        return BoneSkinningResult.Success;
    }
    static void BindBones(Anima2D.Bone2D bone)
    {
        BindInfo bindInfo = new BindInfo();
        //bindInfo.bindPose = bone.transform.worldToLocalMatrix * spriteMeshInstance.transform.localToWorldMatrix;

    }
    static void CalculateAutomaticWeights()
    {

    }

    public static void AutoProcessing()
    {

    }
}
