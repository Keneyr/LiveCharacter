using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 默认是draw overlay 以及 draw pies 五彩斑斓的好看的
/// </summary>

public class DrawSkinning : MonoBehaviour
{
    static RectTransform rt;
    static Camera SkinningCamera;
    static RawImage rawImage;

    RenderTexture renderTargetTexture;
    // Start is called before the first frame update
    void Start()
    {
        SkinningCamera = GameObject.Find("SkinningCamera").GetComponent<Camera>();
        SkinningCamera.targetTexture = renderTargetTexture;

        rawImage = GetComponent<RawImage>();
        rawImage.texture = renderTargetTexture;
        rt = GetComponent<RectTransform>();
    }
    public void OnRenderObject()
    {

    }

    public static void ResetPastCharacterInfo()
    {

    }
    public static void InitBindingInfo()
    {

    }
}
