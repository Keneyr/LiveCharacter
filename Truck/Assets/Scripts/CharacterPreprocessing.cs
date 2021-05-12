using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Sprites;
using System.Linq;
using System.Reflection;
using UnityEditor;
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
        //Object _object = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(tx));
        //sprite = _object as Sprite;

        Vector2[] vertices;
        
        if (sprite)
        {
            spriteMeshData = ScriptableObject.CreateInstance<SpriteMeshData>();
            spriteMeshData.name = tx.name + "_Data";
            //spriteMeshData.hideFlags = HideFlags.HideInHierarchy;
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(Application.streamingAssetsPath + "/" + spriteMeshData.name + ".asset");
            Console.Log("Save asset to" + assetPath);

            GetSpriteContourData(sprite, out vertices);
            spriteMeshData.vertices = vertices;

            AssetDatabase.CreateAsset(spriteMeshData, assetPath);
            //创建asset资源到本地--方便其他算法共享及后续修改数据
            //AssetDatabase.AddObjectToAsset(spriteMeshData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);

            DrawSprite.DrawEdge(spriteMeshData);
        }
    }
    private static void GetSpriteContourData(Sprite sprite, out Vector2[] vertices)
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
