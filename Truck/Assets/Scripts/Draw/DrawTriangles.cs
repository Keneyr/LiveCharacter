using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DrawTriangles : MonoBehaviour
{
    static List<int> indices = new List<int>();
    static List<Vector2> points = new List<Vector2>();
    static Camera triangulateCamera;
    static float lineWidth = 0.01f;
    static float outlineWidth = 0.03f;
    static float pointRadius = 0.04f;
    static float expandScale = 1.1f;
    
    static Material meshMaterial;
    static Material outlineMaterial;
    RenderTexture renderTargetTexture;
    static float layer = 1;

    static RectTransform rt;
    private void Start()
    {
        CreateLineMaterial();
        triangulateCamera = GameObject.Find("TriangulateCamera").GetComponent<Camera>();
        Extra.InitCamera(triangulateCamera,layer);
        renderTargetTexture = new RenderTexture(256,256,24);
        triangulateCamera.targetTexture = renderTargetTexture;
        GetComponent<RawImage>().texture = renderTargetTexture;
        rt = GetComponent<RectTransform>();
    }
    public void OnRenderObject()
    {
        drawTriangles();
    }

    public static void ResetPastCharacterInfo()
    {
        indices.Clear();
        points.Clear();
    }
    void CreateLineMaterial()
    {
        if (!meshMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            meshMaterial = new Material(shader);
            meshMaterial.hideFlags = HideFlags.HideAndDontSave;

            //cyan color
            meshMaterial.SetColor("_Color", Color.black);

            // Turn on alpha blending
            meshMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            meshMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            meshMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            meshMaterial.SetInt("_ZWrite", 0);
        }
        if (!outlineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            outlineMaterial = new Material(shader);
            outlineMaterial.hideFlags = HideFlags.HideAndDontSave;

            //cyan color
            outlineMaterial.SetColor("_Color", Color.white);

            // Turn on alpha blending
            outlineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            outlineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            outlineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            outlineMaterial.SetInt("_ZWrite", 0);
        }
    }
    //这个函数参数最好是一个bool变量，而不是一直找其他类下的静态变量
    public static void drawTriangles()
    {
        if (indices.Count == 0)
            return;

        for (int i=0;i<indices.Count;i+=3)
        {
            int index = indices[i];
            int index1 = indices[i + 1];
            int index2 = indices[i + 2];

            Vector3 v1 = new Vector3(points[index].x, points[index].y,layer);
            Vector3 v2 = new Vector3(points[index1].x, points[index1].y, layer);
            Vector3 v3 = new Vector3(points[index2].x, points[index2].y, layer);

            Extra.DrawLine(v1,v2, Vector3.forward, lineWidth, Color.black,meshMaterial);
            Extra.DrawLine(v2,v3, Vector3.forward, lineWidth, Color.black, meshMaterial);
            Extra.DrawLine(v1,v3, Vector3.forward, lineWidth, Color.black, meshMaterial);
        }
        for(int i=0;i< points.Count; i++)
        {
            Extra.DrawCircle(new Vector3(points[i].x, points[i].y, layer), pointRadius, 0,Color.cyan,outlineMaterial);
        }
        DrawEdge.drawEdge(layer);
    }

    

    public static void InitMeshIndices(SpriteMeshData spriteMeshData)
    {
        if (!spriteMeshData)
            return;
        if (spriteMeshData.indices.Length == 0)
            return;

        //clear cache
        indices.Clear();
        points.Clear();
        //calculate rect
        
        //Init data
        indices = spriteMeshData.indices.ToList();
        points = spriteMeshData.vertices.ToList();

        //set camera
        Extra.SetInnerCamera(triangulateCamera,layer, Extra.rect, rt);

        lineWidth = Extra.rect.height * 0.005f;

    }
}
