using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


public class DrawEdge : MonoBehaviour
{
    static Material wireMaterial; //方便GL绘图的材质

    private void Start()
    {
        CreateLineMaterial();
    }

    public void OnRenderObject()
    {
        drawEdge(CharacterPreprocessing.spriteMeshData);
    }

    static void CreateLineMaterial()
    {
        if(!wireMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            wireMaterial = new Material(shader);
            wireMaterial.hideFlags = HideFlags.HideAndDontSave;

            // Turn on alpha blending
            wireMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            wireMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            wireMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            wireMaterial.SetInt("_ZWrite", 0);
        }
    }
    public static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float width)
    {
        DrawLine(p1, p2, normal, width, width);
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float widthP1, float widthP2)
    {
        DrawLine(p1, p2, normal, widthP1, widthP2, Color.cyan);
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float widthP1, float widthP2, Color color)
    {

        Vector3 right = Vector3.Cross(normal, p2 - p1).normalized;
        wireMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(color);
        GL.Vertex(p1 + right * widthP1 * 0.5f);
        GL.Vertex(p1 - right * widthP1 * 0.5f);
        GL.Vertex(p2 - right * widthP2 * 0.5f);
        GL.Vertex(p1 + right * widthP1 * 0.5f);
        GL.Vertex(p2 - right * widthP2 * 0.5f);
        GL.Vertex(p2 + right * widthP2 * 0.5f);
        GL.End();
        GL.PopMatrix();
    }

    public static void drawEdge(SpriteMeshData spriteMeshData)
    {
        if(spriteMeshData.name != null)
        {
            List<Node> nodes = new List<Node>();
            List<Vector2> m_TexVertices = new List<Vector2>();
            List<Edge> edges = new List<Edge>();
            m_TexVertices = spriteMeshData.vertices.ToList();
            nodes = m_TexVertices.ConvertAll(v => Node.Create(m_TexVertices.IndexOf(v)) );
            edges = spriteMeshData.edges.ToList().ConvertAll(e=> Edge.Create(nodes[e.index1], nodes[e.index2]));
            

            for(int i=0;i<edges.Count;i++)
            {
                Edge edge = edges[i];
                Vector2 position1 = m_TexVertices[edge.node1.index];
                Vector2 position2 = m_TexVertices[edge.node2.index];
                DrawLine(position1, position2, Vector3.forward, 1.0f);
            }
        }
    }
}
