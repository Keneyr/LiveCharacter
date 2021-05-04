using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using UnityEngine.UI;

public class ImportVideo : MonoBehaviour
{
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        
    }

    private Button Open;
    // Use this for initialization
    void Start()
    {
        //鼠标点击按钮事件
        Open = GameObject.Find("Open").GetComponent<Button>();
        Open.onClick.AddListener(OnClick);
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

    public void OnClick()
    {
        ShowInExplorer("C:/");
    }

}
