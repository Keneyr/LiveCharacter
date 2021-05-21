using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
/// <summary>
/// 公开绘制接口，方便其他绘制类进行调用
/// 其他工具函数
/// </summary>


public class Extra
{
    //const Material defaultMaterial = null;
    //const float lineWidth = 0.05f;
    static Vector3[] s_array;
    static Vector3[] s_circleArray;
    public static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float width, Material mat)
    {
        DrawLine(p1, p2, normal, width, width, mat);
    }

    private static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float widthP1, float widthP2, Material mat)
    {
        Vector3 right = Vector3.Cross(normal, p2 - p1).normalized;
        mat.SetPass(0);
        GL.PushMatrix();
        //GL.LoadOrtho();
        GL.Begin(GL.LINES);
        //GL.Color(color);
        GL.Vertex(p1 + right * widthP1 * 0.5f);
        GL.Vertex(p1 - right * widthP1 * 0.5f);

        GL.Vertex(p2 - right * widthP2 * 0.5f);
        GL.Vertex(p1 + right * widthP1 * 0.5f);

        GL.Vertex(p2 - right * widthP2 * 0.5f);
        GL.Vertex(p2 + right * widthP2 * 0.5f);

        GL.End();
        GL.PopMatrix();
    }

    public static void DrawPointCyan(Vector3 position, float linewidth, Material mat)
    {
        DrawRectangle(position, linewidth, mat);
    }
    private static void DrawCircle(float x, float y, float z, float r, float accuracy)
    {
        GL.PushMatrix();

        float stride = r * accuracy;
        float size = 1 / accuracy;
        float x1 = x, x2 = x, y1 = 0, y2 = 0;
        float x3 = x, x4 = x, y3 = 0, y4 = 0;

        double squareDe;
        squareDe = r * r - Math.Pow(x - x1, 2);
        squareDe = squareDe > 0 ? squareDe : 0;
        y1 = (float)(y + Math.Sqrt(squareDe));
        squareDe = r * r - Math.Pow(x - x1, 2);
        squareDe = squareDe > 0 ? squareDe : 0;
        y2 = (float)(y - Math.Sqrt(squareDe));
        for (int i = 0; i < size; i++)
        {
            x3 = x1 + stride;
            x4 = x2 - stride;
            squareDe = r * r - Math.Pow(x - x3, 2);
            squareDe = squareDe > 0 ? squareDe : 0;
            y3 = (float)(y + Math.Sqrt(squareDe));
            squareDe = r * r - Math.Pow(x - x4, 2);
            squareDe = squareDe > 0 ? squareDe : 0;
            y4 = (float)(y - Math.Sqrt(squareDe));

            //绘制线段
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);
            GL.Vertex(new Vector3(x1 / Screen.width, y1 / Screen.height, z));
            GL.Vertex(new Vector3(x3 / Screen.width, y3 / Screen.height, z));
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);
            GL.Vertex(new Vector3(x2 / Screen.width, y1 / Screen.height, z));
            GL.Vertex(new Vector3(x4 / Screen.width, y3 / Screen.height, z));
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);
            GL.Vertex(new Vector3(x1 / Screen.width, y2 / Screen.height, z));
            GL.Vertex(new Vector3(x3 / Screen.width, y4 / Screen.height, z));
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);
            GL.Vertex(new Vector3(x2 / Screen.width, y2 / Screen.height, z));
            GL.Vertex(new Vector3(x4 / Screen.width, y4 / Screen.height, z));
            GL.End();

            x1 = x3;
            x2 = x4;
            y1 = y3;
            y2 = y4;
        }
        GL.PopMatrix();
    }

    private static void DrawRectangle(Vector3 position, float linewidth, Material mat)
    {
        //linewidth *= 2f;
        mat.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.QUADS);
        //GL.Color(Color.cyan);
        GL.Vertex(position + new Vector3(-linewidth, -linewidth, 0)); //左下角
        GL.Vertex(position + new Vector3(-linewidth, linewidth, 0)); //左上角
        GL.Vertex(position + new Vector3(linewidth, linewidth, 0));//右上角
        GL.Vertex(position + new Vector3(linewidth, -linewidth, 0));//右下角

        GL.End();
        GL.PopMatrix();
    }

    //自适应texture到rawImage上
    public static void AutoFill(Texture2D tx, RectTransform rt)
    {
        // screen: w 512 h 294 自适应显示到canvas上
        float sx = rt.sizeDelta.x;
        float sy = rt.sizeDelta.y;
        //float sx = 512, sy = 294;
        float aspect = sx / sy;
        if (tx.height >= tx.width && tx.height > sy)
        {
            float ratio = tx.height / sy;
            sx = tx.width / ratio;
        }
        else if (tx.width > tx.height && tx.width > sx)
        {
            if (tx.width < aspect * tx.height)
            {

                float ratio = tx.height / sy;
                sx = tx.width / ratio;
            }
            else
            {
                float ratio = tx.width / sx;
                sy = tx.height / ratio;
            }
        }
        rt.sizeDelta = new Vector2(sx, sy);
    }

    public static byte[] GetImageByte(string imagePath)
    {
        FileStream files = new FileStream(imagePath, FileMode.Open);
        byte[] imgByte = new byte[files.Length];
        files.Read(imgByte, 0, imgByte.Length);
        files.Close();
        return imgByte;
    }
    public static void SetInnerCamera(Camera camera, Rect rect, RectTransform rt, float expandScale = 1.1f)
    {
        SetInnerCamera(camera, rect.width, rect.height, rt, expandScale);
    }
    public static void SetInnerCamera(Camera camera, float width, float height, RectTransform rt, float expandScale = 1.1f)
    {
        camera.transform.position = new Vector3(width / 2, height / 2, -10);
        if (width / height > rt.sizeDelta.x / rt.sizeDelta.y) //小幕布的尺寸大小
            camera.orthographicSize = expandScale * width / 2;
        else
            camera.orthographicSize = expandScale * height / 2;
    }
    static float GetBoneRadius(Anima2D.Bone2D bone)
    {
        return Mathf.Min(bone.localLength / 20f, 0.125f * 1f);
    }
    public static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float widthP1, float widthP2, Color color)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

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
    static void DrawSolidArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, Color color)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        if (s_array == null)
        {
            s_array = new Vector3[60];
        }

        SetDiscSectionPoints(s_array, 60, normal, from, angle);
        //handleWireMaterial.SetPass(0);
        GL.PushMatrix();
        //GL.MultMatrix(Handles.matrix);
        GL.Begin(4);
        for (int i = 1; i < s_array.Length; i++)
        {
            GL.Color(color);
            GL.Vertex(center);
            GL.Vertex(center + s_array[i - 1] * radius);
            GL.Vertex(center + s_array[i] * radius);
        }
        GL.End();
        GL.PopMatrix();
    }
    public static void DrawBoneBody(Anima2D.Bone2D bone)
    {
        DrawBoneBody(bone, bone.color);
    }
    static void DrawBoneBody(Anima2D.Bone2D bone, Color color)
    {
        //Handles.matrix = bone.transform.localToWorldMatrix;
        DrawBoneBody(Vector3.zero, bone.localEndPosition, GetBoneRadius(bone), color);
    }
    static void DrawBoneBody(Vector3 position, Vector3 endPosition, float radius, Color color)
    {
        Vector3 distance = position - endPosition;

        if (distance.magnitude > radius && color.a > 0f)
        {
            DrawLine(position, endPosition, Vector3.back, 2f * radius, 0f, color);
            DrawSolidArc(position, Vector3.back, Vector3.Cross(endPosition - position, Vector3.forward), 180f, radius, color);
        }
    }
    static void SetDiscSectionPoints(Vector3[] dest, int count, Vector3 normal, Vector3 from, float angle)
    {
        from.Normalize();
        Quaternion rotation = Quaternion.AngleAxis(angle / (float)(count - 1), normal);
        Vector3 vector = from;
        for (int i = 0; i < count; i++)
        {
            dest[i] = vector;
            vector = rotation * vector;
        }
    }

    public static void DrawBoneCap(Anima2D.Bone2D bone)
    {
        Color color = bone.color * 0.25f;
        color.a = 1f;
        DrawBoneCap(bone, color);
    }
    public static void DrawBoneCap(Anima2D.Bone2D bone, Color color)
    {
        //Handles.matrix = bone.transform.localToWorldMatrix;
        DrawBoneCap(Vector3.zero, GetBoneRadius(bone), color);
    }
    static void DrawBoneCap(Vector3 position, float radius, Color color)
    {
        //Handles.color = color;
        DrawCircle(position, radius * 0.65f);
    }
    static void DrawCircle(Vector3 center, float radius)
    {
        DrawCircle(center, radius, 0f);
    }
    public static void DrawCircle(Vector3 center, float radius, float innerRadius)
    {
        innerRadius = Mathf.Clamp01(innerRadius);

        if (s_circleArray == null)
        {
            s_circleArray = new Vector3[12];
            SetDiscSectionPoints(s_circleArray, 12, Vector3.forward, Vector3.right, 360f);
        }

        //Shader.SetGlobalColor("_HandleColor", Handles.color * new Color(1f, 1f, 1f, 0.5f));
        Shader.SetGlobalFloat("_HandleSize", 1f);
        //handleWireMaterial.SetPass(0);
        GL.PushMatrix();
        //GL.MultMatrix(Handles.matrix);
        GL.Begin(4);
        for (int i = 1; i < s_circleArray.Length; i++)
        {
            //GL.Color(Handles.color);
            GL.Vertex(center + s_circleArray[i - 1] * radius * innerRadius);
            GL.Vertex(center + s_circleArray[i - 1] * radius);
            GL.Vertex(center + s_circleArray[i] * radius);
            GL.Vertex(center + s_circleArray[i - 1] * radius * innerRadius);
            GL.Vertex(center + s_circleArray[i] * radius);
            GL.Vertex(center + s_circleArray[i] * radius * innerRadius);
        }
        GL.End();
        GL.PopMatrix();
    }
}
