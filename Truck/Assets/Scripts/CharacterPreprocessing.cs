using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.IO;
using System;
using System.Text;
using System.Diagnostics;
using TriangleNet;
using TriangleNet.Geometry;
using System.Threading.Tasks;
using System.Threading;
/// <summary>
/// 对角色图像(png/jpg/psd)预处理合集，包括轮廓检测，姿态提取，三角剖分，骨骼生成，自动蒙皮，一键预处理
/// 注意：用到了UnityEditor.Sprites下的函数
/// </summary>
public class CharacterPreprocessing : MonoBehaviour
{
    //spriteMeshData
    static Sprite sprite;
    public static SpriteMeshData spriteMeshData = null;
    //static Texture2D texture;
    static string txname = null;
    
    //骨骼
    public static OpenPose.MultiArray<float> keypoints = new OpenPose.MultiArray<float>();
    public static GameObject fbone = new GameObject("Bone");
    public static List<Bone2D> BonesInHierarchy = new List<Bone2D>();
    
    //SpriteMeshInstance
    public static SpriteMeshGameObject spriteMeshGO = null;
    static Material m_SpritesMaterial = null;
    public static Material spritesDefaultMaterial
    {
        get
        {
            if (!m_SpritesMaterial)
            {
                m_SpritesMaterial = new Material(Shader.Find("Unlit/Transparent"));
                //GameObject go = new GameObject();
                //SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                //m_SpritesMaterial = sr.sharedMaterial;
                //GameObject.DestroyImmediate(go);
            }

            return m_SpritesMaterial;
        }
    }
    /// <summary>
    /// 轮廓检测
    /// </summary>
    /// <returns></returns>
    public static ContourDetectResult DetectContour()
    {
        if (CharacterManager.instance.tx == null)
            return ContourDetectResult.TextureError;

        Texture2Sprite();

        if (sprite)
        {
            spriteMeshData = ScriptableObject.CreateInstance<SpriteMeshData>();
            spriteMeshData.sprite = sprite;
            float detail = 0.25f;
            float alphaTolerance = 0.05f;
            float tesselation = 0f;
            bool holeDetection = true;
            GenerateSpriteOutline(detail,alphaTolerance,holeDetection,tesselation,spriteMeshData);
            txname = System.IO.Path.GetFileNameWithoutExtension(CharacterManager.instance.tx.name);
            spriteMeshData.name = txname + "_Data";
            spriteMeshData.meshname = txname;
            
            //UpdateSpriteMeshDataSharedMesh();

            SaveAssetsFile(spriteMeshData);

            DrawEdge.InitEdges(spriteMeshData);
            return ContourDetectResult.Success;
        }
        return ContourDetectResult.SpriteError;
    }
    static void GenerateSpriteOutline(float detail,float alphaTolerance,bool holeDetection,float tesselation,SpriteMeshData _spriteMeshData)
    {
        List<Vector2> l_texcoords;
        List<IndexedEdge> l_indexedEdges;
        List<int> l_indices;
        Texture2D texture = _spriteMeshData.sprite.texture;
        List<Hole> holes = new List<Hole>();
        InitFromOutline(texture,detail,alphaTolerance,holeDetection,
            out l_texcoords,out l_indexedEdges,out l_indices);

        spriteMeshData.vertices = l_texcoords.ToArray();
        spriteMeshData.edges = l_indexedEdges.ToArray();
        spriteMeshData.indices = l_indices.ToArray();
    }
    static void InitFromOutline(Texture2D texture,float detail,float alphaTolerance,bool holeDetection,
        out List<Vector2> vertices,out List<IndexedEdge> indexEdges,out List<int> indices)
    {
        vertices = new List<Vector2>();
        indexEdges = new List<IndexedEdge>();
        indices = new List<int>();

        if(texture)
        {
            Rect rect = new Rect(0,0,texture.width,texture.height);
            Vector2[][] paths = GenerateOutline(texture, rect, detail, (byte)(alphaTolerance * 255f), holeDetection);
            int startIndex = 0;
            for(int i=0;i<paths.Length; i++)
            {
                Vector2[] path = paths[i];
                for(int j=0;j<path.Length;j++)
                {
                    vertices.Add(new Vector2(path[j].x + rect.center.x, path[j].y - rect.center.y)); //+rect.center?
                    indexEdges.Add(new IndexedEdge(startIndex + j,startIndex + ((j + 1) % path.Length)));
                }
                startIndex += path.Length;
            }
            List<Hole> holes = new List<Hole>();
            //有了细化的轮廓点，再进行三角剖分
            Triangulate(vertices,indexEdges,holes,ref indices);
        }

    }
    //默认是调用SpriteUtility函数下的方法，但是这个是UnityEditor下的函数，所以要有改动
    static Vector2[][] GenerateOutline(Texture2D texture, Rect rect, float detail, byte alphaTolerance, bool holeDetection)
    {
        Vector2[][] paths = null;

        //GenerateOutlinePlugin.GenerateOutline(texture,rect,detail,alphaTolerance,holeDetection,paths);

        MethodInfo methodInfo = typeof(UnityEditor.Sprites.SpriteUtility).GetMethod("GenerateOutline", BindingFlags.Static | BindingFlags.NonPublic);

        if (methodInfo != null)
        {
            object[] parameters = new object[] { texture, rect, detail, alphaTolerance, holeDetection, null };
            methodInfo.Invoke(null, parameters);

            paths = (Vector2[][])parameters[5];
        }
        return paths;
        
    }
    public static void Triangulate(List<Vector2> vertices,List<IndexedEdge>edges,List<Hole>holes,ref List<int> indices)
    {
        indices.Clear();
        if(vertices.Count>0)
        {
            InputGeometry inputGeometry = new InputGeometry(vertices.Count);
            for(int i=0;i<vertices.Count;++i)
            {
                Vector2 position = vertices[i];
                inputGeometry.AddPoint(position.x,position.y);
            }
            for(int i=0;i<edges.Count;++i)
            {
                IndexedEdge edge = edges[i];
                inputGeometry.AddSegment(edge.index1,edge.index2);
            }
            for(int i=0;i<holes.Count;++i)
            {
                Vector2 hole = holes[i].vertex;
                inputGeometry.AddHole(hole.x,hole.y);
            }
            TriangleNet.Mesh triangleMesh = new TriangleNet.Mesh();
            triangleMesh.Triangulate(inputGeometry);
            foreach(TriangleNet.Data.Triangle triangle in triangleMesh.Triangles)
            {
                if (triangle.P0 >= 0 && triangle.P0 < vertices.Count &&
                       triangle.P0 >= 0 && triangle.P1 < vertices.Count &&
                       triangle.P0 >= 0 && triangle.P2 < vertices.Count)
                {
                    indices.Add(triangle.P0);
                    indices.Add(triangle.P2);
                    indices.Add(triangle.P1);
                }
            }
        }
    }
    static void GetSpriteContourData(Sprite sprite, out Vector3[] vertices, out IndexedEdge[] edges)
    {
        int width = 0;
        int height = 0;

        GetSpriteTextureSize(sprite, ref width, ref height);

        //Vector2[] uvs = SpriteUtility.GetSpriteUVs(sprite, false);
        Vector2[] uvs = sprite.uv;
        vertices = new Vector3[uvs.Length];

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

    //注意当第一次点击导入角色时，也会调用Reset函数，所以不能轻易把变量设置成null，否则最开始new到的内存白瞎
    public static void ResetPastCharacterInfo()
    {
        //spriteMesh
        spriteMeshData = null;

        //bones
        keypoints.Clear();
        //fbone = null;
        BonesInHierarchy.Clear();

        //spriteMeshInstance
        spriteMeshGO = null;

        //清除子物体，否则换角色不会重置
        for (int i=0;i<BonesInHierarchy.Count;i++)
        {
            Destroy(BonesInHierarchy[i].gameObject);
        }

        DrawEdge.ResetPastCharacterInfo();
        DrawSkeleton.ResetPastCharacterInfo();
        DrawTriangles.ResetPastCharacterInfo();
        DrawSkinning.ResetPastCharacterInfo();
    }
    static void Texture2Sprite()
    {
        Texture2D tx = CharacterManager.instance.tx;
        //sprite = tx as Object as Sprite;
        sprite = Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), /*Top Left*/Vector2.zero, /*pixelsPerUnit*/100); 
    }
    static void SaveAssetsFile(SpriteMeshData _spriteMeshData)
    {
        string assetPath = "/" + _spriteMeshData.name + ".asset";
        AssetDataBase.SaveAsset<SpriteMeshData>(assetPath, _spriteMeshData);
    }

    /// <summary>
    /// 姿态提取
    /// </summary>
    /// <returns></returns>
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

            //imagePath = CharacterManager.instance.tx.name.Substring(0, CharacterManager.instance.tx.name.LastIndexOf("."));
            //imagePath = imagePath + "_rendered.png";

            return PoseExtractResult.OpenPoseStart;
        }
        return PoseExtractResult.ImageError;
    }
    //forOpenPose
    public static PoseExtractResult ExtractPoseDatum(OpenPose.OPDatum datum)
    {
        txname = System.IO.Path.GetFileNameWithoutExtension(CharacterManager.instance.tx.name);
        if(datum.name.Equals(txname))
        {
            keypoints = datum.poseKeypoints;

            //延时读取，因为openpose会延迟占用文件资源
            //var t2 = Task.Run(ReadRenderedImageTask);
            Thread.Sleep(1000);
            string imagePath = CharacterManager.instance.tx.name.Substring(0, CharacterManager.instance.tx.name.LastIndexOf("."));
            imagePath = imagePath + "_rendered.png";
            DrawSkeleton.InitRenderImage(imagePath);

            return PoseExtractResult.Success;
        }
        return PoseExtractResult.OpenPoseError;
        
    }
    static async void ReadRenderedImageTask()
    {
        await Task.Delay(1000);

        string imagePath = CharacterManager.instance.tx.name.Substring(0, CharacterManager.instance.tx.name.LastIndexOf("."));
        imagePath = imagePath + "_rendered.png";
        DrawSkeleton.InitRenderImage(imagePath);
        //return PoseExtractResult.Success;

    }


    /// <summary>
    /// 三角剖分
    /// </summary>
    /// <returns></returns>

    public static TriangulateResult Triangulation()
    {
        if(sprite)
        {
            if(spriteMeshData.vertices.Length > 0 && spriteMeshData.edges.Length > 0)
            {
                //int[] indices;
                //GetSpriteMeshData(sprite,out indices);
                //spriteMeshData.indices = indices;
                //UpdateSpriteMeshDataSharedMesh();
                List<Vector2> l_texcoords = spriteMeshData.vertices.ToList();
                List<IndexedEdge> l_indexedEdges = spriteMeshData.edges.ToList();
                List<int> l_indices = spriteMeshData.indices.ToList();
                List<Hole> holes = new List<Hole>();
                float tesselation = 0f;
                Tessellate(l_texcoords, l_indexedEdges, holes, l_indices, tesselation * 10f);

                SaveAssetsFile(spriteMeshData);

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

    /// <summary>
    /// 骨骼生成
    /// </summary>
    /// <returns></returns>
    public static BoneGenerateResult GenerateSkeleton()
    {
        //如果是刚才的姿态提取算法提取到的数据
        if(keypoints.Count > 0)
        {
            Extra.ConvertKeyPoints2Skeleton(keypoints,fbone,BonesInHierarchy);

            DrawSkeleton.InitSkeletonBone2D(fbone,BonesInHierarchy);

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
                Extra.ParseJson(Extra.ReadFile(jsonPath),ref keypoints);

                Extra.ConvertKeyPoints2Skeleton(keypoints,fbone,BonesInHierarchy);

                DrawSkeleton.InitSkeletonBone2D(fbone,BonesInHierarchy);
                return BoneGenerateResult.Success;
            }
        }
        return BoneGenerateResult.jsonError;
    }


    /// <summary>
    /// 蒙皮
    /// </summary>
    /// <returns></returns>

    public static BoneSkinningResult BoneSkinning()
    {
        if (!fbone || fbone.transform.childCount <= 0 || BonesInHierarchy.Count <= 0)
            return BoneSkinningResult.BoneError;
        if (fbone.transform.childCount != BonesInHierarchy.Count)
            return BoneSkinningResult.BoneError;
        if (!spriteMeshData || spriteMeshData.indices.Length <= 0)
            return BoneSkinningResult.MeshError;

        CreateSpriteMeshGameObject(); //创建SpriteMeshInstance
        BindBones(); //骨骼绑定，计算权重，自动更新skinnedMeshRenderer
        
        DrawSkinning.InitBindingInfo(spriteMeshData);
        return BoneSkinningResult.Success;
    }
    static void CreateSpriteMeshGameObject()
    {
        GameObject gameObject = new GameObject(spriteMeshData.meshname);

        if (gameObject)
        {
            gameObject.transform.position = new Vector3(0,0,0);
            spriteMeshGO = gameObject.AddComponent<SpriteMeshGameObject>();
            //spriteMeshGO.

            spriteMeshGO.spriteMeshData = spriteMeshData;

            spriteMeshGO.sharedMaterial = spritesDefaultMaterial;
            spriteMeshGO.sharedMaterial.mainTexture = spriteMeshData.sprite.texture;
            //material
        }
    }
    static void BindBones()
    {
        if(spriteMeshGO)
        {
            List<BindingInfo> bindPoses = new List<BindingInfo>();
            foreach(Bone2D bone in BonesInHierarchy)//因为要绑定所有的骨骼
            {
                BindingInfo bindInfo = new BindingInfo();
                //参考姿势->从模型空间转换到世界空间(骨架空间)，再次转换到骨骼空间
                bindInfo.bindPose = bone.transform.worldToLocalMatrix * spriteMeshGO.transform.localToWorldMatrix; 
                bindInfo.boneLength = bone.localLength;
                bindInfo.name = bone.name;
                bindInfo.color = Extra.GetRingColor(bindPoses.Count);
                //bindInfo.path = GetBonePath(bone);

                //spriteMeshData.bindPoses spriteMeshGO.bones
                if (!bindPoses.Contains(bindInfo))
                {
                    bindPoses.Add(bindInfo);
                }
            }
            //update spriteMeshData & spriteMeshInstance(GO)
            List<Vector2> m_TexVertices = spriteMeshData.vertices.ToList();
            spriteMeshData.bindPoses = bindPoses.ToArray();
         
            spriteMeshGO.bones = BonesInHierarchy;
            CalculateAutomaticWeights();
            UpdateRenderer(); //估计要放在SpriteMeshGameObject下每帧执行
        }
    }
    static void UpdateRenderer()
    {
        if (!spriteMeshGO)
            return;
        if(spriteMeshData)
        {
            UpdateSpriteMeshDataSharedMesh();
            UnityEngine.Mesh sharedMesh = spriteMeshData.sharedMesh;

            if (sharedMesh.bindposes.Length > 0 && spriteMeshGO.bones.Count > sharedMesh.bindposes.Length)
            {
                spriteMeshGO.bones = spriteMeshGO.bones.GetRange(0, sharedMesh.bindposes.Length);
            }

            if (CanEnableSkinning(spriteMeshGO))
            {
                
                SkinnedMeshRenderer skinnedMeshRenderer = spriteMeshGO.cachedSkinnedRenderer;
                if(!skinnedMeshRenderer)
                {
                    skinnedMeshRenderer = spriteMeshGO.gameObject.AddComponent<SkinnedMeshRenderer>();       
                }
                skinnedMeshRenderer.updateWhenOffscreen = true;
                
                
                skinnedMeshRenderer.bones = spriteMeshGO.bones.ConvertAll(bone=>bone.transform).ToArray();
                skinnedMeshRenderer.sharedMesh = sharedMesh;
                skinnedMeshRenderer.material = spriteMeshGO.sharedMaterial;
                //skinnedMeshRenderer.material.mainTexture = 
                if (spriteMeshGO.bones.Count>0)
                {
                    //skinnedMeshRenderer.rootBone = spriteMeshGO.bones[0].transform;
                    skinnedMeshRenderer.rootBone = null;
                }
                skinnedMeshRenderer.materials[0] = spriteMeshGO.sharedMaterial;
            }
            
        }
    }
    static bool CanEnableSkinning(SpriteMeshGameObject spriteMeshInstance)
    {
        return spriteMeshInstance.spriteMeshData && 
            !HasNullBones(spriteMeshInstance) && 
            spriteMeshInstance.bones.Count > 0 && 
     (spriteMeshInstance.spriteMeshData.sharedMesh.bindposes.Length == spriteMeshInstance.bones.Count);
    }
    public static bool HasNullBones(SpriteMeshGameObject spriteMeshInstance)
    {
        if (spriteMeshInstance)
        {
            return spriteMeshInstance.bones.Contains(null);
        }
        return false;
    }
    static void UpdateSpriteMeshDataSharedMesh()
    {
        if (!spriteMeshData)
            return;
        if(!spriteMeshData.sharedMesh)
        {
            UnityEngine.Mesh mesh = new UnityEngine.Mesh();
            spriteMeshData.sharedMesh = mesh;
            
        }

        List<Matrix4x4> bindposes = 
            (new List<BindingInfo>(spriteMeshData.bindPoses)).ConvertAll(p => p.bindPose);

        BoneWeight[] boneWeightsData = spriteMeshData.boneWeights;

        List<UnityEngine.BoneWeight> boneWeights = new List<UnityEngine.BoneWeight>(boneWeightsData.Length);

        Vector2 textureWidthHeightInv = 
            new Vector2(1f / sprite.texture.width, 1f / sprite.texture.height)*100.0f;

        Vector2[] uvs = 
            (new List<Vector2>(spriteMeshData.vertices)).ConvertAll(v =>Vector2.up + Vector2.Scale(v, textureWidthHeightInv)).ToArray(); //?
    
        Vector3[] tmp = 
            new List<Vector2>((spriteMeshData.vertices)).ConvertAll(v => new Vector3(v.x,v.y,0)).ToArray() ;

        Vector3[] normals = 
            (new List<Vector3>(tmp)).ConvertAll(v => Vector3.back).ToArray();

        List<float> verticesOrder = new List<float>(spriteMeshData.vertices.Length);
        for (int i = 0; i < boneWeightsData.Length; i++)
        {
            BoneWeight boneWeight = boneWeightsData[i];

            List<KeyValuePair<int, float>> pairs = new List<KeyValuePair<int, float>>();
            pairs.Add(new KeyValuePair<int, float>(boneWeight.boneIndex0, boneWeight.weight0));
            pairs.Add(new KeyValuePair<int, float>(boneWeight.boneIndex1, boneWeight.weight1));
            pairs.Add(new KeyValuePair<int, float>(boneWeight.boneIndex2, boneWeight.weight2));
            pairs.Add(new KeyValuePair<int, float>(boneWeight.boneIndex3, boneWeight.weight3));

            pairs = pairs.OrderByDescending(s => s.Value).ToList();

            UnityEngine.BoneWeight boneWeight2 = new UnityEngine.BoneWeight();
            boneWeight2.boneIndex0 = Mathf.Max(0, pairs[0].Key);
            boneWeight2.boneIndex1 = Mathf.Max(0, pairs[1].Key);
            boneWeight2.boneIndex2 = Mathf.Max(0, pairs[2].Key);
            boneWeight2.boneIndex3 = Mathf.Max(0, pairs[3].Key);
            boneWeight2.weight0 = pairs[0].Value;
            boneWeight2.weight1 = pairs[1].Value;
            boneWeight2.weight2 = pairs[2].Value;
            boneWeight2.weight3 = pairs[3].Value;

            boneWeights.Add(boneWeight2);

            float vertexOrder = i;

            if (spriteMeshData.bindPoses.Length > 0)
            {
                vertexOrder = spriteMeshData.bindPoses[boneWeight2.boneIndex0].zOrder * boneWeight2.weight0 +
                    spriteMeshData.bindPoses[boneWeight2.boneIndex1].zOrder * boneWeight2.weight1 +
                        spriteMeshData.bindPoses[boneWeight2.boneIndex2].zOrder * boneWeight2.weight2 +
                        spriteMeshData.bindPoses[boneWeight2.boneIndex3].zOrder * boneWeight2.weight3;
            }

            verticesOrder.Add(vertexOrder);
        }



        spriteMeshData.sharedMesh.Clear();
        spriteMeshData.sharedMesh.vertices = tmp;
        spriteMeshData.sharedMesh.uv = uvs; //?感觉这样不对呢..
        spriteMeshData.sharedMesh.triangles = spriteMeshData.indices.ToArray();
        spriteMeshData.sharedMesh.normals = normals;
        spriteMeshData.sharedMesh.boneWeights = boneWeights.ToArray();
        spriteMeshData.sharedMesh.bindposes = bindposes.ToArray();
        spriteMeshData.sharedMesh.RecalculateBounds();
    }

    static void CalculateAutomaticWeights()
    {
        if (!spriteMeshData)
            return;
        if (spriteMeshData.bindPoses.Length <= 0)
            return;
        if (spriteMeshData.vertices.Length <= 0)
            return;
        List<Vector2> m_TexVertices = spriteMeshData.vertices.ToList();
        List<Node> targetNodes = m_TexVertices.ConvertAll(v => Node.Create(m_TexVertices.IndexOf(v)));
        List<IndexedEdge> indexedEdges = spriteMeshData.edges.ToList();

        //控制点--一根骨骼的两个端点
        List<Vector2> controlPoints = new List<Vector2>();
        List<IndexedEdge> controlPointEdges = new List<IndexedEdge>();
        List<int> pins = new List<int>();

        foreach (BindingInfo bindInfo in spriteMeshData.bindPoses.ToList())
        {
            Vector2 tip = (Vector2)bindInfo.position;
            Vector2 tail = (Vector2)bindInfo.endPoint;

            if (bindInfo.boneLength <= 0f)
            {
                int index = controlPoints.Count;
                controlPoints.Add(tip);
                pins.Add(index);

                continue;
            }
            int index1 = -1;
            if (!ContainsVector(tip, controlPoints, 0.01f, out index1))
            {
                index1 = controlPoints.Count;
                controlPoints.Add(tip);
            }

            int index2 = -1;
            if (!ContainsVector(tail, controlPoints, 0.01f, out index2))
            {
                index2 = controlPoints.Count;
                controlPoints.Add(tail);
            }
            IndexedEdge edge = new IndexedEdge(index1, index2);
            controlPointEdges.Add(edge);
        }
        /*
        * 第一个参数：顶点
        * 第个参数：边
        * 第三个参数：骨骼的两个端点做控制点
        * 第四个参数：骨骼
        * 第五个参数：
        */
        UnityEngine.BoneWeight[] boneWeights = BbwPlugin.CalculateBbw
                (m_TexVertices.ToArray(), indexedEdges.ToArray(), controlPoints.ToArray(), controlPointEdges.ToArray(), pins.ToArray());

        //每个顶点都有自己绑定的权重信息
        if(spriteMeshData.boneWeights.Length != boneWeights.Length)
        {
            spriteMeshData.boneWeights = new BoneWeight[boneWeights.Length];

        }
        foreach (Node node in targetNodes)
        {
            UnityEngine.BoneWeight unityBoneWeight = boneWeights[node.index];

            SetBoneWeight(node, CreateBoneWeightFromUnityBoneWeight(unityBoneWeight));
        }

    }
    static void SetBoneWeight(Node node, BoneWeight boneWeight)
    {
        
        spriteMeshData.boneWeights[node.index] = boneWeight;
    }

    static BoneWeight CreateBoneWeightFromUnityBoneWeight(UnityEngine.BoneWeight unityBoneWeight)
    {
        BoneWeight boneWeight = new BoneWeight();

        boneWeight.boneIndex0 = unityBoneWeight.boneIndex0;
        boneWeight.boneIndex1 = unityBoneWeight.boneIndex1;
        boneWeight.boneIndex2 = unityBoneWeight.boneIndex2;
        boneWeight.boneIndex3 = unityBoneWeight.boneIndex3;
        boneWeight.weight0 = unityBoneWeight.weight0;
        boneWeight.weight1 = unityBoneWeight.weight1;
        boneWeight.weight2 = unityBoneWeight.weight2;
        boneWeight.weight3 = unityBoneWeight.weight3;

        return boneWeight;
    }

    static bool ContainsVector(Vector2 vectorToFind, List<Vector2> list, float epsilon, out int index)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector2 v = list[i];
            if ((v - vectorToFind).sqrMagnitude < epsilon)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    //曲面细分
    public static void Tessellate(List<Vector2> vertices, List<IndexedEdge> indexedEdges, List<Hole> holes, List<int> indices, float tessellationAmount)
    {
        if (tessellationAmount <= 0f)
        {
            return;
        }

        indices.Clear();

        if (vertices.Count >= 3)
        {
            InputGeometry inputGeometry = new InputGeometry(vertices.Count);

            for (int i = 0; i < vertices.Count; ++i)
            {
                Vector2 vertex = vertices[i];
                inputGeometry.AddPoint(vertex.x, vertex.y);
            }

            for (int i = 0; i < indexedEdges.Count; ++i)
            {
                IndexedEdge edge = indexedEdges[i];
                inputGeometry.AddSegment(edge.index1, edge.index2);
            }

            for (int i = 0; i < holes.Count; ++i)
            {
                Vector2 hole = holes[i].vertex;
                inputGeometry.AddHole(hole.x, hole.y);
            }

            TriangleNet.Mesh triangleMesh = new TriangleNet.Mesh();
            TriangleNet.Tools.Statistic statistic = new TriangleNet.Tools.Statistic();

            triangleMesh.Triangulate(inputGeometry);

            triangleMesh.Behavior.MinAngle = 20.0;
            triangleMesh.Behavior.SteinerPoints = -1;
            triangleMesh.Refine(true);

            statistic.Update(triangleMesh, 1);

            triangleMesh.Refine(statistic.LargestArea / tessellationAmount);
            triangleMesh.Renumber();

            vertices.Clear();
            indexedEdges.Clear();

            foreach (TriangleNet.Data.Vertex vertex in triangleMesh.Vertices)
            {
                vertices.Add(new Vector2((float)vertex.X, (float)vertex.Y));
            }

            foreach (TriangleNet.Data.Segment segment in triangleMesh.Segments)
            {
                indexedEdges.Add(new IndexedEdge(segment.P0, segment.P1));
            }

            foreach (TriangleNet.Data.Triangle triangle in triangleMesh.Triangles)
            {
                if (triangle.P0 >= 0 && triangle.P0 < vertices.Count &&
                   triangle.P0 >= 0 && triangle.P1 < vertices.Count &&
                   triangle.P0 >= 0 && triangle.P2 < vertices.Count)
                {
                    indices.Add(triangle.P0);
                    indices.Add(triangle.P2);
                    indices.Add(triangle.P1);
                }
            }
        }
    }

    public static void AutoProcessing()
    {

    }
}
