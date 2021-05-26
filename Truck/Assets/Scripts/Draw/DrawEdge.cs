using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class DrawEdge : MonoBehaviour
{
    static List<Node> nodes = new List<Node>();
    static List<Vector2> m_TexVertices = new List<Vector2>();
    static List<Edge> edges = new List<Edge>();
    static Camera contourCamera;
    static float lineWidth = 0.05f;
    static float expandScale = 1.1f;
    
    static Material wireMaterial; //方便GL绘图的材质
    RenderTexture renderTargetTexture;
    static RectTransform rt;
    static float layer = 0;
    public static void ResetPastCharacterInfo()
    {
        m_TexVertices.Clear();
        nodes.Clear();
        edges.Clear();
    }

    private void Start()
    {
        CreateLineMaterial();
        contourCamera = GameObject.Find("ContourCamera").GetComponent<Camera>();
        Extra.InitCamera(contourCamera, layer);
        renderTargetTexture = new RenderTexture(256,256,24);
        contourCamera.targetTexture = renderTargetTexture;
        GetComponent<RawImage>().texture = renderTargetTexture;
        rt = GetComponent<RectTransform>();
    }

    public void OnRenderObject()
    {
        drawEdge(layer);
    }

    void CreateLineMaterial()
    {
        if(!wireMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            wireMaterial = new Material(shader);
            wireMaterial.hideFlags = HideFlags.HideAndDontSave;

            //cyan color
            wireMaterial.SetColor("_Color", Color.cyan);

            // Turn on alpha blending
            wireMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            wireMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            wireMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            wireMaterial.SetInt("_ZWrite", 0);
        }
    }
    
    
    public static void drawEdge(float layer)
    {
        if (edges.Count == 0)
            return;
        
        for (int i=0;i<edges.Count;i++)
        {
            Edge edge = edges[i];
            Vector3 position1 = new Vector3(m_TexVertices[edge.node1.index].x, m_TexVertices[edge.node1.index].y,layer) ;
            Vector3 position2 = new Vector3(m_TexVertices[edge.node2.index].x, m_TexVertices[edge.node2.index].y, layer);
            Extra.DrawLine(position1, position2, Vector3.forward, lineWidth, wireMaterial);
        }
        
    }

    //Called when DetectContour() in CharacterPreprocessing.cs return with a Success
    public static void InitEdges(SpriteMeshData spriteMeshData)
    {
        //clear cache
        m_TexVertices.Clear();
        nodes.Clear();
        edges.Clear();
        //calculate rect
        Rect rect = new Rect();
        for (int i = 0; i < spriteMeshData.vertices.Length; i++)
        {
            spriteMeshData.vertices[i] /= 100.0f;
            rect.yMax = Mathf.Max(rect.yMax, spriteMeshData.vertices[i].y);
            rect.xMax = Mathf.Max(rect.xMax, spriteMeshData.vertices[i].x);
            rect.yMin = Mathf.Min(rect.yMin, spriteMeshData.vertices[i].y);
            rect.xMin = Mathf.Min(rect.xMin, spriteMeshData.vertices[i].x);
        }
        //Init data
        m_TexVertices = spriteMeshData.vertices.ToList();
        nodes = m_TexVertices.ConvertAll(v => Node.Create(m_TexVertices.IndexOf(v)));
        edges = spriteMeshData.edges.ToList().ConvertAll(e => Edge.Create(nodes[e.index1], nodes[e.index2]));
        //set camera
        Extra.SetInnerCamera(contourCamera,layer, rect, rt, expandScale);
        
        lineWidth = rect.height*0.01f;
    }
}
