using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenPose;
using UnityEditor;
using System.IO;
using System.Runtime.InteropServices;
/// <summary>
/// 用来读入pose.json，拿到其中的关键点数据
/// </summary>

public class PoseJsonLoderUtils
{
    //我投降了，放弃了边读取文件边计算的想法，因为普通类想要访问Editor下的类没思路，然后parseJson这边又要用到Editor
    //root.GetFiles().CopyTo(f,0);
    public static List<FileInfo> jsonMotionFiles = new List<FileInfo>();
    public static void ParseJson(string jsonData)
    {
        Dictionary<string, Object> pose = Ps2D.MiniJSON.Json.Deserialize(jsonData) as Dictionary<string, Object>;
        //Dictionary<string, Object> pose0 = Ps2D.MiniJSON.Json.Deserialize(pose["pose_0"]) as Dictionary<string, Object>;
        Dictionary<string, Object> pose0 = pose["pose_0"] as Dictionary<string, Object>;
        List<Object> keypointsOb = pose0["data"] as List<Object>; //不知道这样装箱拆箱各种强制转换会不会丢失数据精度
        MultiArray<float> keypoints = new MultiArray<float>();
        foreach (var keypoint in keypointsOb)
        {
            //keypoints.Add((float)keypoint);
            float fnum = 0;
            float.TryParse(keypoint.ToString(), out fnum);
            keypoints.Add(fnum);
        }
        //删掉分数，只留下有用的点数据
        MultiArray<float> keypointsXY = new MultiArray<float>();
        //1.把54个值去掉score，只保留x，y值，得到36个值
        for (int i = 0; i < keypoints.Count; i++)
        {
            if (i % 3 == 0 || i % 3 == 1)
            {
                keypointsXY.Add(keypoints[i]);
            }
        }
        //2.我们只要前14个有用关键点，对应28个值，将值转化为点
        List<UnityEngine.Vector2> points = new List<UnityEngine.Vector2>();
        for (int i = 0; i < keypointsXY.Count - 8; i += 2)
        {
            points.Add(new UnityEngine.Vector2(keypointsXY[i] / 100f, -keypointsXY[i + 1] / 100f));//(x,y)
        }
        //3.把这14个关键点的列表放进队列里
        PoseRetargetUserScript.jsonMotionPointsSequence.Enqueue(points);
    }
    public static void ParseJson(UnityEngine.Object jsonData, int step, int startFrame, int endFrame)
    {
            PoseRetargetUserScript.jsonMotionPointsSequence.Clear();
            if(jsonMotionFiles.Count == 0 || jsonMotionFiles.Count == 1)
            {
                return;
            }
            foreach (FileInfo f in jsonMotionFiles)
            {
                ParseJson(ReadFile(f.FullName));
            }
            //触发PoseRetarget开始重定向
            PoseRetargetUserScript.ContollerPoseRetarget = true;
            PoseRetargetUserScript.jsonMotionSkeletonLineCelibratedSequence.Clear();
            PoseRetargetUserScript.OpJsonMotionPointsSequence();
    }

    public static List<FileInfo> GetJsonFileNames(UnityEngine.Object jsonData, int step, int startFrame, int endFrame)
    {
        jsonMotionFiles.Clear();
        //读取路径下的json文件
        string folder = AssetDatabase.GetAssetPath(jsonData);//把文件路径也当作了asset资源来看待？
        if (!string.IsNullOrEmpty(folder)) /*&& File.Exists(folder)*/
        {
            //展开该路径下的所有json文件，进行读取
            folder = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("Assets")) + folder;
            //spritePath = spritePath.Replace(@"/", @"\");
            //string directory = Path.GetDirectoryName(folder);
            DirectoryInfo root = new DirectoryInfo(folder);
            FileInfo[] allFiles = root.GetFiles().Where(f => f.Name.EndsWith(".json")).ToArray();
            //对文件按照名字排序
            //allFiles.OrderBy(f=>f.Name);
            //Array.Sort(allFiles, new FileNameSort()); // 对获取的文件名进行排序
            Array.Sort(allFiles, delegate (FileInfo x, FileInfo y) { return x.LastWriteTime.CompareTo(y.LastWriteTime); });

            //foreach (FileInfo f in allFiles)
            //{
            //    UnityEngine.Debug.Log(f.Name);
            //}
            //按照初始帧和结束帧以及步长
            int file_index = startFrame;
            while (file_index <= endFrame)
            {
                jsonMotionFiles.Add(allFiles[file_index - 1]);
                file_index += step;
            }
        }
        return jsonMotionFiles;
    }
    private static string ReadFile(string fileName)
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
}
public class FileNameSort : IComparable
{
    [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int StrCmpLogicalW(string param1, string param2);
    
    //前后文件名进行比较。
    public int Compare(object name1, object name2)
    {
        if (null == name1 && null == name2)
        {
            return 0;
        }
        if (null == name1)
        {
            return -1;
        }
        if (null == name2)
        {
            return 1;
        }
        return StrCmpLogicalW(name1.ToString(), name2.ToString());
    }
    public int CompareTo(object obj)
    {
        throw new NotImplementedException();
    }
}
