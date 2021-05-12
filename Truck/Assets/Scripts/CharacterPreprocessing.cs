using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Sprites;
using System.Linq;
using System.Reflection;
/// <summary>
/// 对角色图像(png/jpg/psd)预处理合集，包括轮廓检测，姿态提取，三角剖分，骨骼生成，自动蒙皮，一键预处理
/// </summary>
public static class CharacterPreprocessing
{
    public static Sprite sprite;
    public static SpriteMeshData spriteMeshData = null;

    public static Texture2D texture;
    private static Rect rect;
    private static float detail;
    private static float alphaTolerance;
    private static bool holeDetection;
    private static Vector2[][] paths;


    public static void DetectContour()
    {
        if (CharacterManager.instance.tx == null)
            return;
        Texture2D tx = CharacterManager.instance.tx;
        //sprite = tx as Object as Sprite;
        sprite = Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), Vector2.zero);

        Vector2[] vertices;
        IndexedEdge[] edges;
        
        if (sprite)
        {
            //spriteMeshData = new SpriteMeshData();
            spriteMeshData = ScriptableObject.CreateInstance<SpriteMeshData>();
            spriteMeshData.name = tx.name + "_Data";

           
            GetSpriteContourData(sprite, out vertices, out edges);

            spriteMeshData.vertices = vertices;
            spriteMeshData.edges = edges;

            string assetPath = "/" + spriteMeshData.name + ".asset";
            AssetDataBase.SaveAsset<SpriteMeshData>(assetPath, spriteMeshData);

            //DrawEdge.drawEdge(spriteMeshData);
        }
    }
    private static void GetSpriteContourData(Sprite sprite, out Vector2[] vertices, out IndexedEdge[] edges)
    {
        int width = 0;
        int height = 0;

        GetSpriteTextureSize(sprite, ref width, ref height);

        Vector2[] uvs = SpriteUtility.GetSpriteUVs(sprite, false);

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
    private static void GetSpriteTextureSize(Sprite sprite, ref int width, ref int height)
    {
        if (sprite)
        {
            Texture2D texture = SpriteUtility.GetSpriteTexture(sprite, false);

            width = texture.width;
            height = texture.height;
        }
    }
    
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
    public static void ExtractPose()
    {

    }
    public static void Triangulation()
    {

    }
    public static void GenerateSkeleton()
    {

    }
    public static void BoneSkinning()
    {

    }
    public static void AutoProcessing()
    {

    }
}
