using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// 控制台程序，控制UI输出信息
/// </summary>
public class ConsoleController : Singleton<ConsoleController>
{

    [SerializeField]
    [Tooltip("控制台输出框对象")]
    private Text outputText = null;

    protected void Awake()
    {
        base.Awake();
        outputText.text =
            "> Welcome To LiveCharacter2D \n";
    }
    
    public static void ShowMessage(object message)
    {
        instance.outputText.text += "> " + message + "\n";
    }


}
