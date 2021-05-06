using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 当用户通过对话框选择视频路径后，将视频渲染到RawImage上
/// 并且可以用户交互的初始帧显示初始帧;控制视频
/// </summary>


public class VideoTime
{
    public int currentHour;
    public int currentMinute;
    public int currentSecond;

    public int clipHour;
    public int clipMinute;
    public int clipSecond;
}
public class URLVideoPlayerController : Singleton<URLVideoPlayerController>
{
    private RawImage rawImage;
    private VideoPlayer videoPlayer;

    VideoTime videoTime;
    void Start()
    {
        rawImage = this.GetComponent<RawImage>();
        videoPlayer = this.GetComponent<VideoPlayer>();
        videoTime = new VideoTime();
    }
    void Update()
    {
        
    }
    public void ShowVideoTime()
    {
        //当前视频播放时间
        SetCurrentVideoTime();
        VideoButtonController.instance.ShowVideoTime(videoTime);
        VideoButtonController.instance.ShowTotalFrames(videoPlayer.frameCount);
    }
    private void SetCurrentVideoTime()
    {
        videoTime.currentHour = (int)videoPlayer.time / 3600;
        videoTime.currentMinute = (int)(videoPlayer.time - videoTime.currentHour * 3600) / 60;
        videoTime.currentSecond = (int)(videoPlayer.time - videoTime.currentHour * 3600 - videoTime.currentMinute * 60);
    }
    //初始化视频一切参数
    public void ShowVideo(string filepath)
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = filepath;
        InitVideoParams();
        ShowVideoTime();

        //显示初始画面
        videoPlayer.Prepare();
        if (videoPlayer.isPrepared)
        {
            rawImage.texture = videoPlayer.texture; //渲染视频到UGUI上
        }
    }
    private void InitVideoParams()
    {
        videoPlayer.playOnAwake = false;
        SetVideoTimeParams();
        
    }
    private void SetVideoTimeParams()
    {
        //帧数 / 帧速率 = 总时长
        var clipLength = videoPlayer.frameCount / videoPlayer.frameRate;
        videoTime.clipHour = (int)clipLength / 3600; ;
        videoTime.clipMinute = (int)(clipLength - videoTime.clipHour * 3600) / 60;
        videoTime.clipSecond = (int)(clipLength - videoTime.clipHour * 3600 - videoTime.clipMinute * 60);
    }
    
    //根据用户开始帧索引显示视频帧画面
    public void SetVideoTimeIndexChange(float value)
    {
        videoPlayer.time = value / videoPlayer.frameRate;
        //videoPlayer.time = value * videoPlayer.clip.length;
        SetCurrentVideoTime();
        VideoButtonController.instance.ShowVideoTime(videoTime);
    }
}
