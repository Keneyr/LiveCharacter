using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System;
using System.Linq;
//namespace Assets.AutoCha2DAni.Scripts
/// <summary>
/// 目前的想法是，编辑器模式下，update来（读取json文件传来的数据，然后计算相关仿射矩阵）带动Scene下的Bone运动
/// Animation Clip窗口开启recored模式，直接记录AnimationClip就好
/// </summary>
[InitializeOnLoad]
[ExecuteInEditMode]
public class PoseRetargetUserScript : MonoBehaviour
{
    //注意队列里存放的骨骼顺序
    public static bool ContollerPoseRetarget = false;
    public static Queue<List<UnityEngine.Vector2>> jsonMotionPointsSequence = new Queue<List<UnityEngine.Vector2>>(); //突然想问Unity是怎么和c#无缝融合的
    public static Queue<List<SkeletonLine>> jsonMotionSkeletonLineCelibratedSequence = new Queue<List<SkeletonLine>>();
    // Thread
    private static Thread opThread;

    [SerializeField]
    static Animation animation = null;

    [SerializeField]
    GameObject targetCharacter = null;

    [SerializeField]
    static GameObject[] m_CharacterBones = new GameObject[13]; //等有时间自定义这块的面板，类似SpriteMeshInstance可以自己拖动列表
    static float[] boneLengths = new float[13];

    private float frameTime = 0f;

    //计算重定向时间
    [SerializeField]
    float retargetingTime = 0f;

    AnimationCurve[] curve_x = new AnimationCurve[13];
    AnimationCurve[] curve_y = new AnimationCurve[13];
    AnimationCurve[] curve_z = new AnimationCurve[13];
    AnimationCurve[] curve_w = new AnimationCurve[13];

    EditorCurveBinding []ecb = new EditorCurveBinding[4];

    string[] propertyNameBinding = 
    {
        "m_LocalPosition.x",
        "m_LocalPosition.y",
        "m_LocalRotation.z",
        "m_LocalRotation.w",
    };
    
    private void OnEnable()
    {
        //查找hierarchy所有的Bone2D物体，赋值
        ApplyBonesToScript();
        ApplyAnimationTargetToScript();
    }
    private void Start()
    {
        //查找hierarchy所有的Bone2D物体，赋值
        ApplyBonesToScript();
        ApplyAnimationTargetToScript();
        
    }
    private void OnDisable()
    {
        //清空曲线
        frameTime = 0f;
        for(int i = 0;i < m_CharacterBones.Length; i++)
        {
            curve_x[i] = null;
            curve_y[i] = null;
            curve_z[i] = null;
            curve_w[i] = null;

        }
        retargetingTime = 0f;
    }

        //尽量减少帧消耗.用一个队列来当缓冲区，队列里面应该放的是第t帧到第t+1帧人体13个骨骼的仿射变换矩阵，update负责有数据就拿，感觉会多线程比较好一点呢..
    private void Update()
    {
        if (ContollerPoseRetarget)
        {
            DriveCharacterBone2DMove();
        }
    }
    //驱动选择的角色的相关骨骼运动,Animation那边手动调好就可以录制AnimationClip了
    private void DriveCharacterBone2DMove()
    {
        //更新Bone2D相关属性
        if (jsonMotionSkeletonLineCelibratedSequence.Count > 1) //不知道这里有主线程和子线程同时操作队列，需不需要考虑锁
        {
            //更新重定向时间
            retargetingTime += Time.deltaTime;

            List<SkeletonLine> currentPose = jsonMotionSkeletonLineCelibratedSequence.Dequeue();
            List<SkeletonLine> nextPose = jsonMotionSkeletonLineCelibratedSequence.Peek();

            Debug.Log("队列第 " + jsonMotionSkeletonLineCelibratedSequence.Count + "帧： ");
            //frameTime += UnityEngine.Time.deltaTime;
            frameTime += 0.1f;
            Debug.Log("frameTime:===========" + frameTime);

            for (int i = 0; i < currentPose.Count; i++)
            {
                SkeletonLineOffset poseofset = new SkeletonLineOffset(nextPose[i].m_startPoint - currentPose[i].m_startPoint,
                                                                      nextPose[i].m_endPoint - currentPose[i].m_endPoint);
                
                //或者在这里就直接更新场景中的Bone2D
                GameObject bone = m_CharacterBones[i];
                //Debug.Log(bone.name + "的起点偏移量是:" + poseofset.m_startOffset.x + " , " + poseofset.m_startOffset.y);
                if (bone)
                {
                    //Debug.Log(bone.name + "之前的位置为：" + bone.transform.position);
                    bone.transform.position = bone.transform.position + new Vector3(poseofset.m_startOffset.x, poseofset.m_startOffset.y, 0);
                    //Debug.Log(bone.name + "变化后的的位置为：" + bone.transform.position);

                    UpdateBone2DComponentProperty(bone, poseofset);
                    //创造一个animation clip把这些骨骼变形信息都存起来
                    //check
                    //if(bone.name == "neck_bone")
                    Create2DCharacterAnimationClip(bone,i,false);
                }
            }
        }
    }
    
