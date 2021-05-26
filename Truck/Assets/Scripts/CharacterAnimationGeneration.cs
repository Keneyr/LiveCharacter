using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责动画生成的最后一步，当内存中存在有效运动数据、角色蒙皮数据，开始重定向
/// </summary>
public class CharacterAnimationGeneration : MonoBehaviour
{
    public static bool StartDriving = false;
    static int startIndex;
    static int endIndex;
    private void Update()
    {
        if(StartDriving)
        {
            ApplyCalibratedMotionData2InitialCharacter();
        }
    }

    public static AnimationGenerationResult GenerateAnimation()
    {
        //判断准备数据是否已到位
        if (!CharacterPreprocessing.spriteMeshData
            || CharacterPreprocessing.spriteMeshData.bindPoses.Length <= 0)
            return AnimationGenerationResult.SpriteMeshDataError;

        if (!CharacterPreprocessing.spriteMeshGO
            || CharacterPreprocessing.spriteMeshGO.bones.Count <= 0
            || !CharacterPreprocessing.spriteMeshGO.cachedSkinnedRenderer)
            return AnimationGenerationResult.SpriteMeshInstanceError;

        if (VideoPreprocessing.calibratedSkeletonQueue.Count <= 0)
            return AnimationGenerationResult.VideoDataError;

        //根据UI上的开始帧、结束帧和帧间隔，来提取队列中的数据，获取其偏移量
        //根据二维角色归一化的ratio，来还原二维角色应该进行的动作,驱动其原始骨骼形变
        //将归一化的运动数据和还原后的运动数据记载到本地

        ExtractEffectiveMotionData2Character();

        return AnimationGenerationResult.Success;
    }
    static void ExtractEffectiveMotionData2Character()
    {
        //默认keyPoints存储的是从0帧到结尾帧，并且帧间隔是2的
        //后期改，判断keyPoints的长度，文件名间隔等
        float starframe = VideoSliderController.instance.sliderStartFrame.value;
        float endframe = VideoSliderController.instance.sliderEndFrame.value;
        float frameInterval = VideoSliderController.instance.sliderInterval.value;

        startIndex = (int)(starframe / frameInterval); //比如初始帧是8，那么在队列里的索引就是4
        endIndex = (int)(endframe / frameInterval);

        if(endIndex < VideoPreprocessing.calibratedSkeletonQueue.Count) //否则越界
        {
            StartDriving = true;
        }
    }
    static void ApplyCalibratedMotionData2InitialCharacter()
    {
        if(VideoPreprocessing.calibratedSkeletonQueue.Count > 1)
        {
            Pose currentPose = VideoPreprocessing.calibratedSkeletonQueue.Dequeue();
            Pose nextPose = VideoPreprocessing.calibratedSkeletonQueue.Peek();

            for(int i=0;i<currentPose.m_bones.Count;i++)
            {
                    SkeletonLineOffset poseofset = new SkeletonLineOffset
                    (nextPose.m_bones[i].m_startPoint - currentPose.m_bones[i].m_startPoint,
                     nextPose.m_bones[i].m_endPoint - currentPose.m_bones[i].m_endPoint);

                Bone2D bone = CharacterPreprocessing.BonesInHierarchy[i];
                if(bone)
                {
                    bone.transform.position = bone.transform.position + new Vector3(poseofset.m_startOffset.x, poseofset.m_startOffset.y, 0);
                    UpdateBone2DComponentProperty(bone,poseofset);
                    //Create2DCharacterAnimationClip(bone,i,false);

                }
            }
        }
    }

    static void UpdateBone2DComponentProperty(Bone2D bone,SkeletonLineOffset poseoffset)
    {
        //端点位置，length，rotation matrix
        //应该是这里引用类型造成所有骨骼的Bone2D组件都被和谐统一了，导致的bug...
        Bone2D boneComponent = bone.GetComponent<Bone2D>();

        boneComponent.globalstartPosition = bone.transform.position;

        //为什么要求虚拟的端点位置，因为怕更新了以后，再更改旋转矩阵，直接又一次主动更新端点位置到别处
        Vector3 virtualendPosition = boneComponent.globalendPosition + new Vector3(poseoffset.m_endOffset.x, poseoffset.m_endOffset.y);
        boneComponent.localLength = (boneComponent.globalstartPosition - virtualendPosition).magnitude;

        //注意这里骨骼端点位置更新的位移是和起始点一样的， 符合先平移再旋转的理论
        boneComponent.globalendPosition = boneComponent.globalendPosition + new Vector3(poseoffset.m_startOffset.x, poseoffset.m_startOffset.y);

        //计算仿射变换矩阵--应该在角色骨骼上计算,并更新角色骨骼的旋转矩阵
        CalculateDeformationMatrix(bone, virtualendPosition);
    }
    //计算仿射变换矩阵--这里计算从t到t+1帧的旋转矩阵，还是直接计算t+1帧相对于x轴的旋转矩阵，要分清...
    static private void CalculateDeformationMatrix(Bone2D bone, Vector3 virtualendPosition)
    {
        //从jsonMotionPointsCelibratedSequence队列中拿骨骼运动矩阵数据
        //但是我看了一下c++代码，我竟然是求的视频前后帧骨骼端点在x和y的offset，然后为了改变皮肤，才求的转换矩阵
        //所以这里先不写，看能不能靠Unity的蒙皮搞起-搞起不了，得求出骨骼的旋转矩阵，不然就是简单的position的改变

        //直接计算t+1帧相对于x轴的旋转矩阵
        Quaternion l_deltaRotation = Quaternion.identity;
        //先看看BoneUtils类中有没有现成的接口--似乎都不太现成，所以先自己写吧
        Bone2D boneComponent = bone.GetComponent<Bone2D>();

        //boneComponent.localLength = (boneComponent.globalstartPosition - boneComponent.globalendPosition).magnitude;
        Vector2 localPosition = new Vector2(virtualendPosition.x - boneComponent.globalstartPosition.x,
                                             virtualendPosition.y - boneComponent.globalstartPosition.y);
        float angle = Mathf.Atan2(localPosition.y, localPosition.x) * Mathf.Rad2Deg;
        //Debug.Log("骨骼" + bone.name + "的角度角度应该是：" + angle);
        l_deltaRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //bone.transform.localRotation *= l_deltaRotation; //经过试验，发现还是这个是相对好一点的...

        bone.transform.localRotation = l_deltaRotation;
        //旋转完了要再次更新bone的尾巴端点的位置
        boneComponent.globalendPosition = virtualendPosition;
    }
}


