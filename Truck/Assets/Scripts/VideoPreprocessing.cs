using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

/// <summary>
/// 对视频进行预处理，包括提取运动，姿态检索
/// </summary>
public static class VideoPreprocessing
{
    public static Queue<OpenPose.MultiArray<float>> keyPointsQueue = new Queue<OpenPose.MultiArray<float>>();
    public static Queue<Pose> skeletonQueue = new Queue<Pose>();
    public static Queue<Pose> calibratedSkeletonQueue = new Queue<Pose>();

    public static string RenderImageSavePath = "";
    public static string JsonFileSavePath = "";

    public static void ResetPastVideoInfo()
    {
        keyPointsQueue.Clear();
        skeletonQueue.Clear();
        calibratedSkeletonQueue.Clear();
        RenderImageSavePath = "";
        JsonFileSavePath = "";

        DrawVideoPoseFrames.ResetPastVideoInfo();
    }
    public static VideoMotionExtractResult ExtractVideoMotion()
    {
        //OpenPose视频检测
        string videoPath = AVProVideoController.instance.videoPath;

        if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
        {
            //videoPath = System.IO.Path.GetDirectoryName(videoPath);
            string dir = videoPath.Substring(0, videoPath.LastIndexOf("/"));
            JsonFileSavePath = dir + "/keypoints";
            RenderImageSavePath = dir + "/images"; //默认路径是//格式

            int filecount = Extra.GetFilesCountInPath(RenderImageSavePath,".png");

            //判断视频检测帧序列是否已经存在于本地，如果已经存在，则不再启动OpenPose，直接读取视频帧png文件
            if (filecount > 0)
            {
                //读取渲染图像到展览区
                DrawVideoPoseFrames.InitVideoPoseInfo();
                return VideoMotionExtractResult.Success;
            }

            VideoOpenPoseScript.producerString = videoPath;
            VideoOpenPoseScript.folderSaveKeyPoints = JsonFileSavePath;
            VideoOpenPoseScript.folderSaveImage = RenderImageSavePath;

            return VideoMotionExtractResult.OpenPoseStart;
        }
        return VideoMotionExtractResult.VideoError;
    }

    //OpenPose调用接口
    public static VideoMotionExtractResult ExtractVideoMotionPoseDatum(OpenPose.OPDatum datum)
    {
        //存放的数据是从0帧到末尾帧，并且帧间隔是默认的1
        keyPointsQueue.Enqueue(datum.poseKeypoints); 

        //判断队列中的数据量是否达到video文件的上限，达到了以后才开始绘制，否则容易造成显示图像错乱
        float endframe = VideoSliderController.instance.sliderEndFrame.value;
        float startframe = VideoSliderController.instance.sliderStartFrame.value;
        float frameinterval = VideoSliderController.instance.sliderInterval.value;

        //已经把视频数据全部识别完，默认处理的视频帧没有帧间隔
        if(keyPointsQueue.Count >= endframe)
        {
            VideoOpenPoseScript.stopOpenPose = true;
            //要展示的图片大致已经存在
            if ((keyPointsQueue.Count - startframe) / frameinterval >= (endframe - startframe) / frameinterval)
            {
                Thread.Sleep(1000);
                DrawVideoPoseFrames.InitVideoPoseInfo();
            }
        }
        

        return VideoMotionExtractResult.Success;
    }

