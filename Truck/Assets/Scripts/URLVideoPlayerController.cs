using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 当用户通过对话框选择视频路径后，将视频渲染到RawImage上
/// 并且可以用户交互的初始帧显示初始帧
/// </summary>

public class URLVideoPlayerController : Singleton<URLVideoPlayerController>
{
    private RawImage rawImage;
    private VideoPlayer videoPlayer;

    void Start()
    {
        rawImage = this.GetComponent<RawImage>();
        videoPlayer = this.GetComponent<VideoPlayer>();
    }
    void Update()
    {
        rawImage.texture = videoPlayer.texture;
    }
    public void ShowVideo(string filepath)
    {
        videoPlayer.url = filepath;
        videoPlayer.playOnAwake = false;
        VideoButtonController.instance.ShowTotalFrames(videoPlayer.frameCount);
    }

}
