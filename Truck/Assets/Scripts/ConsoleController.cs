using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制台程序，控制UI输出信息
/// </summary>
public class ConsoleController : Singleton<ConsoleController>
{

    [SerializeField]
    [Tooltip("控制台输出框对象")]
    private Text outputText = null;

    //public static ConsoleController consoleController;

    private void Awake()
    {
        //instance = this;
    }

    public void ShowMessages(string message)
    {
        UnityEngine.Debug.Log(message);
        outputText.text +="> " + message + "\n";
    }
}