    private void Create2DCharacterAnimationClip(GameObject bone,int bone_index,bool isInitFrame)
    {
        //EditorCurveBinding[] ecb = new EditorCurveBinding[4];
        
        //关于旋转角，animation clip想要的应该是0-1的一个四元数,而不是欧拉角
        if (isInitFrame)
        {
            frameTime = 0;
            if (curve_x[bone_index] == null)
                curve_x[bone_index] = new AnimationCurve(); //实例化数组里的每个元素，才能给元素类对象的成员赋值
            if (curve_y[bone_index] == null)
                curve_y[bone_index] = new AnimationCurve(); //实例化数组里的每个元素，才能给元素类对象的成员赋值
            if (curve_z[bone_index] == null)
                curve_z[bone_index] = new AnimationCurve(); //实例化数组里的每个元素，才能给元素类对象的成员赋值
            if (curve_w[bone_index] == null)
                curve_w[bone_index] = new AnimationCurve(); //实例化数组里的每个元素，才能给元素类对象的成员赋值

            for (int i = 0; i < ecb.Length; i++)
            {
                
                ecb[i].propertyName = propertyNameBinding[i];
                ecb[i].type = typeof(Transform);
                
            }
        }
        var root_path = targetCharacter.name + "_bone/";
        string path = root_path + bone.name;

        for (int i = 0;i < ecb.Length; i++)
        {
            ecb[i].path = path;
        }

        //获取animation组件下的clip资源(文件)
        //foreach (AnimationState anima in animation)
        //{
        //    AnimationClip _clip = anima.clip;
        //    //创建x轴位置动画曲线
        //    //curve_x = new AnimationCurve();
        //    curve_x.AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.position.x));
        //    _clip.SetCurve(root_path + bone.name,typeof(Transform), "m_LocalPosition.x", curve_x);

        //    //创建y轴位置动画曲线
        //    //curve_y = new AnimationCurve();
        //    curve_y.AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.position.y));
        //    _clip.SetCurve(root_path + bone.name,typeof(Transform), "m_LocalPosition.y", curve_y);

        //    //创建z轴(欧拉)旋转角动画曲线
        //    //curve_z = new AnimationCurve();
        //    curve_z.AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.rotation.z));
        //    _clip.SetCurve(root_path + bone.name, typeof(Transform), "m_LocalEulerAngles.z", curve_z);
        //    AnimationUtility.SetEditorCurve
        //}

        Debug.Log(bone.name + " " + bone.transform.position.x);
        Debug.Log(bone.name + " " + bone.transform.position.y);
        Debug.Log(bone.name + " " + bone.transform.rotation.z);
        Debug.Log(bone.name + " " + bone.transform.rotation.w); //四元数

        //foreach (AnimationState animaclip in animation)
        //{
        //    var _clip = animaclip.clip;
        //    curve_x[bone_index].AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.position.x));
        //    //_clip.SetCurve(root_path + bone.name, typeof(Transform), "m_LocalPosition.x", curve_x);

        //    curve_y[bone_index].AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.position.y));
        //    //_clip.SetCurve(root_path + bone.name, typeof(Transform), "m_LocalPosition.y", curve_y);

        //    curve_z[bone_index].AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.rotation.z));
        //    //_clip.SetCurve(root_path + bone.name, typeof(Transform), "m_LocalEulerAngles.z", curve_z);

        //    curve_w[bone_index].AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.rotation.w));

            
        //    AnimationUtility.SetEditorCurve(_clip, ecb[0], curve_x[bone_index]);
        //    AnimationUtility.SetEditorCurve(_clip, ecb[1], curve_y[bone_index]);
        //    AnimationUtility.SetEditorCurve(_clip, ecb[2], curve_z[bone_index]);
        //    AnimationUtility.SetEditorCurve(_clip, ecb[3], curve_w[bone_index]);

        //}
        var _clip = animation.clip;
        curve_x[bone_index].AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.position.x));
        //_clip.SetCurve(root_path + bone.name, typeof(Transform), "m_LocalPosition.x", curve_x);

        curve_y[bone_index].AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.position.y));
        //_clip.SetCurve(root_path + bone.name, typeof(Transform), "m_LocalPosition.y", curve_y);

        curve_z[bone_index].AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.rotation.z));
        //_clip.SetCurve(root_path + bone.name, typeof(Transform), "m_LocalEulerAngles.z", curve_z);

        curve_w[bone_index].AddKey(new Keyframe(/*"当前帧时间"*/frameTime, bone.transform.rotation.w));


        AnimationUtility.SetEditorCurve(_clip, ecb[0], curve_x[bone_index]);
        AnimationUtility.SetEditorCurve(_clip, ecb[1], curve_y[bone_index]);
        AnimationUtility.SetEditorCurve(_clip, ecb[2], curve_z[bone_index]);
        AnimationUtility.SetEditorCurve(_clip, ecb[3], curve_w[bone_index]);
    }

    //更新一块2d骨骼的属性
    private void UpdateBone2DComponentProperty(GameObject bone, SkeletonLineOffset poseoffset)
    {
        //端点位置，length，rotation matrix
        //应该是这里引用类型造成所有骨骼的Bone2D组件都被和谐统一了，导致的bug...
        Anima2D.Bone2D boneComponent = bone.GetComponent<Anima2D.Bone2D>();

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
    private void CalculateDeformationMatrix(GameObject bone, Vector3 virtualendPosition)
    {
        //从jsonMotionPointsCelibratedSequence队列中拿骨骼运动矩阵数据
        //但是我看了一下c++代码，我竟然是求的视频前后帧骨骼端点在x和y的offset，然后为了改变皮肤，才求的转换矩阵
        //所以这里先不写，看能不能靠Unity的蒙皮搞起-搞起不了，得求出骨骼的旋转矩阵，不然就是简单的position的改变

        //直接计算t+1帧相对于x轴的旋转矩阵
        Quaternion l_deltaRotation = Quaternion.identity;
        //先看看BoneUtils类中有没有现成的接口--似乎都不太现成，所以先自己写吧
        Anima2D.Bone2D boneComponent = bone.GetComponent<Anima2D.Bone2D>();
        
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
    //自动搜孙hierarchy面板下的bone2D物体，然后程序把这些骨骼赋值给m_CharacterBones，方便人机交互及校准算法计算
    public static void ApplyBonesToScript()
    {
        //依靠标签查找
        //注意这里查找到的骨骼应该按照hierarchy上的的顺序排序
        Debug.Log("在面板上查找标签为bone2D的物体，并监视");
        m_CharacterBones = GameObject.FindGameObjectsWithTag("bone2D").OrderBy(g => g.transform.GetSiblingIndex()).ToArray(); ;
        
        for(int i=0;i<m_CharacterBones.Length;i++)
        {
            Debug.Log(m_CharacterBones[i].name);
            //boneLengths[i] = m_CharacterBones[i].GetComponent<Anima2D.Bone2D>().length;
            boneLengths[i] = m_CharacterBones[i].GetComponent<Anima2D.Bone2D>().localLength;
        }

    }
    public void ApplyAnimationTargetToScript()
    {
        Debug.Log("在面板上查找标签为Character的物体角色，找到其动画并监视");
        targetCharacter = GameObject.FindGameObjectWithTag("Character");
        Debug.Log(targetCharacter.name);

        //targetCharacter = Selection.activeGameObject; //选中的角色

        animation = targetCharacter.GetComponent<Animation>();
        if (!animation)
        {
            animation = targetCharacter.AddComponent<Animation>();
        }
        foreach (AnimationState anima in animation)
        {
            AnimationClip _clip = anima.clip;
            _clip.ClearCurves();
        }
        //创建初始帧
        for (int i = 0; i < m_CharacterBones.Length; i++)
        {
            GameObject bone = m_CharacterBones[i];
            Debug.Log("索引索引索引：" + i);
            Create2DCharacterAnimationClip(bone, i, true);
        }
    }

    //校准算法
    private static void OPExecuteThread()
    {
        int rootpointsindex = 1;
        int[] firstlayerpointsindex = new int[5] { 0, 2, 5, 8, 11 };
        int[] ffirstlayerpointsindex = new int[4] { 2, 5, 8, 11 };
        int[] secondlayerpointsindex = new int[4] { 3, 6, 9, 12 };
        List<float> skeletonRatio = new List<float>();
        List<SkeletonLine> sourceSkeleton = new List<SkeletonLine>();

        int index = 0;
        while (jsonMotionPointsSequence.Count > 0)
        {
            if (index == 0)
            {
                //1. 求出第一帧视频骨骼和二维角色骨骼的比例
                List<UnityEngine.Vector2> pose0 = jsonMotionPointsSequence.Peek(); //只获取top值，并不拿
                for (int i = 0; i < pose0.Count; i++)
                {
                    //root根节点到第一层级关键点的骨骼
                    if (i == rootpointsindex)
                    {
                        for (int j = 0; j < firstlayerpointsindex.Length; j++)
                        {
                            sourceSkeleton.Add(new SkeletonLine(pose0[i], pose0[firstlayerpointsindex[j]]));
                        }

                    }
                    //第一层级关键点到第二层级关键点的骨
                    else if (System.Array.IndexOf(ffirstlayerpointsindex, i) != -1 || System.Array.IndexOf(secondlayerpointsindex, i) != -1)
                    {
                        sourceSkeleton.Add(new SkeletonLine(pose0[i], pose0[i + 1]));
                    }
                }
                //每个骨骼的比例值
                for (int i = 0; i < sourceSkeleton.Count; i++)
                {
                    //注意有的骨骼不存在的情况
                    float l1 = boneLengths[i]; //因为子线程无法使用GetComponent等函数
                    float l2 = sourceSkeleton[i].m_Length;
                    if (l1 == 0 || l2 == 0)
                    {
                        skeletonRatio.Add(1f);
                    }else
                    {
                        skeletonRatio.Add(l1 / l2);
                        //skeletonRatio.Add(1f);
                    }
                }
            }


            //2. 根据第一帧的比例更新视频中关键点轨迹的位置,以胸部为中心向外扩展,更新所有视频帧数据
            //求出骨骼的单位向量，乘以新骨骼的尺寸，加上以胸部为根节点的位置就是，新关键点的位置
            sourceSkeleton.Clear();
            List<UnityEngine.Vector2> pose = jsonMotionPointsSequence.Dequeue();
            for (int i = 0; i < pose.Count; i++)
            {
                
                if (i == rootpointsindex)
                {
                    for (int j = 0; j < firstlayerpointsindex.Length; j++)
                    {
                        sourceSkeleton.Add(new SkeletonLine(pose[i], pose[firstlayerpointsindex[j]]));
                    }
                }
                //第一层级关键点到第二层级关键点的骨骼...第二层级关键点到第三层级关键点的骨骼
                else if (System.Array.IndexOf(ffirstlayerpointsindex, i) != -1 || System.Array.IndexOf(secondlayerpointsindex, i) != -1)
                {
                    sourceSkeleton.Add(new SkeletonLine(pose[i], pose[i + 1]));
                }
            }
#if true

            //2.1 更新视频中每帧骨骼的长度
            for (int j = 0; j < sourceSkeleton.Count; j++)
            {
                sourceSkeleton[j].m_Length = skeletonRatio[j] * sourceSkeleton[j].m_Length;
            }
            //2.2 更新一级关键点--前5个骨骼
            for (int j = 0; j < 5; j++)
            {
#if false
                //我怎么现在觉得这更新方式有问题呢？？
                Vector2 delta = new Vector2(sourceSkeleton[j].m_Length * sourceSkeleton[j].m_unit.x, 
                    sourceSkeleton[j].m_Length * sourceSkeleton[j].m_unit.y);
                sourceSkeleton[j].m_endPoint = sourceSkeleton[j].m_endPoint + delta;
#endif
#if true
                Vector2 delta = new Vector2(Mathf.Cos(sourceSkeleton[j].m_theta) * sourceSkeleton[j].m_Length,
                    Mathf.Sin(sourceSkeleton[j].m_theta) * sourceSkeleton[j].m_Length
                    );
                //sourceSkeleton[j].m_endPoint = sourceSkeleton[j].m_endPoint + delta;
                sourceSkeleton[j].m_endPoint = sourceSkeleton[j].m_startPoint + delta;
#endif
            }
            //2.3 以竖直结构更新二级三级关键点--4个骨骼+4个骨骼=8骨骼
                
            for (int j = 5; j < 13; j += 2)
            {
                sourceSkeleton[j].m_startPoint = sourceSkeleton[(j - 3) / 2].m_endPoint;
                Vector2 delta = new Vector2(Mathf.Cos(sourceSkeleton[j].m_theta) * sourceSkeleton[j].m_Length,
                    Mathf.Sin(sourceSkeleton[j].m_theta) * sourceSkeleton[j].m_Length
                    );
                //sourceSkeleton[j].m_endPoint = sourceSkeleton[j].m_endPoint + delta;
                sourceSkeleton[j].m_endPoint = sourceSkeleton[j].m_startPoint + delta;
            }
            for(int j = 6; j < 13; j += 2)
            {
                sourceSkeleton[j].m_startPoint = sourceSkeleton[j - 1].m_endPoint;
                Vector2 delta = new Vector2(Mathf.Cos(sourceSkeleton[j].m_theta) * sourceSkeleton[j].m_Length,
                   Mathf.Sin(sourceSkeleton[j].m_theta) * sourceSkeleton[j].m_Length
                   );
                //sourceSkeleton[j].m_endPoint = sourceSkeleton[j].m_endPoint + delta;
                sourceSkeleton[j].m_endPoint = sourceSkeleton[j].m_startPoint + delta;
            }
#endif

            //2.5 放入专门盛放校准后的骨骼姿态队列
            //jsonMotionSkeletonLineCelibratedSequence.Enqueue(sourceSkeleton); //这种只是添加的类对象的引用...
            //List<SkeletonLine> sourceSkeletonDeepCopy = Clone<SkeletonLine>(sourceSkeleton);
            List<SkeletonLine> sourceSkeletonDeepCopy = sourceSkeleton.ToList().ConvertAll(b=>b.Clone() as SkeletonLine);
            jsonMotionSkeletonLineCelibratedSequence.Enqueue(sourceSkeletonDeepCopy);

            index++;
        }

        //清空队列
        jsonMotionPointsSequence.Clear();
        sourceSkeleton.Clear();
    }
#if false
    
    public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }

    private static List<T> Clone<T>(List<T> sourceSkeleton)
    {
        using (Stream objectStream = new MemoryStream())
        {
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(objectStream, sourceSkeleton);
            objectStream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(objectStream) as List<T>;
        }
    }

    private static List<T> Clone<T>(System.Object List)
    {
        using (Stream objectStream = new MemoryStream())
        {
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(objectStream, List);
            objectStream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(objectStream) as List<T>;
        }
    }
#endif
    private static void GenerateVideoBonesToScene()
    {
        int rootpointsindex = 1;
        int[] firstlayerpointsindex = new int[5] { 0, 2, 5, 8, 11 };
        int[] ffirstlayerpointsindex = new int[4] { 2, 5, 8, 11 };
        int[] secondlayerpointsindex = new int[4] { 3, 6, 9, 12 };
        List<SkeletonLine> sourceSkeleton = new List<SkeletonLine>();

        int index = 0;
        if (jsonMotionPointsSequence.Count > 0)
        {
            if (index == 0)
            {
                //1. 求出第一帧视频骨骼和二维角色骨骼的比例
                List<UnityEngine.Vector2> pose0 = jsonMotionPointsSequence.Peek(); //只获取top值，并不拿
                for (int i = 0; i < pose0.Count; i++)
                {
                    //root根节点到第一层级关键点的骨骼
                    if (i == rootpointsindex)
                    {
                        for (int j = 0; j < firstlayerpointsindex.Length; j++)
                        {
                            sourceSkeleton.Add(new SkeletonLine(pose0[i], pose0[firstlayerpointsindex[j]]));
                        }

                    }
                    //第一层级关键点到第二层级关键点的骨
                    else if (System.Array.IndexOf(ffirstlayerpointsindex, i) != -1 || System.Array.IndexOf(secondlayerpointsindex, i) != -1)
                    {
                        sourceSkeleton.Add(new SkeletonLine(pose0[i], pose0[i + 1]));
                    }
                }
                //遍历13根骨骼，生成bone2d到场景中
                for (int i = 0; i < sourceSkeleton.Count; i++)
                {
                    GameObject bone = new GameObject();
                    Anima2D.Bone2D boneComponent = bone.AddComponent<Anima2D.Bone2D>();
                    if (sourceSkeleton[i].m_Length == 0)
                    {
                        bone.transform.position = Vector3.zero;
                        boneComponent.globalendPosition = Vector3.zero;
                        boneComponent.globalstartPosition = Vector3.zero;
                        ResetZeroBone2DProperty(bone);
                    }
                    else
                    {
                        bone.transform.position = new Vector3(sourceSkeleton[i].m_startPoint.x, sourceSkeleton[i].m_startPoint.y, 0); //pixelsToUnit
                        boneComponent.globalstartPosition = bone.transform.position;
                        boneComponent.globalendPosition = new Vector3(sourceSkeleton[i].m_endPoint.x, sourceSkeleton[i].m_endPoint.y, 0);
                        UpdateBone2DProperty(bone);
                    }
                }
            }
        }
    }
    private static void UpdateBone2DProperty(GameObject bone)
    {
        Quaternion l_deltaRotation = Quaternion.identity;
        //先看看BoneUtils类中有没有现成的接口--似乎都不太现成，所以先自己写吧
        Anima2D.Bone2D boneComponent = bone.GetComponent<Anima2D.Bone2D>();
        boneComponent.localLength = (boneComponent.globalstartPosition - boneComponent.globalendPosition).magnitude;
        Vector2 localPosition = new Vector2(boneComponent.globalendPosition.x - boneComponent.globalstartPosition.x,
                                            boneComponent.globalendPosition.y - boneComponent.globalstartPosition.y);
        float angle = Mathf.Atan2(localPosition.y, localPosition.x) * Mathf.Rad2Deg;
        l_deltaRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        bone.transform.localRotation *= l_deltaRotation;
    }
    private static void ResetZeroBone2DProperty(GameObject bone)
    {
        Anima2D.Bone2D boneComponent = bone.GetComponent<Anima2D.Bone2D>();
        boneComponent.localLength = 0;
    }

    //从队列中拿出各帧的骨骼关键点，按照第一帧对二维角色骨骼的比例对全帧进行缩放，计算前后帧的仿射矩阵,并放入矩阵队列里，供update使用
    public static void OpJsonMotionPointsSequence()
    {
        //生成视频序列第一帧的骨骼到场景中，为了对比实验截图（无校准前骨骼的样子）
        //GenerateVideoBonesToScene();


        Debug.Log("开启新线程矫正源视频中的运动数据...");
        Debug.Log("清空原始的Animation clip，从头开始录制...");
        opThread = new Thread(new ThreadStart(OPExecuteThread));
        opThread.Start();
    }

}

