using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 控制台程序，控制UI输出信息
/// </summary>
public class ConsoleController : Singleton<ConsoleController>
{

    [SerializeField]
    [Tooltip("控制台输出框对象")]
    private Text outputText = null;
    Scrollbar scrollbar ;
    RectTransform content;

    protected void Awake()
    {
        base.Awake();
        scrollbar = GetComponentInChildren<Scrollbar>();
        content = outputText.transform.parent as RectTransform;
        outputText.text =
            "> Welcome To LiveCharacter2D \n";
        scrollbar.value = 1;
    }
    
    public static void ShowMessage(object message)
    {
        float textHeight = instance.outputText.preferredHeight;
        instance.outputText.text += "> " + message + "\n";
        instance.outputText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
       // instance.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
        if (instance.content.parent.GetComponent<RectTransform>().rect.height < textHeight) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(instance.scrollbar.GetComponent<RectTransform>());
            DOTween.To(() => instance.scrollbar.value = 0, v => instance.scrollbar.value = v, 0, 0.1f).SetEase(Ease.InElastic);
        }
    }
}
