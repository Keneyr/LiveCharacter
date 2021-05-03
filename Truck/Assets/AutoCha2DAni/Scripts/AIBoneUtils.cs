using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using OpenPose;
using Anima2D;

/// <summary>
/// 将AutoChaAniUserScript传递过来的骨骼关键点转化为有层次的Bone2D类型物体到Hierarchy面板上
/// </summary>

class AIBoneUtils
{
    //根据这个keypoints的关键点顺序，转化为对应的Bone2D物体到Scene中，后续再改
    public static void ChangeKeypoints2Bone2D(MultiArray<float> keypoints)
    {
        Debug.Log("将2D角色骨骼信息生成到Hierarchy面板中...");
        //18*3=54个点，(x,y,score)的形式
        foreach (var t in keypoints)
        {
            Debug.Log(t);
        }
        MultiArray<float> keypointsXY = new MultiArray<float>();
        
        //1.把54个值去掉score，只保留x，y值，得到36个值
        for (int i = 0;i < keypoints.Count;i++)
        {
            if(i % 3 == 0 || i % 3 == 1)
            {
                keypointsXY.Add(keypoints[i]);
            }
        }

        //2.我们只要前14个有用关键点，对应28个值，将值转化为点
        List<Vector2> points = new List<Vector2>();
        for (int i = 0;i < keypointsXY.Count - 8; i+=2)
        {
            points.Add(new Vector2(keypointsXY[i] / 100f, -keypointsXY[i+1] / 100f));//(x,y)
        }

        //3.将点两两配对转化为Bone2D，确定Bone2D的两个端点的位置和length以及rotation
        //（根据OpenPose转化的骨骼图和我们想要的骨骼属性结构）
        // 考虑关键点为0的情况--要不先手动删除空bone物体把...
        int rootpointsindex = 1;
        int[] firstlayerpointsindex = new int[5] { 0, 2, 5, 8, 11 };
        int[] ffirstlayerpointsindex = new int[4]{ 2, 5, 8, 11 };
        int[] secondlayerpointsindex = new int[4] { 3, 6, 9, 12 };
        //14个关键点，对应13个骨骼
        //其实应该叫锁骨、上胳膊、下胳膊这样，但是你懂就好
        string[] skeletonsName =
        {
            "neck_bone", //0
            " ",  //1
            "rshoulder_bone", //2
            "relbow_bone", //3
            "rwrist_bone", //4
            "lshoulder_bone", //5
            "lelbow_bone", //6
            "lwrist_bone", //7
            "rhip_bone", //8
            "rknee_bone", //9
            "rankle_bone", //10
            "lhip_bone", //11
            "lknee_bone", //12
            "lankle_bone" //13
        };
        for(int i = 0;i < points.Count;i++)
        {
            //3.0.root根节点到第一层级关键点的骨骼
            if(i == rootpointsindex)
            {
                for(int j = 0; j < firstlayerpointsindex.Length;j++)
                {
                    //转化为骨骼，零散到hierarchy,因为我是根据两个关键点求Bone2D的，所以Bone2D的长度、旋转角都应该求出来更新其值
                    GameObject bone = new GameObject(skeletonsName[firstlayerpointsindex[j]]);
                    bone.tag = "bone2D";
                    Bone2D boneComponent = bone.AddComponent<Bone2D>();
                    if (points[i] == Vector2.zero || points[firstlayerpointsindex[j]] == Vector2.zero)
                    {
                        bone.transform.position = Vector3.zero;
                        boneComponent.globalendPosition = Vector3.zero;
                        boneComponent.globalstartPosition = Vector3.zero;
                        ResetZeroBone2DProperty(bone);
                    }else
                    {
                        bone.transform.position = new Vector3(points[i].x, points[i].y, 0); //pixelsToUnit
                        boneComponent.globalstartPosition = bone.transform.position;
                        boneComponent.globalendPosition = new Vector3(points[firstlayerpointsindex[j]].x, points[firstlayerpointsindex[j]].y, 0);
                        UpdateBone2DProperty(bone);
                    }

                }
                
            }
            //3.1. 第一层级关键点到第二层级关键点的骨骼...第二层级关键点到第三层级关键点的骨骼
            else if(Array.IndexOf(ffirstlayerpointsindex,i) != -1 || Array.IndexOf(secondlayerpointsindex, i) != -1)
            {
                //转化为骨骼，零散到hierarchy,因为我是根据两个关键点求Bone2D的，所以Bone2D的长度、旋转角都应该求出来更新其值
                GameObject bone = new GameObject(skeletonsName[i + 1]);
                bone.tag = "bone2D";
                Bone2D boneComponent = bone.AddComponent<Bone2D>();

                //没有检测到关键点的情况,直接归为坐标原点
                if (points[i] == Vector2.zero || points[i+1] == Vector2.zero)
                {
                    bone.transform.position = Vector3.zero;
                    boneComponent.globalendPosition = Vector3.zero;
                    boneComponent.globalstartPosition = Vector3.zero;
                    ResetZeroBone2DProperty(bone);
                }else
                {
                    bone.transform.position = new Vector3(points[i].x, points[i].y, 0);
                    boneComponent.globalendPosition = new Vector3(points[i + 1].x, points[i + 1].y, 0);
                    boneComponent.globalstartPosition = bone.transform.position;
                    UpdateBone2DProperty(bone);
                }       
            }
        }
    }

    //3.2.根据骨骼的两个端点，计算其旋转角(矩阵),长度，更新端点值让其能够在Scene场景中的绘制也随之跟新
    private static void UpdateBone2DProperty(GameObject bone)
    {
        Quaternion l_deltaRotation = Quaternion.identity;
        //先看看BoneUtils类中有没有现成的接口--似乎都不太现成，所以先自己写吧
        Bone2D boneComponent = bone.GetComponent<Bone2D>();
        boneComponent.localLength = (boneComponent.globalstartPosition - boneComponent.globalendPosition).magnitude;
        Vector2 localPosition = new Vector2(boneComponent.globalendPosition.x - boneComponent.globalstartPosition.x,
                                            boneComponent.globalendPosition.y - boneComponent.globalstartPosition.y);
        float angle = Mathf.Atan2(localPosition.y, localPosition.x) * Mathf.Rad2Deg;
        l_deltaRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        bone.transform.localRotation *= l_deltaRotation;
    }
    private static void ResetZeroBone2DProperty(GameObject bone)
    {
        Bone2D boneComponent = bone.GetComponent<Bone2D>();
        boneComponent.localLength = 0;
    }

    //因为unity插件的openpose检测效果没有c++的好，所以决定先麻烦一下，c++搞完了读取json文件，然后转换为bone2d
    public static void ChangeKeypoints2Bone2DFromJsonFile()
    {

    }

    private static Vector3 GetDefaultInstantiatePosition()
    {
        Vector3 result = Vector3.zero;

        return result;
    }
}

