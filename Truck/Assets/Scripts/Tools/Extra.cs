using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Text;
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

    public static Rect rect;
    public static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float width, Color color, Material mat)
    {
        DrawLine(p1, p2, normal, width, width, color, mat);
    }

    private static void DrawLine(Vector3 p1, Vector3 p2, Vector3 normal, float widthP1, float widthP2, Color color, Material mat)
    {
        Vector3 right = Vector3.Cross(normal, p2 - p1).normalized;
        mat.SetPass(0);
        GL.PushMatrix();
        //GL.LoadOrtho();
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

    public static void InitCamera(Camera camera, float layer) {
        camera.transform.position = new Vector3(0, 0, layer);
        camera.farClipPlane = 0.1f;
        camera.nearClipPlane = -0.1f;
    }
    static List<Color> mColors = new List<Color>();

    public static Color GetRingColor(int index)
    {
        index = Mathf.Clamp(index, 0, index);
        index %= mColors.Count;

        return mColors[index];
    }

    static Extra()
    {
        float hueAngleStep = Mathf.Clamp(45f, 1f, 360f);
        float hueLoopOffset = Mathf.Clamp(20f, 1f, 360f);

        int numColors = (int)(360f / hueAngleStep) * (int)(360f / hueLoopOffset);

        mColors.Capacity = numColors;

        for (int i = 0; i < numColors; ++i)
        {
            float hueAngle = i * hueAngleStep;
            float loops = (int)(hueAngle / 360f);
            float hue = ((hueAngle % 360f + (loops * hueLoopOffset % 360f)) / 360f);

#if UNITY_5_0_0 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
				mColors.Add(EditorGUIUtility.HSVToRGB(hue, 1f, 1f));
#else
            mColors.Add(Color.HSVToRGB(hue, 1f, 1f));

#endif
        }
    }
    public static void SetInnerCamera(Camera camera, float layer, Rect rect, RectTransform rt, float expandScale = 1.1f)
    {
        SetInnerCamera(camera,layer,rect.width, rect.height, rt, expandScale);
    }
    public static void SetInnerCamera(Camera camera, float layer,float width, float height, RectTransform rt,float expandScale = 1.1f)
    {
        camera.transform.position = new Vector3(width / 2, -height / 2, layer);
        camera.farClipPlane = 0.1f;
        camera.nearClipPlane = -0.1f;
        if (width / height > rt.sizeDelta.x / rt.sizeDelta.y) //小幕布的尺寸大小
            camera.orthographicSize = expandScale * width / 2;
        else
            camera.orthographicSize = expandScale * height / 2;
    }

    static float GetBoneRadius(Bone2D bone)
    {
        return Mathf.Min(bone.localLength / 20f, 0.125f * 1f);
    }
    static void DrawSolidArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, Color color,Material mat)
    {
        if (s_array == null)
        {
            s_array = new Vector3[12];
        }

        SetDiscSectionPoints(s_array, 12, normal, from, angle);
        mat.SetPass(0);

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
    public static void DrawBoneBody(Bone2D bone, Material mat,float layer)
    {
        DrawBoneBody(bone, bone.color,mat, layer);
    }
    public static void DrawBoneBody(Bone2D bone, Color color, Material mat,float layer)
    {
        //Handles.matrix = bone.transform.localToWorldMatrix;
        Vector3 begin = bone.globalstartPosition;
        Vector3 end = bone.globalendPosition;
        end.z = begin.z =layer;
        DrawBoneBody(begin , end, GetBoneRadius(bone), color, mat);
    }
    static void DrawBoneBody(Vector3 position, Vector3 endPosition, float radius, Color color, Material mat)
    {
        Vector3 distance = position - endPosition;
        if (distance.magnitude > radius && color.a > 0f)
        {
            Color outline = color * 0.5f;
            outline.a = 1;
            DrawLine(position, endPosition, Vector3.back, 2f * radius*1.2f, 0f, outline, mat);
            DrawLine(position, endPosition, Vector3.back, 2f * radius, 0f,color, mat);
            DrawSolidArc(position, Vector3.back, Vector3.Cross(endPosition - position, Vector3.forward), 180f, radius*1.2f, outline, mat);
            DrawSolidArc(position, Vector3.back, Vector3.Cross(endPosition - position, Vector3.forward), 180f, radius, color, mat);
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

    public static void DrawBoneCap(Bone2D bone, Material mat,float layer)
    {
        Color color = bone.color * 0.25f;
        color.a = 1f;
        DrawBoneCap(bone, color, mat,layer);
    }
    public static void DrawBoneCap(Bone2D bone, Color color, Material mat,float layer)
    {
        //Handles.matrix = bone.transform.localToWorldMatrix;
        Vector3 center = bone.globalstartPosition;
        center.z = layer;
        DrawBoneCap(center, GetBoneRadius(bone), color, mat);
    }
    static void DrawBoneCap(Vector3 position, float radius, Color color, Material mat)
    {
        //Handles.color = color;
        DrawCircle(position, radius * 0.65f,color, mat);
    }
    static void DrawCircle(Vector3 center, float radius,Color color, Material mat)
    {
        DrawCircle(center, radius, 0f,color, mat);
    }
    public static void DrawCircle(Vector3 center, float radius, float innerRadius, Color color,Material mat)
    {
        innerRadius = Mathf.Clamp01(innerRadius);

        if (s_circleArray == null)
        {
            s_circleArray = new Vector3[12];
            SetDiscSectionPoints(s_circleArray, 12, Vector3.forward, Vector3.right, 360f);
        }

        //Shader.SetGlobalColor("_HandleColor", Handles.color * new Color(1f, 1f, 1f, 0.5f));
        Shader.SetGlobalFloat("_HandleSize", 1f);
        mat.SetPass(0);

        GL.PushMatrix();
        //GL.MultMatrix(Handles.matrix);
        GL.Begin(4);
        for (int i = 1; i < s_circleArray.Length; i++)
        {
            GL.Color(color);
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

    public static void  DrawPie(Vector3 position, BoneWeight boneWeight, float pieSize, Material mat,List<Color> colors)
    {
        int boneIndex = boneWeight.boneIndex0;
        float angleStart = 0f;
        float angle = 0f;
        float outlineScale = 1.2f;
        Color color;
        
        DrawSolidArc(position, Vector3.forward, Vector3.up, 360, pieSize* outlineScale, Color.black, mat);
        if (boneIndex >= 0)
        {
            angleStart = 0f;
            angle = Mathf.Lerp(0f, 360f, boneWeight.weight0);
            color = colors[boneWeight.boneIndex0];
           // mat.color = color;
            DrawSolidArc(position, Vector3.forward, Vector3.up, angle, pieSize,color,mat);
        }
         
        boneIndex = boneWeight.boneIndex1;

        if (boneIndex >= 0)
        {
            angleStart += angle;
            angle = Mathf.Lerp(0f, 360f, boneWeight.weight1);
            color = colors[boneWeight.boneIndex1];
           // mat.color = color;
            DrawSolidArc(position, Vector3.forward, Quaternion.AngleAxis(angleStart, Vector3.forward) * Vector3.up, angle,pieSize, color, mat);
        }

        boneIndex = boneWeight.boneIndex2;

        if (boneIndex >= 0)
        {
            angleStart += angle;
            angle = Mathf.Lerp(0f, 360f, boneWeight.weight2);
            color = colors[boneWeight.boneIndex2];
          //  mat.color = color;
            DrawSolidArc(position, Vector3.forward, Quaternion.AngleAxis(angleStart, Vector3.forward) * Vector3.up, angle, pieSize, color, mat);
        }

        boneIndex = boneWeight.boneIndex3;

        if (boneIndex >= 0)
        {
            angleStart += angle;
            angle = Mathf.Lerp(0f, 360f, boneWeight.weight3);
            color = colors[boneWeight.boneIndex3];
         //   mat.color = color;
            DrawSolidArc(position, Vector3.forward, Quaternion.AngleAxis(angleStart, Vector3.forward) * Vector3.up, angle, pieSize, color, mat);
        }
    }

    public static int GetFilesCountInPath(string path, string type)
    {
        int count = 0;
        if (!string.IsNullOrEmpty(path))
        {
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] allFiles = root.GetFiles().Where(f => f.Name.EndsWith(type)).ToArray();
            count = allFiles.Length;
        }

        return count;
    }

    //处理队列中的(x,y,score)点信息，以层次遍历树结构的方式，构建骨架信息--for character
    public static void ConvertKeyPoints2Skeleton(OpenPose.MultiArray<float> keypoints,
        GameObject fbone, List<Bone2D> BonesInHierarchy)
    {
        if (keypoints.Count <= 0 || fbone == null)
            return;
        if (BonesInHierarchy.Count > 0)
            BonesInHierarchy.Clear();

        List<Vector2> points;
        ExtractXYPointsInXYScoreFormat(keypoints, out points);


        //3.将点两两配对转化为Bone2D，确定Bone2D的两个端点的位置和length以及rotation
        //（根据OpenPose转化的骨骼图和我们想要的骨骼属性结构）
        // 考虑关键点为0的情况--要不先手动删除空bone物体把...
        int rootpointsindex = 1;
        int[] firstlayerpointsindex = new int[5] { 0, 2, 5, 8, 11 };
        int[] ffirstlayerpointsindex = new int[4] { 2, 5, 8, 11 };
        int[] secondlayerpointsindex = new int[4] { 3, 6, 9, 12 };
        //14个关键点，对应13个骨骼
        //其实应该叫锁骨、上胳膊、下胳膊这样，但是你懂就好
        string[] skeletonsName =
        {
            "neck_bone", //0
            " ",  //1
            "rshoulder_bone", //2
            "relbow_bone", //3
            "rwrist_bone", //4
            "lshoulder_bone", //5
            "lelbow_bone", //6
            "lwrist_bone", //7
            "rhip_bone", //8
            "rknee_bone", //9
            "rankle_bone", //10
            "lhip_bone", //11
            "lknee_bone", //12
            "lankle_bone" //13
        };
        fbone.transform.position = Vector3.zero; //一定要为0，否则程序上会有不必要的麻烦
        for (int i = 0; i < points.Count; i++)
        {
            //3.0.root根节点到第一层级关键点的骨骼
            if (i == rootpointsindex)
            {
                for (int j = 0; j < firstlayerpointsindex.Length; j++)
                {
                    //转化为骨骼，零散到hierarchy,因为我是根据两个关键点求Bone2D的，所以Bone2D的长度、旋转角都应该求出来更新其值
                    GameObject bone = new GameObject(skeletonsName[firstlayerpointsindex[j]]);
                    bone.tag = "bone2D";
                    bone.transform.parent = fbone.transform;
                    Bone2D boneComponent = bone.AddComponent<Bone2D>();
                    BonesInHierarchy.Add(boneComponent);

                    if (points[i] == Vector2.zero || points[firstlayerpointsindex[j]] == Vector2.zero)
                    {
                        bone.transform.position = Vector3.zero;
                        boneComponent.globalendPosition = Vector3.zero;
                        boneComponent.globalstartPosition = Vector3.zero;
                        ResetZeroBone2DProperty(bone);
                    }
                    else
                    {
                        bone.transform.position = new Vector3(points[i].x, points[i].y, 0); //pixelsToUnit
                        boneComponent.globalstartPosition = bone.transform.position;
                        boneComponent.globalendPosition = new Vector3(points[firstlayerpointsindex[j]].x, points[firstlayerpointsindex[j]].y, 0);
                        UpdateBone2DProperty(bone);
                    }

                }

            }
            //3.1. 第一层级关键点到第二层级关键点的骨骼...第二层级关键点到第三层级关键点的骨骼
            else if (Array.IndexOf(ffirstlayerpointsindex, i) != -1 || Array.IndexOf(secondlayerpointsindex, i) != -1)
            {
                //转化为骨骼，零散到hierarchy,因为我是根据两个关键点求Bone2D的，所以Bone2D的长度、旋转角都应该求出来更新其值
                GameObject bone = new GameObject(skeletonsName[i + 1]);
                bone.tag = "bone2D";
                bone.transform.parent = fbone.transform;
                Bone2D boneComponent = bone.AddComponent<Bone2D>();
                BonesInHierarchy.Add(boneComponent);
                //没有检测到关键点的情况,直接归为坐标原点
                if (points[i] == Vector2.zero || points[i + 1] == Vector2.zero)
                {
                    bone.transform.position = Vector3.zero;
                    boneComponent.globalendPosition = Vector3.zero;
                    boneComponent.globalstartPosition = Vector3.zero;
                    ResetZeroBone2DProperty(bone);
                }
                else
                {
                    bone.transform.position = new Vector3(points[i].x, points[i].y, 0);
                    boneComponent.globalendPosition = new Vector3(points[i + 1].x, points[i + 1].y, 0);
                    boneComponent.globalstartPosition = bone.transform.position;
                    UpdateBone2DProperty(bone);
                }
            }
        }
    }

    static void ResetZeroBone2DProperty(GameObject bone)
    {
        Bone2D boneComponent = bone.GetComponent<Bone2D>();
        boneComponent.localLength = 0;
    }
    //3.2.根据骨骼的两个端点，计算其旋转角(矩阵),长度，更新端点值让其能够在Scene场景中的绘制也随之跟新
    static void UpdateBone2DProperty(GameObject bone)
    {
        Quaternion l_deltaRotation = Quaternion.identity;
        //先看看BoneUtils类中有没有现成的接口--似乎都不太现成，所以先自己写吧
        Bone2D boneComponent = bone.GetComponent<Bone2D>();
        boneComponent.localLength = (boneComponent.globalstartPosition - boneComponent.globalendPosition).magnitude;
        Vector2 localPosition = new Vector2(boneComponent.globalendPosition.x - boneComponent.globalstartPosition.x,
                                            boneComponent.globalendPosition.y - boneComponent.globalstartPosition.y);
        float angle = Mathf.Atan2(localPosition.y, localPosition.x) * Mathf.Rad2Deg;
        l_deltaRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        bone.transform.localRotation *= l_deltaRotation;
    }

    //拿到角色的pose
    public static void GetCharacterPose(out Pose characterPose)
    {
        characterPose = null;
        //直接拿keypoints来转换为pose
        if (CharacterPreprocessing.keypoints.Count <= 0)
            return;

        List<Vector2> points;
        ExtractXYPointsInXYScoreFormat(CharacterPreprocessing.keypoints, out points);

        int rootpointsindex = 1;
        int[] firstlayerpointsindex = new int[5] { 0, 2, 5, 8, 11 };
        int[] ffirstlayerpointsindex = new int[4] { 2, 5, 8, 11 };
        int[] secondlayerpointsindex = new int[4] { 3, 6, 9, 12 };
        string[] skeletonsName =
        {
            "neck_bone", //0
            " ",  //1
            "rshoulder_bone", //2
            "relbow_bone", //3
            "rwrist_bone", //4
            "lshoulder_bone", //5
            "lelbow_bone", //6
            "lwrist_bone", //7
            "rhip_bone", //8
            "rknee_bone", //9
            "rankle_bone", //10
            "lhip_bone", //11
            "lknee_bone", //12
            "lankle_bone" //13
        };
        List<SkeletonLine> bones = new List<SkeletonLine>();
        for (int i = 0; i < points.Count; i++)
        {
            //3.0.root根节点到第一层级关键点的骨骼
            if (i == rootpointsindex)
            {
                for (int j = 0; j < firstlayerpointsindex.Length; j++)
                {
                    SkeletonLine skeletonLine = new SkeletonLine(points[i], points[firstlayerpointsindex[j]]);
                    skeletonLine.m_name = skeletonsName[firstlayerpointsindex[j]];
                    bones.Add(skeletonLine);

                    if (points[i] == Vector2.zero || points[firstlayerpointsindex[j]] == Vector2.zero)
                    {
                        skeletonLine.m_startPoint = Vector2.zero;
                        skeletonLine.m_startPoint = Vector2.zero;
                    }
                }

            }
            //3.1. 第一层级关键点到第二层级关键点的骨骼...第二层级关键点到第三层级关键点的骨骼
            else if (Array.IndexOf(ffirstlayerpointsindex, i) != -1 || Array.IndexOf(secondlayerpointsindex, i) != -1)
            {
                SkeletonLine skeletonLine = new SkeletonLine(points[i], points[i + 1]);
                skeletonLine.m_name = skeletonsName[i + 1];
                bones.Add(skeletonLine);

                //没有检测到关键点的情况,直接归为坐标原点
                if (points[i] == Vector2.zero || points[i + 1] == Vector2.zero)
                {
                    skeletonLine.m_startPoint = Vector2.zero;
                    skeletonLine.m_startPoint = Vector2.zero;
                }
            }
        }
        //Pose pose = new Pose(bones);
        characterPose = new Pose(bones);
    }
    //读取文件
    public static string ReadFile(string fileName)
    {
        StringBuilder str = new StringBuilder();
        using (FileStream fs = File.OpenRead(fileName))
        {
            long left = fs.Length;
            int maxLength = 100;//每次读取的最大长度  
            int start = 0;//起始位置  
            int num = 0;//已读取长度  
            while (left > 0)
            {
                byte[] buffer = new byte[maxLength];//缓存读取结果  
                char[] cbuffer = new char[maxLength];
                fs.Position = start;//读取开始的位置  
                num = 0;
                if (left < maxLength)
                {
                    num = fs.Read(buffer, 0, Convert.ToInt32(left));
                }
                else
                {
                    num = fs.Read(buffer, 0, maxLength);
                }
                if (num == 0)
                {
                    break;
                }
                start += num;
                left -= num;
                str = str.Append(Encoding.UTF8.GetString(buffer));
            }
        }
        return str.ToString();
    }
    //处理Json文件--自定义的json文件内容
    public static void ParseJson(string jsonData, ref OpenPose.MultiArray<float> keypoints)
    {
        Dictionary<string, System.Object> pose = Ps2D.MiniJSON.Json.Deserialize(jsonData) as Dictionary<string, System.Object>;
        //Dictionary<string, Object> pose0 = Ps2D.MiniJSON.Json.Deserialize(pose["pose_0"]) as Dictionary<string, Object>;
        Dictionary<string, System.Object> pose0 = pose["pose_0"] as Dictionary<string, System.Object>;
        List<System.Object> keypointsOb = pose0["data"] as List<System.Object>; //不知道这样装箱拆箱各种强制转换会不会丢失数据精度
        //keypoints = new OpenPose.MultiArray<float>();
        foreach (var keypoint in keypointsOb)
        {
            //keypoints.Add((float)keypoint);
            float fnum = 0;
            float.TryParse(keypoint.ToString(), out fnum);
            keypoints.Add(fnum);
        }
    }

    //读取路径下的所有json文件内容，到一个队列里面
    public static void ParseJson(string folderPath, Queue<OpenPose.MultiArray<float>> keyPointsQueue)
    {
        if (!string.IsNullOrEmpty(folderPath))
        {
            keyPointsQueue.Clear();
            DirectoryInfo root = new DirectoryInfo(folderPath);
            FileInfo[] allFiles = root.GetFiles().Where(f => f.Name.EndsWith("json")).ToArray();

            for (int i = 0; i < allFiles.Length; i+= (int)VideoSliderController.instance.sliderInterval.value)
            {
                OpenPose.MultiArray<float> keypoints = new OpenPose.MultiArray<float>();
                ParseJson(ReadFile(allFiles[i].FullName), ref keypoints);
                keyPointsQueue.Enqueue(keypoints);
            }

        }
    }

    //处理队列中的(x,y,score)点信息，以层次遍历树结构的方式，构建骨架信息到新队列--for video
    public static void IterateConvertKeyPoints2Pose
        (Queue<OpenPose.MultiArray<float>> keyPointsQueue, ref Queue<Pose> skeletonQueue)
    {
        int rootpointsindex = 1;
        int[] firstlayerpointsindex = new int[5] { 0, 2, 5, 8, 11 };
        int[] ffirstlayerpointsindex = new int[4] { 2, 5, 8, 11 };
        int[] secondlayerpointsindex = new int[4] { 3, 6, 9, 12 };
        string[] skeletonsName =
        {
            "neck_bone", //0
            " ",  //1
            "rshoulder_bone", //2
            "relbow_bone", //3
            "rwrist_bone", //4
            "lshoulder_bone", //5
            "lelbow_bone", //6
            "lwrist_bone", //7
            "rhip_bone", //8
            "rknee_bone", //9
            "rankle_bone", //10
            "lhip_bone", //11
            "lknee_bone", //12
            "lankle_bone" //13
        };
        skeletonQueue.Clear();

        while (keyPointsQueue.Count > 0)
        {
            OpenPose.MultiArray<float> keypoints = keyPointsQueue.Dequeue(); //注意已经出队了

            List<Vector2> points;
            ExtractXYPointsInXYScoreFormat(keypoints, out points);

            List<SkeletonLine> bones = new List<SkeletonLine>();

            for (int i = 0; i < points.Count; i++)
            {
                //3.0.root根节点到第一层级关键点的骨骼
                if (i == rootpointsindex)
                {
                    for (int j = 0; j < firstlayerpointsindex.Length; j++)
                    {
                        SkeletonLine skeletonLine = new SkeletonLine(points[i], points[firstlayerpointsindex[j]]);
                        skeletonLine.m_name = skeletonsName[firstlayerpointsindex[j]];
                        bones.Add(skeletonLine);

                        if (points[i] == Vector2.zero || points[firstlayerpointsindex[j]] == Vector2.zero)
                        {
                            skeletonLine.m_startPoint = Vector2.zero;
                            skeletonLine.m_startPoint = Vector2.zero;
                        }
                    }

                }
                //3.1. 第一层级关键点到第二层级关键点的骨骼...第二层级关键点到第三层级关键点的骨骼
                else if (Array.IndexOf(ffirstlayerpointsindex, i) != -1 || Array.IndexOf(secondlayerpointsindex, i) != -1)
                {
                    SkeletonLine skeletonLine = new SkeletonLine(points[i], points[i + 1]);
                    skeletonLine.m_name = skeletonsName[i + 1];
                    bones.Add(skeletonLine);

                    //没有检测到关键点的情况,直接归为坐标原点
                    if (points[i] == Vector2.zero || points[i + 1] == Vector2.zero)
                    {
                        skeletonLine.m_startPoint = Vector2.zero;
                        skeletonLine.m_startPoint = Vector2.zero;
                    }
                }
            }
            Pose pose = new Pose(bones);
            skeletonQueue.Enqueue(pose);
        }

    }


    //将视频中归一化的pose，缩放至character原尺寸大小
    public static void IterateConvertKeyPoints2CalibratedPose
        (Queue<OpenPose.MultiArray<float>> _keyPointsQueue, ref Queue<Pose> calibratedSkeletonQueue, bool IsInitFrames = true)
    {
        Queue<OpenPose.MultiArray<float>> keyPointsQueue = new Queue<OpenPose.MultiArray<float>>();
        //如果是true,代表keyPointsQueue中存储的关键点所属的帧是无间隔的，这时要把他们重新按照UI上的帧间隔来存储
        if (IsInitFrames)
        {
            
            int starFrame = (int)VideoSliderController.instance.sliderStartFrame.value;
            int interval = (int)VideoSliderController.instance.sliderInterval.value;

            List<OpenPose.MultiArray<float>> tmp = _keyPointsQueue.ToList();

            for (int i = starFrame; i < tmp.Count; i += interval)
            {
                keyPointsQueue.Enqueue(tmp[i]);
            }
        }else
        {
            keyPointsQueue = _keyPointsQueue;
        }
        
        //获取CharacterPreprocessing中的Bone2D
        int boneCount = CharacterPreprocessing.BonesInHierarchy.Count;
        if (boneCount == 0)
            return;

        float[] boneLengths = new float[boneCount];
        string[] boneNames = new string[boneCount];
        for (int i = 0; i < boneCount; i++)
        {
            boneLengths[i] = CharacterPreprocessing.BonesInHierarchy[i].localLength;
            boneNames[i] = CharacterPreprocessing.BonesInHierarchy[i].name;
        }
        float[] ratio = new float[boneCount];

        //转化keypoints到校准到pose对象
        int rootpointsindex = 1;
        int[] firstlayerpointsindex = new int[5] { 0, 2, 5, 8, 11 };
        int[] ffirstlayerpointsindex = new int[4] { 2, 5, 8, 11 };
        int[] secondlayerpointsindex = new int[4] { 3, 6, 9, 12 };
        string[] skeletonsName =
        {
            "neck_bone", //0
            " ",  //1
            "rshoulder_bone", //2
            "relbow_bone", //3
            "rwrist_bone", //4
            "lshoulder_bone", //5
            "lelbow_bone", //6
            "lwrist_bone", //7
            "rhip_bone", //8
            "rknee_bone", //9
            "rankle_bone", //10
            "lhip_bone", //11
            "lknee_bone", //12
            "lankle_bone" //13
        };

        calibratedSkeletonQueue.Clear();
        int index = 0;
        while (keyPointsQueue.Count > 0)
        {
            OpenPose.MultiArray<float> keypoints = keyPointsQueue.Dequeue(); //注意是否已经出队了
            List<Vector2> points;
            ExtractXYPointsInXYScoreFormat(keypoints, out points);

            List<SkeletonLine> bones = new List<SkeletonLine>();
            //第一帧
            if (index == 0)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    //3.0.root根节点到第一层级关键点的骨骼
                    if (i == rootpointsindex)
                    {
                        for (int j = 0; j < firstlayerpointsindex.Length; j++)
                        {
                            SkeletonLine skeletonLine = new SkeletonLine(points[i], points[firstlayerpointsindex[j]]);
                            skeletonLine.m_name = skeletonsName[firstlayerpointsindex[j]];
                            bones.Add(skeletonLine);

                            if (points[i] == Vector2.zero || points[firstlayerpointsindex[j]] == Vector2.zero)
                            {
                                skeletonLine.m_startPoint = Vector2.zero;
                                skeletonLine.m_startPoint = Vector2.zero;
                            }
                        }

                    }
                    //3.1. 第一层级关键点到第二层级关键点的骨骼...第二层级关键点到第三层级关键点的骨骼
                    else if (Array.IndexOf(ffirstlayerpointsindex, i) != -1 || Array.IndexOf(secondlayerpointsindex, i) != -1)
                    {
                        SkeletonLine skeletonLine = new SkeletonLine(points[i], points[i + 1]);
                        skeletonLine.m_name = skeletonsName[i + 1];
                        bones.Add(skeletonLine);

                        //没有检测到关键点的情况,直接归为坐标原点
                        if (points[i] == Vector2.zero || points[i + 1] == Vector2.zero)
                        {
                            skeletonLine.m_startPoint = Vector2.zero;
                            skeletonLine.m_startPoint = Vector2.zero;
                        }
                    }
                }
                //求出骨骼比例
                for(int i=0;i<bones.Count;i++)
                {
                    float l1 = boneLengths[i]; //character initial
                    float l2 = bones[i].m_Length; //video normalized
                    if(l1==0 || l2 ==0)
                    {
                        ratio[i] = 1f;
                    }
                    else
                    {
                        ratio[i] = l1 / l2;
                    }
                }
            }
            bones.Clear();
            //第一帧及其余帧
            for(int i=0;i<points.Count;i++)
            {
                if (i == rootpointsindex)
                {
                    for (int j = 0; j < firstlayerpointsindex.Length; j++)
                    {
                        SkeletonLine skeletonLine = new SkeletonLine(points[i], points[firstlayerpointsindex[j]]);
                        skeletonLine.m_name = skeletonsName[firstlayerpointsindex[j]];
                        bones.Add(skeletonLine);
                    }
                }
                //第一层级关键点到第二层级关键点的骨骼...第二层级关键点到第三层级关键点的骨骼
                else if (System.Array.IndexOf(ffirstlayerpointsindex, i) != -1 || System.Array.IndexOf(secondlayerpointsindex, i) != -1)
                {
                    SkeletonLine skeletonLine = new SkeletonLine(points[i], points[i + 1]);
                    skeletonLine.m_name = skeletonsName[i + 1];
                    bones.Add(skeletonLine);
                }
            }
            //2.1 更新视频中每帧骨骼的长度
            for (int j = 0; j < bones.Count; j++)
            {
                bones[j].m_Length = ratio[j] * bones[j].m_Length;
            }
            //2.2 更新一级关键点--前5个骨骼
            for (int j = 0; j < 5; j++)
            {
                Vector2 delta = new Vector2(Mathf.Cos(bones[j].m_theta) * bones[j].m_Length,
                    Mathf.Sin(bones[j].m_theta) * bones[j].m_Length
                    );
                //sourceSkeleton[j].m_endPoint = sourceSkeleton[j].m_endPoint + delta;
                bones[j].m_endPoint = bones[j].m_startPoint + delta;
            }
            //2.3 以竖直结构更新二级三级关键点--4个骨骼+4个骨骼=8骨骼
            for (int j = 5; j < 13; j += 2)
            {
                bones[j].m_startPoint = bones[(j - 3) / 2].m_endPoint;
                Vector2 delta = new Vector2(Mathf.Cos(bones[j].m_theta) * bones[j].m_Length,
                    Mathf.Sin(bones[j].m_theta) * bones[j].m_Length
                    );
                bones[j].m_endPoint = bones[j].m_startPoint + delta;
            }
            for (int j = 6; j < 13; j += 2)
            {
                bones[j].m_startPoint = bones[j - 1].m_endPoint;
                Vector2 delta = new Vector2(Mathf.Cos(bones[j].m_theta) * bones[j].m_Length,
                   Mathf.Sin(bones[j].m_theta) * bones[j].m_Length
                   );
                bones[j].m_endPoint = bones[j].m_startPoint + delta;
            }
            //List<SkeletonLine> newbone = bones;
            Pose pose = new Pose(bones);
            calibratedSkeletonQueue.Enqueue(pose);
            index++;
        }
    }
    static void ExtractXYPointsInXYScoreFormat(OpenPose.MultiArray<float> keypoints, out List<Vector2> points)
    {
        
        //18*3=54个点，(x,y,score)的形式
        OpenPose.MultiArray<float> keypointsXY = new OpenPose.MultiArray<float>();

        //1.把54个值去掉score，只保留x，y值，得到36个值
        for (int i = 0; i < keypoints.Count; i++)
        {
            if (i % 3 == 0 || i % 3 == 1)
            {
                keypointsXY.Add(keypoints[i]);
            }
        }

        //2.我们只要前14个有用关键点，对应28个值，将值转化为点
        points = new List<Vector2>();
        for (int i = 0; i < keypointsXY.Count - 8; i += 2)
        {
            points.Add(new Vector2(keypointsXY[i] / 100f, -keypointsXY[i + 1] / 100f));//(x,y)
        }

    }
    
    //处理pose信息，求出运动偏移量,及二维角色的骨骼偏移量--for video
    public static void IterateGetPoseOffset()
    {

    }



}
