using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CameraGroup : Singleton<CameraGroup>
{
    Camera contourCamera;
    Camera triangulateCamera;
    Camera skeletonCamera;
    Camera skinningCamera;
    RawImage contourImg;
    RawImage triangulateImg;
    RawImage skeletonImg;
    RawImage skinningImg;
    RawImage mainCharaterImg;
    // Start is called before the first frame update
    void Start()
    {
        contourCamera = transform.Find("ContourCamera").GetComponent<Camera>();
        triangulateCamera =transform.Find("TriangulateCamera").GetComponent<Camera>();
        skeletonCamera = transform.Find("SkeletonCamera").GetComponent<Camera>();
        skinningCamera = transform.Find("SkinningCamera").GetComponent<Camera>();
        contourImg = CharacterInnerStepManager.instance.imageContour;
        triangulateImg = CharacterInnerStepManager.instance.imageTriangulation;
        skeletonImg = CharacterInnerStepManager.instance.imageSkeleton;
        skinningImg = CharacterInnerStepManager.instance.BoneBinding;
    }

    public static Rect rect;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateRenderTexture(float width, float height) {     
        RenderTexture rt = new RenderTexture((int)width, (int)height, 24);
        contourImg.texture = contourCamera.targetTexture = rt;
        rt = new RenderTexture((int)width, (int)height, 24);
        triangulateImg.texture = triangulateCamera.targetTexture = rt;
        rt = new RenderTexture((int)width, (int)height, 24);
        skeletonImg.texture = skeletonCamera.targetTexture = rt;
        rt = new RenderTexture((int)width, (int)height, 24);
        skinningImg.texture = skinningCamera.targetTexture = rt;
        transform.position = new Vector3(width / 200, -height / 200, 0);

        contourCamera.orthographicSize = height / 200;
        triangulateCamera.orthographicSize = height / 200;
        skeletonCamera.orthographicSize = height / 200;
        skinningCamera.orthographicSize = height / 200;

      
        SetRectTransfrom(contourImg);
        SetRectTransfrom(triangulateImg);
        SetRectTransfrom(skeletonImg);
        SetRectTransfrom(skinningImg);

        //contourImg
        //if (width / height > rt.sizeDelta.x / rt.sizeDelta.y) //小幕布的尺寸大小
        //    camera.orthographicSize = width / 2;
        //else
        //    camera.orthographicSize = height / 2;
    }

    public void SetRectTransfrom(RawImage img) {
        float xscaler = img.rectTransform.rect.height * rect.width * 4/ (img.rectTransform.rect.width * rect.height*3);
        float yscaler = 1.0f;
        if (xscaler > 1)
        {
            yscaler /= xscaler;
            xscaler = 1;
        }
        img.transform.localScale = new Vector3(xscaler, yscaler, 1);
    }

    public void GenerateRenderTexture(Rect rect)
    {
        CameraGroup.rect = rect;
        GenerateRenderTexture(rect.width, rect.height);
    }
}
