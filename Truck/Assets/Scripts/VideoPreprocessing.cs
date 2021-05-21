using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 对视频进行预处理，包括提取运动，姿态检索
/// </summary>
public static class VideoPreprocessing
{
    static Queue<OpenPose.MultiArray<float>> keyPointsQueue = new Queue<OpenPose.MultiArray<float>>();


    public static VideoMotionExtractResult ExtractVideoMotion()
    {
        //OpenPose视频检测
        string videoPath = AVProVideoController.instance.videoPath;
        

        if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
        {
            //判断视频检测帧序列是否已经存在于本地，如果已经存在，则不再启动OpenPose，直接读取视频帧png文件
            string renderVideoPath = videoPath.Substring(0,videoPath.LastIndexOf("."));
            if(!string.IsNullOrEmpty(renderVideoPath) && File.Exists(renderVideoPath))
            {
                //读取渲染图像到展览区
                DrawVideoPoseFrames.InitVideoPoseInfo();
                return VideoMotionExtractResult.Success;
            }
            VideoOpenPoseScript.producerString = videoPath;
            videoPath = System.IO.Path.GetDirectoryName(videoPath);
            VideoOpenPoseScript.folderSaveKeyPoints = videoPath + "\\keypoints";
            VideoOpenPoseScript.folderSaveImage = videoPath + "\\images";
            
            return VideoMotionExtractResult.OpenPoseStart;
        }
        return VideoMotionExtractResult.VideoError;
    }

    //OpenPose调用接口
    public static VideoMotionExtractResult ExtractVideoMotionPoseDatum(OpenPose.OPDatum datum)
    {
        keyPointsQueue.Enqueue(datum.poseKeypoints);

        return VideoMotionExtractResult.Success;
    }

    public static VideoMotionRetrivalResult RetrivalVideoMotion()
    {
        //OpenPose实时运行获取到的视频帧数据
        if (keyPointsQueue.Count > 0)
        {

        }
        //没有启用OpenPose，本地已经有了json文件序列，直接读取
        else
        {

        }
        return VideoMotionRetrivalResult.jsonError;
    }
}
