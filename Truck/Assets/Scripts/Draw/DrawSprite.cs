using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static void DrawEdge(SpriteMeshData spriteMeshData)
    {
        if(spriteMeshData)
        {
            for(int i=0;i<spriteMeshData.vertices.Length;i++)
            {

            }
        }
    }
}
