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
    static float lineWidth = 0.05f;
    static float expandScale = 1.1f;
    
    static Material meshMaterial;
    RenderTexture renderTargetTexture;

    static RectTransform rt;
    private void Start()
    {
        CreateLineMaterial();
        triangulateCamera = GameObject.Find("TriangulateCamera").GetComponent<Camera>();
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
            Vector2 v1 = points[index];
            Vector2 v2 = points[index1];
            Vector2 v3 = points[index2];

            Extra.DrawLine(v1,v2, Vector3.forward, lineWidth, meshMaterial);
            Extra.DrawLine(v2,v3, Vector3.forward, lineWidth, meshMaterial);
            Extra.DrawLine(v1,v3, Vector3.forward, lineWidth, meshMaterial);
        }
        for(int i=0;i< points.Count; i++)
        {
            Extra.DrawPointCyan(points[i], lineWidth, meshMaterial);
        }
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
        Rect rect = new Rect();
        for(int i=0;i<spriteMeshData.vertices.Length;i++)
        {
            rect.yMax = Mathf.Max(rect.yMax, spriteMeshData.vertices[i].y);
            rect.xMax = Mathf.Max(rect.xMax, spriteMeshData.vertices[i].x);
            rect.yMin = Mathf.Min(rect.yMin, spriteMeshData.vertices[i].y);
            rect.xMin = Mathf.Min(rect.xMin, spriteMeshData.vertices[i].x);
        }
        //Init data
        indices = spriteMeshData.indices.ToList();
        points = spriteMeshData.vertices.ToList();

        //set camera
        Extra.SetInnerCamera(triangulateCamera, rect, rt, expandScale);

        lineWidth = rect.height * 0.01f;

    }
}
