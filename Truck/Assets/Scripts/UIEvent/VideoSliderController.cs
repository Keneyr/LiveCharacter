﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum SignalType
{
    startFrameChange = 0,
    endFrameChange = 1,
    intervalChange = 2,
};


/// <summary>
/// 根据用户交互，设置初始帧，结束帧，帧间隔 参数
/// 查看视频总帧数
/// </summary>
public class VideoSliderController : Singleton<VideoSliderController>
{
    public Slider sliderStartFrame;
    public InputField inputStartFrame;
    //private int startFrame;

    public Slider sliderEndFrame;
    public InputField inputEndFrame;
    //private int endFrame;

    public Slider sliderInterval;
    public InputField inputInterval;
    //private int interval;

    public Text textTotalFrame;
    //private int totalFrames;

    public Text textCurrentTime;

    //SignalType signalType;

    private void Awake()
    {
        base.Awake();

    }
   
    /// <summary>
    /// 监听，让slider和inputfield相互控制
    /// </summary>
    void Start()
    {
        sliderStartFrame.onValueChanged.AddListener(SliderStartChange);
        sliderEndFrame.onValueChanged.AddListener(SliderEndChange);
        sliderInterval.onValueChanged.AddListener(SliderIntervalChange);
        inputStartFrame.onValueChanged.AddListener(InputFieldStartChange);
        inputEndFrame.onValueChanged.AddListener(InputFieldEndChange);
        inputInterval.onValueChanged.AddListener(InputFieldIntervalChange);

    }
    bool IsSelectingSlider()
    {
        if (EventSystem.current.currentSelectedGameObject.Equals(sliderStartFrame.gameObject) ||
           EventSystem.current.currentSelectedGameObject.Equals(sliderEndFrame.gameObject) ||
           EventSystem.current.currentSelectedGameObject.Equals(sliderInterval.gameObject))
            return true;

        return false;
    }
    bool IsSelectingInputField()
    {
        if (EventSystem.current.currentSelectedGameObject.Equals(inputStartFrame.gameObject) ||
           EventSystem.current.currentSelectedGameObject.Equals(inputEndFrame.gameObject) ||
           EventSystem.current.currentSelectedGameObject.Equals(inputInterval.gameObject))
            return true;

        return false;
    }

    void SliderStartChange(float value)
    {
        //改成判断是否更新对应的text阈更好，因为很可能在程序中更新slider
        //if (!IsSelectingSlider())
        //    return;
        if (IsSelectingInputField())
            return;
        inputStartFrame.text = value.ToString();
        //更新视频播放时间及视频帧画面
        SetVideoTimeIndexChange(value);
        //更新展示的渲染图
        SetShowRenderFramePngIndexChange(value, SignalType.startFrameChange);
    }
    void SliderEndChange(float value)
    {
        //if (!IsSelectingSlider())
        //    return;
        if (IsSelectingInputField())
            return;
        inputEndFrame.text = value.ToString();

        //更新展示的渲染图
        SetShowRenderFramePngIndexChange(value, SignalType.endFrameChange);
    }
    void SliderIntervalChange(float value)
    {
        //if (!IsSelectingSlider())
        //    return;
        if (IsSelectingInputField())
            return;
        inputInterval.text = value.ToString();

        //更新展示的渲染图
        SetShowRenderFramePngIndexChange(value, SignalType.intervalChange);
    }
    void InputFieldStartChange(string str)
    {
        //if (!IsSelectingInputField())
        //    return;
        if (IsSelectingSlider())
            return;
        if(int.Parse(str)>=0 && int.Parse(str) <= sliderStartFrame.maxValue)
        {
            sliderStartFrame.value = int.Parse(str);
            SetVideoTimeIndexChange(sliderStartFrame.value);
        }
        else
        {
            Console.LogWarning("超出最大值");
            sliderInterval.value = sliderInterval.maxValue;
        }
    }
    void InputFieldEndChange(string str)
    {
        //if (!IsSelectingInputField())
        //    return;
        if (IsSelectingSlider())
            return;
        if (int.Parse(str) >= 0 && int.Parse(str) <= sliderEndFrame.maxValue)
        {
            sliderEndFrame.value = int.Parse(str);
        }
        else
        {
            Console.LogWarning("超出最大值");
            sliderInterval.value = sliderInterval.maxValue;
        }
    }

    void InputFieldIntervalChange(string str)
    {
        //if (!IsSelectingInputField())
        //    return;
        if (IsSelectingSlider())
            return;
        if (int.Parse(str) >= 0 && int.Parse(str) <= sliderInterval.maxValue)
        {
            sliderInterval.value = int.Parse(str);
        }
        else
        {
            Console.LogWarning("超出最大值");
            sliderInterval.value = sliderInterval.maxValue;
        }

    }
    //根据用户开始帧索引显示视频帧画面--相当于是可拖动的进度条
    void SetVideoTimeIndexChange(float value)
    {
        //URLVideoPlayerController.instance.SetVideoTimeIndexChange(value);
        AVProVideoController.instance.SetVideoTimeIndexChange(value);
    }
    //根据用户开始的帧索引显示渲染后的视频帧画面到可视化区
    void SetShowRenderFramePngIndexChange(float value, SignalType _signalType)
    {
        switch(_signalType)
        {
            case SignalType.startFrameChange:
                //更新右侧条
                DrawVideoPoseFrames.SetShowRenderFramePngIndexChange();
                break;
            case SignalType.endFrameChange:
                //更新尾部texture
                DrawVideoPoseFrames.SetShowRenderFramePngEndIndexChange();
                break;
            case SignalType.intervalChange:
                //更新texture
                DrawVideoPoseFrames.SetShowRenderFramePngIntervalChange();
                break;
            default:
                break;
        }
    }

#if false
    //因为帧索引从0起步
    public void ShowTotalFrames(ulong totalframe)
    {
        string message = "视频总帧数：" + totalframe.ToString();
        Console.Log(message);

        textTotalFrame.text = totalframe.ToString();
        sliderStartFrame.maxValue = totalframe - 1;
        sliderEndFrame.maxValue = totalframe - 1;
        sliderInterval.maxValue = totalframe - 1;
    }
#endif
    public void ShowVideoTime(AVVideoTime videoTime)
    {
        textCurrentTime.text = string.Format("{0:D2}:{1:D2} / {2:D2}:{3:D2}",
            videoTime.currentMinute, videoTime.currentSecond, videoTime.clipMinute, videoTime.clipSecond);
        textTotalFrame.text = videoTime.totalFrame.ToString();
    }


}