    public static VideoMotionRetrivalResult RetrivalVideoMotion()
    {
        //OpenPose实时运行获取到的视频帧数据
        if (keyPointsQueue.Count > 0)
        {
            //Extra.IterateConvertKeyPoints2Pose(keyPointsQueue,ref skeletonQueue);
            Extra.IterateConvertKeyPoints2CalibratedPose(keyPointsQueue, ref calibratedSkeletonQueue, true);
        }
        //没有启用OpenPose，本地已经有了json文件序列，直接读取，并赋值给keyPointsQueue
        else
        {
            Extra.ParseJson(JsonFileSavePath,keyPointsQueue);
            //Extra.IterateConvertKeyPoints2Pose(keyPointsQueue, ref skeletonQueue);
            Extra.IterateConvertKeyPoints2CalibratedPose(keyPointsQueue, ref calibratedSkeletonQueue, false);
        }

        //video frame i <---> character
        int index = FindSimilarFrameIndexWithCharacterInVideo();

        if (index != -1)
        {
            ChangeSliderValue(index);

            Console.Log("The most similiar frame index is " + index +". Suggest this frame");
            return VideoMotionRetrivalResult.Success;
        }
        return VideoMotionRetrivalResult.AlgorithmError;
    }
    static void ChangeSliderValue(int index)
    {
        //float targetframe = index * VideoSliderController.instance.sliderInterval.value;
        VideoSliderController.instance.sliderStartFrame.value = index;
        //Console.Log("the most similar pose in video frame is frame " + index);
    }

    //输出寻找过程中的Error，并且把那个合适的帧图片标红在可视化区域，更新UI
    //返回的是第几个数据，要想真正的得到视频帧索引，应该和帧间隔换算
    static int FindSimilarFrameIndexWithCharacterInVideo()
    {
        int flg = -1;
        float sumError = float.PositiveInfinity;
        int index = (int)VideoSliderController.instance.sliderStartFrame.value;

        Pose characterPose;
        Extra.GetCharacterPose(out characterPose);
        if (characterPose == null)
        {
            return -1;   
        }
        float[] errors = new float[calibratedSkeletonQueue.Count];
        int i = 0;
        Queue<Pose> tmpcalibratedSkeletonQueue = new Queue<Pose>(calibratedSkeletonQueue);

        //处理临时变量，不应该处理所拥有的全局性变量
        List<SkeletonLine> characterSkeleton = characterPose.m_bones;
        while (tmpcalibratedSkeletonQueue.Count > 0)
        {
            Pose pose = tmpcalibratedSkeletonQueue.Dequeue();
            //List<SkeletonLine> videoSkeleton = pose.m_bones;
            float error = 0f;
#if false
            for (int i = 0; i < videoSkeleton.Count; i++)
            {
                //判断此根骨骼的theta角误差
                error += UnityEngine.Mathf.Abs(characterSkeleton[i].m_theta - videoSkeleton[i].m_theta);
            }
#endif
            //2GPD算子：骨骼之间的theta、配对关节点之间的距离和fhy
            error += (characterPose.realgeoFeature - pose.realgeoFeature);
            error += (characterPose.pairgeoFeature - pose.pairgeoFeature);

            if (error < sumError)
            {
                sumError = error;
                flg = index;
            }
            index += (int)VideoSliderController.instance.sliderInterval.value;

            errors[i] = error;
            i++;
        }

        //添加实验：显示Error分布图表
        //TestShowLineChart(errors);
        return flg;
    }

    //实验结果，方便查看姿态检索时的error值的分布情况
    static void TestShowLineChart(float[] errors)
    {
        GameObject gameObject = new GameObject("errors");
        var chart = gameObject.GetComponent<XCharts.LineChart>();
        if (chart == null)
        {
            chart = gameObject.AddComponent<XCharts.LineChart>();
            chart.SetSize(580, 300);//代码动态添加图表需要设置尺寸，或直接操作chart.rectTransform
        }
        //设置标题
        chart.title.show = true;
        chart.title.text = "Pose Similiraty Error";
        //设置提示框和图例是否显示
        chart.tooltip.show = true;
        chart.legend.show = false;
        //设置是否使用双坐标轴和坐标轴类型
        chart.xAxes[0].show = true;
        chart.xAxes[1].show = false;
        chart.yAxes[0].show = true;
        chart.yAxes[1].show = false;
        chart.xAxes[0].type = XCharts.Axis.AxisType.Value;
        chart.yAxes[0].type = XCharts.Axis.AxisType.Value;

        //设置坐标轴分割线
        chart.xAxes[0].splitNumber = 10;
        chart.xAxes[0].boundaryGap = true;
        //清空数据，添加Line类型的Serie用于接收数据
        chart.RemoveData();
        chart.AddSerie(XCharts.SerieType.Line);

        for (int i = 0; i < errors.Length; i += 2)
        {
            chart.AddXAxisData("",i);
            chart.AddData(0, errors[i]);
            //chart.AddData(0, Random.Range(10, 20));
        }
    }
}


