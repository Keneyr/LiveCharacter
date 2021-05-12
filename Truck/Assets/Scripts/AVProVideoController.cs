using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using UnityEngine.UI;

/// <summary>
/// 和AVPro插件进行交互，控制MediaPlayer的一些属性，进而控制视频播放
/// </summary>
public class AVVideoTime
{
    public AVVideoTime() { }
    //public int currentHour;
    public int currentMinute;
    public int currentSecond;

    //public int clipHour;
    public int clipMinute;
    public int clipSecond;

    public int totalFrame = 0;
    public void Init(int a,int b,int c,int d)
    {
        currentMinute = a;
        currentSecond = b;
        clipMinute = c;
        clipSecond = d;
    }
}

public class AVProVideoController : Singleton<AVProVideoController>
{

    public MediaPlayer mediaPlayer; //持有控制视频播放的组件
    private AVVideoTime avvideoTime = new AVVideoTime();
    
    void Awake()
    {
        base.Awake();
        mediaPlayer.Events.AddListener(MediaEventHandler);
    }
    //加载视频
    public void LoadVideo(string videoPath)
    {
        //通过插件中的方法加载（1.加载路径格式（与面板上相对应）2.加载的文件名 3.默认是否开始播放）
        mediaPlayer.OpenVideoFromFile(
            MediaPlayer.FileLocation.AbsolutePathOrURL,
            videoPath, false); 
    }

    void InitVideoParams()
    {
        int totalFrame = GetTotalFrame() - 1;
        VideoSliderController.instance.sliderEndFrame.maxValue = totalFrame;
        VideoSliderController.instance.sliderStartFrame.maxValue = totalFrame;
        VideoSliderController.instance.sliderInterval.maxValue = totalFrame;

        VideoSliderController.instance.sliderEndFrame.value = 0;
        VideoSliderController.instance.sliderStartFrame.value = 0;
        VideoSliderController.instance.sliderInterval.value = totalFrame>2?2:0;
        

    }

    //视频播放时间触发
    private void MediaEventHandler(MediaPlayer arg0, MediaPlayerEvent.EventType arg1, ErrorCode arg2)
    {
        switch(arg1)
        {
            case MediaPlayerEvent.EventType.Closing:
                Console.Log("关闭播放器");
                break;
            case MediaPlayerEvent.EventType.Error:
                Console.LogError("播放器出错");
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                Console.Log("播放完成");
                break;
            case MediaPlayerEvent.EventType.FirstFrameReady:
                Console.Log("准备完成");
                InitVideoParams();
                UpdateTimeText();
                break;
            case MediaPlayerEvent.EventType.MetaDataReady:
                Console.Log("媒体数据准备中");
                break;
            case MediaPlayerEvent.EventType.ReadyToPlay:
                Console.Log("准备去播放");
                break;
            case MediaPlayerEvent.EventType.Started:
                Console.Log("开始播放");
                break;
            default:
                //ConsoleController.ShowMessage("");
                break;
        }
    }
    
    //更新播放进度的时间显示
    void UpdateTimeText()
    {
        //对当前播放时间转换时间格式
        //转化为秒
        int tCurrentSeconds = (int)mediaPlayer.Control.GetCurrentTimeMs() / 1000;
        //获取当前分
        int tCurrentMin = tCurrentSeconds / 60;
        //重新赋值剩余多少秒
        tCurrentSeconds = tCurrentSeconds % 60;

        //对总时间转化时间格式
        //转化为秒
        int tVideoTimeSeconds = (int)mediaPlayer.Info.GetDurationMs() / 1000;
        //获取总的分数
        int tVideoTimeMin = tVideoTimeSeconds / 60;
        //重新赋值剩余多少秒
        tVideoTimeSeconds = tVideoTimeSeconds % 60;

        avvideoTime.Init(tCurrentMin, tCurrentSeconds, tVideoTimeMin, tVideoTimeSeconds);
        avvideoTime.totalFrame = GetTotalFrame();
        VideoSliderController.instance.ShowVideoTime(avvideoTime);
        
    }

    public int GetTotalFrame()
    {
        return Helper.ConvertTimeSecondsToFrame(mediaPlayer.Info.GetDurationMs() / 1000.0f, 30);
    }

    public int GetCurFrame()
    {
        return Helper.ConvertTimeSecondsToFrame(mediaPlayer.Control.GetCurrentTimeMs() / 1000.0f, 30);
    }

    

    public void SetVideoTimeIndexChange(float value)
    {
        //获取视频总长度
        float tVideoTime = mediaPlayer.Info.GetDurationMs();
        //当前视频时间
        float tCurrentTime = value * tVideoTime / GetTotalFrame();
        //将视频调到对应的节点
        mediaPlayer.Control.Seek(tCurrentTime);
        UpdateTimeText();
    }

}

