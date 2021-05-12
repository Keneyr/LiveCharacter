using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class DrawSprite
{
    public static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float width)
    {
        DrawLine(p1, p2, normal, width, width);
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float widthP1, float widthP2)
    {
        DrawLine(p1, p2, normal, widthP1, widthP2);
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float widthP1, float widthP2, Color color)
    {

        Vector3 right = Vector3.Cross(normal, p2 - p1).normalized;
        //handleWireMaterial.SetPass(0);
        GL.PushMatrix();
        //GL.MultMatrix(Handles.matrix);
        GL.Begin(4);
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

    public static void drawDotCyan(Vector3 position)
    {
        
    }
    public static void DrawEdge(SpriteMeshData spriteMeshData)
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
                Vector2 position = m_TexVertices[edge.node1.index];
                //DrawEdge(edge,1.0f);
            }
        }
    }
    void DrawEdge(Edge edge,float width)
    {
        Vector2 p1, p2;
        //p1 = edge
        //DrawLine()
    }
}