//定义关键点中的骨骼信息
public class SkeletonLine : ICloneable
{
    public Vector2 m_startPoint;
    public Vector2 m_endPoint;

    public string m_name;

    public float m_Length = 0.0f;

    public float m_theta; //斜率,用theta来表示

    public SkeletonLine(Vector2 point1, Vector2 point2)
    {
        m_startPoint = point1;
        m_endPoint = point2;
        m_Length = (m_endPoint - m_startPoint).magnitude;
        
        Vector2 delta = m_endPoint - m_startPoint;
        if (delta.x == 0)
        {
            if (delta.y > 0)
                m_theta = Mathf.Deg2Rad * 90; // 90°转换为弧度制 （顺时针90）
            if (delta.y < 0)
                m_theta = -Mathf.Deg2Rad * 90;
        }
        else
        {
            m_theta = Mathf.Atan2(delta.y, delta.x);
        }

    }
    public object Clone()
    {
        return this.MemberwiseClone(); //浅拷贝
    }
    //浅拷贝
    public SkeletonLine ShallowClone()
    {
        return this.Clone() as SkeletonLine;
    }
    //深拷贝
    public SkeletonLine DeepClone()
    {
        using (Stream objectStream = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(objectStream, this);
            objectStream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(objectStream) as SkeletonLine;
        }
    }
}

//骨骼偏移量
public class SkeletonLineOffset : ICloneable
{
    public Vector2 m_startOffset;
    public Vector2 m_endOffset;
    public SkeletonLineOffset(Vector2 offset1, Vector2 offset2)
    {
        m_startOffset = offset1;
        m_endOffset = offset2;
    }
    public object Clone()
    {
        return this.MemberwiseClone();
    }
    //浅拷贝
    public Pose ShallowClone()
    {
        return this.Clone() as Pose;
    }
    //深拷贝
    public Pose DeepClone()
    {
        using (Stream objectStream = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(objectStream, this);
            objectStream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(objectStream) as Pose;
        }
    }
}

//骨架 （姿态）
public class Pose : ICloneable
{
    public List<SkeletonLine> m_bones = new List<SkeletonLine>();
    public PairGeoFeature pairgeoFeature;
    public RealGeoFeature realgeoFeature;

    public Pose(List<SkeletonLine> bones)
    {
        m_bones = bones; //浅拷贝
        InitPairGeoFeature();
        InitRealGeoFeature();
    }
    public void InitPairGeoFeature()
    {
        //给定下肢的末端骨骼，上肢的末端骨骼，去求它们的几何特征
        //rankle_bone,lankle_bone,rwrist_bone,lwrist_bone
        if (m_bones.Count > 12)
        {
            pairgeoFeature = new PairGeoFeature(m_bones[10], m_bones[12], m_bones[6], m_bones[8]);
        }
        
    }
    public void InitRealGeoFeature()
    {
        //直接求骨架中的骨骼之间相连的夹角
        if(m_bones.Count > 12)
        {
            realgeoFeature = new RealGeoFeature(m_bones);
        }

    }
    public object Clone()
    {
        return this.MemberwiseClone(); //浅拷贝
    }
    //浅拷贝
    public Pose ShallowClone()
    {
        return this.Clone() as Pose;
    }
    //深拷贝
    public Pose DeepClone()
    {
        using (Stream objectStream = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(objectStream, this);
            objectStream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(objectStream) as Pose;
        }
    }
}
public class RealGeoFeature
{
    float[] theta = new float[0];
    public RealGeoFeature(List<SkeletonLine> m_bones)
    {
        theta = new float[m_bones.Count]; //一般而言，13根骨骼对应13个可以计算的夹角
        

        //遍历骨骼，求出对应的theta角度值: 和论文中的夹角有轻微出入
        for(int i = 0; i < m_bones.Count - 1; i++)
        {
            theta[i] = m_bones[i + 1].m_theta - m_bones[i].m_theta;
        }
    }
    public static float operator -(RealGeoFeature geo1,RealGeoFeature geo2)
    {
        float result = 0;
        for(int i = 0; i < geo1.theta.Length; i++)
        {
            result += UnityEngine.Mathf.Abs(geo1.theta[i] - geo2.theta[i]);
        }
        return result;
    }

