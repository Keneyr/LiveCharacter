using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInnerStepManager : Singleton<CharacterInnerStepManager>
{
    public Image imageContour;

    public Image imageSkeleton;

    public Image imageTriangulation;

    public Image BoneBinding;

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
        ConsoleController.instance.ShowMessage("点击编辑轮廓点...");
    }
    void SkeletonChange()
    {
        ConsoleController.instance.ShowMessage("点击编辑骨骼...");
    }
    void TriangulationChange()
    {
        ConsoleController.instance.ShowMessage("点击编辑网格...");
    }
    void BoneBindingChange()
    {
        ConsoleController.instance.ShowMessage("点击编辑蒙皮...");
    }
}
