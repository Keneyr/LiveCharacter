using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

//namespace Assets.AutoCha2DAni.Scripts.Editor

public class VideoDrivenAnimationWindow : EditorWindow
{
    [SerializeField]
    int MaxFrames = 60;

    [SerializeField]
    Object m_VideoFolder;

    [SerializeField]
    int m_Step = 1;

    [SerializeField]
    int m_StartFrame = 1;

    [SerializeField]
    int m_EndFrame = 1;

    [MenuItem("Window/AutoCha2DAni/Video-Driven Animation")]
    static void ContextInitialize()
    {
        EditorWindow.GetWindow<VideoDrivenAnimationWindow>("Video-Driven Animation");
    }
    void OnEnable()
    {
        
    }
    void OnDisable()
    {
        
    }
    void OnGUI()
    {
        EditorGUIUtility.labelWidth = 60f;
        EditorGUIUtility.fieldWidth = 30f;

        EditorGUI.BeginChangeCheck();

        //GUIContent contentLabel = new GUIContent();
        //contentLabel.text = "Video Folder:";
        //contentLabel.tooltip = "The asset folder which contains the motion data keypoints.";
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Video Folder:");
        m_VideoFolder = EditorGUILayout.ObjectField(GUIContent.none, m_VideoFolder, typeof(Object), false);

        //显示视频(json文件序列)总体帧数
        EditorGUILayout.LabelField("TotalVideoFrames:");
        
        if(m_VideoFolder)
        {
            CalculateNumFrames();
            EditorGUILayout.LabelField(MaxFrames.ToString());
        }

        //抉择对视频分帧处理的间隔
        EditorGUILayout.LabelField("Step:");
        m_Step = EditorGUILayout.IntSlider(GUIContent.none, m_Step, 1, MaxFrames);

        //开始帧
        EditorGUILayout.LabelField("StartFrame:");
        m_StartFrame = EditorGUILayout.IntSlider(GUIContent.none, m_StartFrame, 1, MaxFrames);

        //结束帧
        EditorGUILayout.LabelField("EndFrame:");
        m_EndFrame = EditorGUILayout.IntSlider(GUIContent.none, m_EndFrame, 1, MaxFrames);

        EditorGUILayout.LabelField("要处理的帧序列文件名:");
        //根据选择的开始帧和结束帧，显示要读取的文件名字
        if (m_EndFrame != 1)
        {
            List<FileInfo> filesInfo = PoseJsonLoderUtils.GetJsonFileNames(m_VideoFolder, m_Step, m_StartFrame,m_EndFrame);

            EditorGUILayout.LabelField(filesInfo[0].FullName.ToString());
            EditorGUILayout.LabelField(filesInfo[filesInfo.Count-1].FullName.ToString());

            //Debug.Log(filesInfo.Count);

            //foreach (FileInfo f in filesInfo)
            //{
            //    EditorGUILayout.LabelField(f.FullName.ToString()); 
            //}
        }
        //开始处理按钮
        GUILayout.Space(10);

        GUIContent buttonContent = new GUIContent("开始重定向生成动画");
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.GetStyle("button"));
        if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.ExpandWidth(true)))
        {
            if(m_VideoFolder)
                //开启读取视频带动scene中的骨骼运动
                PoseJsonLoderUtils.ParseJson(m_VideoFolder, m_Step, m_StartFrame, m_EndFrame);
        }
        GUILayout.Space(10);
        if(EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }
    private void CalculateNumFrames()
    {
        //读取路径下的json文件，计算其数量，作为总帧数
        string folder = AssetDatabase.GetAssetPath(m_VideoFolder);//把文件路径也当作了asset资源来看待？
        //Debug.Log(folder);
        //??这样写好过分啊！先总结出来所有的资源，再删除非这个路径的资源..但是不管了，后续再改吧...
#if true
        List<string> assetPaths = new List<string>(AssetDatabase.GetAllAssetPaths());
        assetPaths.RemoveAll(each => !each.StartsWith(folder));
#endif
        //Debug.Log(assetPaths.Count-1);
        MaxFrames = assetPaths.Count - 1;
    }

}