    //public float Vector2VectorAngle(Vector2 v1,Vector2 v2)
    //{
    //    float angle;
    //    angle = UnityEngine.Mathf.Atan2(v2.y, v2.x) - UnityEngine.Mathf.Atan2(v1.y, v1.x);
    //    return angle;
    //}
}

public class PairGeoFeature
{
    float legDistance; //腿最末端距离
    float legDistance1;//腿次末端距离
    float armDistance; //胳膊最末端距离
    float armDistance1;//胳膊次末端距离

    float legTheta; //腿最末端旋转角
    float legTheta1; //腿次末端旋转角
    float armTheta; //胳膊最末端旋转角
    float armTheta1; //胳膊次末端旋转角

    //public static float alpha;
    //public static float beta;//几何参数的影响力：权重
    public PairGeoFeature(SkeletonLine leg1, SkeletonLine leg2, SkeletonLine arm1, SkeletonLine arm2)
    {
        legDistance = PointDistance(leg1.m_endPoint, leg2.m_endPoint);
        legDistance1 = PointDistance(leg1.m_startPoint, leg2.m_startPoint);

        armDistance = PointDistance(arm1.m_endPoint, arm2.m_endPoint);
        armDistance1 = PointDistance(arm1.m_startPoint, arm2.m_startPoint);

        legTheta = PointDotLineAngle(leg1.m_endPoint, leg2.m_endPoint);
        legTheta1 = PointDotLineAngle(leg1.m_startPoint, leg2.m_startPoint);
        armTheta = PointDotLineAngle(arm1.m_endPoint, arm2.m_endPoint);
        armTheta1 = PointDotLineAngle(arm1.m_startPoint, arm2.m_endPoint);

        //alpha = 1.5f;
        //beta = 1.5f;
    }
    public static float operator -(PairGeoFeature geo1, PairGeoFeature geo2)
    {
        return UnityEngine.Mathf.Abs(geo1.legDistance - geo2.legDistance)
            + UnityEngine.Mathf.Abs(geo1.armDistance - geo2.armDistance)
            + UnityEngine.Mathf.Abs(geo1.legDistance1 - geo2.legDistance1)
            + UnityEngine.Mathf.Abs(geo1.armDistance1 - geo2.armDistance1)
            + UnityEngine.Mathf.Abs(geo1.legTheta - geo2.legTheta)
            + UnityEngine.Mathf.Abs(geo1.legTheta1 - geo2.legTheta1)
            + UnityEngine.Mathf.Abs(geo1.armTheta1 - geo2.armTheta1)
            + UnityEngine.Mathf.Abs(geo1.armTheta - geo2.armTheta)
            ;
    }
    public float PointDistance(UnityEngine.Vector2 p1, UnityEngine.Vector2 p2)
    {
        return (p1 - p2).sqrMagnitude; //平方和
    }
    public float PointDotLineAngle(UnityEngine.Vector2 p1, UnityEngine.Vector2 p2)
    {
        UnityEngine.Vector2 delta = p2 - p1;
        float m_theta = 0;
        if (delta.x == 0)
        {
            if (delta.y > 0)
                m_theta = UnityEngine.Mathf.Deg2Rad * 90; // 90°转换为弧度制 (顺时针90)
            if (delta.y < 0)
                m_theta = -UnityEngine.Mathf.Deg2Rad * 90;
        }
        else
        {
            m_theta = UnityEngine.Mathf.Atan2(delta.y, delta.x); //弧度制夹角
        }
        return m_theta;
    }
};
