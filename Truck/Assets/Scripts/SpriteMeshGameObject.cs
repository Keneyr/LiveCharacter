using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameObject可以挂载的组件，类似于Anima2D的SpriteMeshInstance,处理SpriteMeshData和Bone2D和SkinnedMeshRenderer
/// 必须要有这个，因为这个类和渲染组件交互，渲染上会舒服很多
/// </summary>


public class SpriteMeshGameObject : MonoBehaviour
{
    [SerializeField]
    SpriteMeshData m_SpriteMeshData;

    [SerializeField]
    int m_SortingOrder = 0;

    [SerializeField]
    Transform[] m_BoneTransforms; //骨骼的transform组件，每有一个transform组件，就应该对应一个Bone2D

    List<Bone2D> m_CachedBones = new List<Bone2D>(); //自定义的Bone2D
    
    SkinnedMeshRenderer mCachedSkinnedRenderer; //要添加的组件

    [SerializeField]
    Material[] m_Materials;

    public SpriteMeshData spriteMeshData
    {
        get
        {
            return m_SpriteMeshData;
        }
        set
        {
            m_SpriteMeshData = value;
        }
    }
    public int sortingOrder
    {
        get
        {
            return m_SortingOrder;
        }
        set
        {
            m_SortingOrder = value;
        }
    }

    public List<Bone2D> bones
    {
        get
        {
            if(m_BoneTransforms!=null && m_CachedBones.Count != m_BoneTransforms.Length)
            {
                m_CachedBones = new List<Bone2D>(m_BoneTransforms.Length);
                for(int i=0;i<m_BoneTransforms.Length;i++)
                {
                    Bone2D l_Bone = null;
                    if(m_BoneTransforms[i])
                    {
                        l_Bone = m_BoneTransforms[i].GetComponent<Bone2D>();
                    }
                    m_CachedBones.Add(l_Bone);
                }
            }
            for (int i = 0; i < m_CachedBones.Count; i++)
            {
                if (m_CachedBones[i] && m_BoneTransforms[i] != m_CachedBones[i].transform)
                {
                    m_CachedBones[i] = null;
                }
                if (!m_CachedBones[i] && m_BoneTransforms[i])
                {
                    m_CachedBones[i] = m_BoneTransforms[i].GetComponent<Bone2D>();
                }
            }

            return m_CachedBones;
        }
        set
        {
            m_CachedBones = new List<Bone2D>(value);
            m_BoneTransforms = new Transform[m_CachedBones.Count];

            for (int i = 0; i < m_CachedBones.Count; i++)
            {
                Bone2D bone = m_CachedBones[i];
                if (bone)
                {
                    m_BoneTransforms[i] = bone.transform;
                }
            }

            if (cachedSkinnedRenderer)
            {
                cachedSkinnedRenderer.bones = m_BoneTransforms;
            }
        }
    }
    
    public SkinnedMeshRenderer cachedSkinnedRenderer
    {
        get
        {
            if(!mCachedSkinnedRenderer)
            {
                mCachedSkinnedRenderer = GetComponent<SkinnedMeshRenderer>();
            }
            return mCachedSkinnedRenderer;
        }
    }

    //private void LateUpdate()
    //{
    //    spriteMeshData.sharedMesh.vertices = spriteMeshData.vertices;
    //    spriteMesh
    //}

    public Material sharedMaterial
    {
        get
        {
            if (m_Materials.Length > 0)
            {
                return m_Materials[0];
            }
            return null;
        }
        set
        {
            m_Materials = new Material[] { value };
        }
    }

    void OnWillRenderObject()
    {
        UpdateRenderers();
    }

    //更新网格
    void UpdateRenderers()
    {
        Mesh l_mesh = null;

        if (cachedSkinnedRenderer)
        {
            cachedSkinnedRenderer.sharedMesh = spriteMeshData.sharedMesh;
        }

       
    }



}
