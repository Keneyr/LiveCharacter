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
    Scrollbar scrollbar ;

    protected void Awake()
    {
        base.Awake();
        scrollbar = GetComponentInChildren<Scrollbar>();
        outputText.text =
            "> Welcome To LiveCharacter2D \n";
        scrollbar.value = 1;
    }
    
    public static void ShowMessage(object message)
    {
        instance.outputText.text += "> " + message + "\n";
        instance.scrollbar.value = 0;
    }


}
