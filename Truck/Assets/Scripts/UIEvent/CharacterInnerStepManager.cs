using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInnerStepManager : Singleton<CharacterInnerStepManager>
{
    public RawImage imageContour;

    public RawImage imageSkeleton;

    public RawImage imageTriangulation;

    public RawImage BoneBinding;

    private void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        imageContour.GetComponent<Button>().onClick.AddListener(ContourChange);
        imageSkeleton.GetComponent<Button>().onClick.AddListener(SkeletonChange);
        imageTriangulation.GetComponent<Button>().onClick.AddListener(TriangulationChange);
        BoneBinding.GetComponent<Button>().onClick.AddListener(BoneBindingChange);
    }
    void ContourChange()
    {
        Console.Log("点击编辑轮廓点...");
    }
    void SkeletonChange()
    {
        Console.Log("点击编辑骨骼...");
    }
    void TriangulationChange()
    {
        Console.Log("点击编辑网格...");
    }
    void BoneBindingChange()
    {
        Console.Log("点击编辑蒙皮...");
    }
}
