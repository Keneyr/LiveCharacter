﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using UnityEngine.UI;

public class ImportVideo : MonoBehaviour
{
    
    private Button Open;
    // Use this for initialization
    void Start()
    {
        //鼠标点击按钮事件
        //Open = GameObject.Find("BtnImportVideo").GetComponent<Button>();
        //Open.onClick.AddListener(OnClick);
    }


    public static bool ShowInExplorer(string itemPath)
    {
        bool result = false;

#if !UNITY_WEBPLAYER
        itemPath = Path.GetFullPath(itemPath.Replace(@"/", @"\"));
        if (File.Exists(itemPath))
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            Process.Start("explorer.exe", "/select," + itemPath);
#endif
            result = true;
        }
        else if (Directory.Exists(itemPath))
        {

            UnityEngine.Application.OpenURL(itemPath);
            result = true;
        }

#endif

        return result;
    }
    public void OpenFile()
    {
        OpenFileDlg pth = new OpenFileDlg();
        pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
        pth.filter = null;
        pth.file = new string(new char[256]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[64]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = Application.dataPath;  // default path  
        pth.title = "打开视频文件";
        pth.defExt = "mp4";
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (BaseFileDialog.GetOpenFileName(pth))
        {
            string filepath = pth.file; //选择的文件路径;
            Console.Log("导入视频文件...");
            Console.Log(filepath);
            //将路径中\转化为/
            filepath = filepath.Replace("\\","/");
            //URLVideoPlayerController.instance.ShowVideo(filepath); //渲染原视频到幕布上
            AVProVideoController.instance.LoadVideo(filepath);
        }
    }

    public void OnClick()
    {
        //ShowInExplorer("C:/");
        OpenFile();
    }

}