[InitializeOnLoad]
[System.Serializable]
public class SkeletonLine : ICloneable
{
    public  Vector2 m_startPoint;
    public  Vector2 m_endPoint;

    public  float m_Length = 0.0f;

    //public  Vector2 m_unit; //单位向量

    public  float m_theta; //斜率,用theta来表示

    public SkeletonLine(Vector2 point1, Vector2 point2)
    {
        m_startPoint = point1;
        m_endPoint = point2;
        m_Length = (m_endPoint - m_startPoint).magnitude;
        //m_unit = (m_endPoint - m_startPoint) / m_Length;
        Vector2 delta = m_endPoint - m_startPoint;
        if(delta.x == 0)
        {
            if(delta.y > 0)
                m_theta = Mathf.Deg2Rad * 90; // 90°转换为弧度制
            if(delta.y < 0)
                m_theta = -Mathf.Deg2Rad * 90;
        }
        else
        {
            m_theta = Mathf.Atan2(delta.y, delta.x);
        }
        
    }
    public object Clone()
    {
        return this.MemberwiseClone();
    }
    //public static SkeletonLine operator -(SkeletonLine sl)
    //{
    //    return new SkeletonLine(m_startPoint - sl.m_startPoint, m_endPoint- sl.m_endPoint);
    //}
}

[InitializeOnLoad]
[System.Serializable]
public class SkeletonLineOffset : ICloneable
{
    public Vector2 m_startOffset;
    public Vector2 m_endOffset;
    public SkeletonLineOffset(Vector2 offset1,Vector2 offset2)
    {
        m_startOffset = offset1;
        m_endOffset = offset2;
    }
    public object Clone()
    {
        return this.MemberwiseClone();
    }
}