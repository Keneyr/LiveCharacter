using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;

public static class Console
{
    private static string fullPath;

    private static bool m_hasForceMono = false;
    public static void InitLogger()
    {
        fullPath = Application.dataPath + "/output.txt";
        if (File.Exists(fullPath)) File.Delete(fullPath);
        if (Directory.Exists(fullPath.Replace("/output.txt", "")))
        {
            FileStream fs = File.Create(fullPath);
            fs.Close();
            Application.logMessageReceived += logCallBack;
        }
        else
        {
            Debug.LogError("directory is not exist");
        }
    }

    private static void logCallBack(string condition, string stackTrace, LogType type)
    {
        if (File.Exists(fullPath))
        {
            using (StreamWriter sw = File.AppendText(fullPath))
            {
                sw.WriteLine(condition);
                sw.WriteLine(stackTrace);
            }
        }
    }

    private static bool UseLog = true;

    public static void SetUseDebuger(bool use)
    {
        UseLog = use;
    }

    public static void Log(object info)
    {
        if (UseLog)
            Debug.Log("<color=blue>[Conosle]</color>" + info);
        ConsoleController.ShowMessage("<color=cyan>"+info+"</color>");

    }

    public static void LogWarning(object WarningInfo)
    {
        if (UseLog)
            Debug.Log("<color=yellow>[Conosle]</color>" + WarningInfo);
        ConsoleController.ShowMessage("<color=yellow>" + WarningInfo + "</color>");
    }

    public static void LogError(object ErrorInfo)
    {
        if (UseLog)
            Debug.LogError("<color=red>[Conosle]</color>" + ErrorInfo);
        ConsoleController.ShowMessage("<color=red>" + ErrorInfo + "</color>");
    }

#if UNITY_EDITOR
    // 处理asset打开的callback函数
    [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
    static bool OnOpenAsset(int instance, int line)
    {
        if (m_hasForceMono) return false;
        // 自定义函数，用来获取log中的stacktrace，定义在后面。
        string stack_trace = GetStackTrace();
        // 通过stacktrace来定位是否是我们自定义的log，我的log中有特殊文字[SDebug]，很好识别
        if (!string.IsNullOrEmpty(stack_trace) && stack_trace.Contains("[Conosle]"))
        {
            // 正则匹配at xxx，在第几行
            Match matches = Regex.Match(stack_trace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
            string pathline = "";
            while (matches.Success)
            {
                pathline = matches.Groups[1].Value;
                // 找到不是我们自定义log文件的那行，重新整理文件路径，手动打开
                if (!pathline.Contains("Console.cs") && !string.IsNullOrEmpty(pathline))
                {
                    int split_index = pathline.LastIndexOf(":");
                    string path = pathline.Substring(0, split_index);
                    line = Convert.ToInt32(pathline.Substring(split_index + 1));
                    m_hasForceMono = true;
                    //方式一
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), line);
                    m_hasForceMono = false;
                    //方式二
                    //string fullpath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                    // fullpath = fullpath + path;
                    //  UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullpath.Replace('/', '\\'), line);
                    return true;
                }
                matches = matches.NextMatch();
            }
            return true;
        }
        return false;
    }

    static string GetStackTrace()
    {
        // 找到类UnityEditor.ConsoleWindow
        var type_console_window = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
        // 找到UnityEditor.ConsoleWindow中的成员ms_ConsoleWindow
        var filedInfo = type_console_window.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
        // 获取ms_ConsoleWindow的值
        var ConsoleWindowInstance = filedInfo.GetValue(null);
        if (ConsoleWindowInstance != null)
        {
            if ((object)EditorWindow.focusedWindow == ConsoleWindowInstance)
            {
                // 找到类UnityEditor.ConsoleWindow中的成员m_ActiveText
                filedInfo = type_console_window.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                string activeText = filedInfo.GetValue(ConsoleWindowInstance).ToString();
                return activeText;
            }
        }
        return null;
    }
#endif
}