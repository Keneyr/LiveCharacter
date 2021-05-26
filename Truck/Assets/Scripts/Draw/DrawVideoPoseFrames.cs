using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

/// <summary>
/// 根据UI交互区的开始帧和帧间隔，显示OpenPose检测好的视频帧渲染图到展览区
/// </summary>
public class DrawVideoPoseFrames : MonoBehaviour
{
    static bool canReadRenderPng = false;
    static bool canChangeRenderPng = false;

    static float startframe;
    static float endframe;
    static float frameinterval;

    static List<GameObject> rawImages = new List<GameObject>();

    static Transform content;

    void Start()
    {
        //判断是否存在[startFrame-endFrame]之间的图像，如果有，就按照顺序基于帧间隔显示
        startframe = VideoSliderController.instance.sliderStartFrame.value;
        endframe = VideoSliderController.instance.sliderEndFrame.value;
        frameinterval = VideoSliderController.instance.sliderInterval.value;

        content = transform;
    }
    void Update()
    {
        
    }
    public static void drawVideoPoseFrames()
    {
        if (!canReadRenderPng)
        {
            return;
        }
        startframe = VideoSliderController.instance.sliderStartFrame.value;
        endframe = VideoSliderController.instance.sliderEndFrame.value;
        frameinterval = VideoSliderController.instance.sliderInterval.value;

        DirectoryInfo root = new DirectoryInfo(VideoPreprocessing.RenderImageSavePath);
        FileInfo[] allFiles = root.GetFiles().Where(f => f.Name.EndsWith(".png")).ToArray();

        rawImages.Clear();
        
        for(int i=0; i<allFiles.Length; i+=(int)frameinterval)
        {
            //迭代此路径下的所有文件
            Texture2D tx = new Texture2D(100, 100);
            tx.LoadImage(Extra.GetImageByte(allFiles[i].FullName));

            GameObject rawImage = new GameObject(i.ToString());
            rawImage.transform.parent = content;
            rawImage.AddComponent<RectTransform>();
            rawImage.AddComponent<RawImage>();

            rawImage.GetComponent<RawImage>().texture = tx;

            rawImages.Add(rawImage);

            //i += (int)frameinterval;
            if (i == 1)
                break;
        }

        canReadRenderPng = false;
        //ResetPastVideoInfo();//防止多次update渲染
    }
    public static void InitVideoPoseInfo()
    {
        canReadRenderPng = true;
        drawVideoPoseFrames();
    }
    //to be continued
    public static void ResetPastVideoInfo()
    {
        canReadRenderPng = false;
        rawImages.Clear();

        //清除子物体，否则换角色不会重置
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    public static void SetShowRenderFramePngIndexChange()
    {

    }
    public static void SetShowRenderFramePngEndIndexChange()
    {

    }
    public static void SetShowRenderFramePngIntervalChange()
    {

    }
}
