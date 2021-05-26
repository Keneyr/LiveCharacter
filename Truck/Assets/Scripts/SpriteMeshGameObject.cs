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
    Material[] m_Materials;

    [SerializeField]
    int m_SortingOrder = 0;

    [SerializeField]
    Transform[] m_BoneTransforms; //骨骼的transform组件，每有一个transform组件，就应该对应一个Bone2D

    List<Bone2D> m_CachedBones = new List<Bone2D>(); //自定义的Bone2D
    
    SkinnedMeshRenderer mCachedSkinnedRenderer; //要添加的组件

   
    Texture2D spriteTexture
    {
        get
        {
            if(spriteMeshData && spriteMeshData.sprite)
            {
                return spriteMeshData.sprite.texture;
            }
            return null;
        }
    }

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
    public Material[] sharedMaterials
    {
        get
        {
            return m_Materials;
        }
        set
        {
            m_Materials = value;
        }
    }
    MaterialPropertyBlock m_MaterialPropertyBlock;
    MaterialPropertyBlock materialPropertyBlock
    {
        get
        {
            if (m_MaterialPropertyBlock == null)
            {
                m_MaterialPropertyBlock = new MaterialPropertyBlock();
            }

            return m_MaterialPropertyBlock;
        }
    }

    Mesh m_InitialMesh = null;
    Mesh m_CurrentMesh = null;
    public Mesh sharedMesh
    {
        get
        {
            if(m_InitialMesh)
            {
                return m_InitialMesh;
            }
            return null;
        }
    }
    public Mesh mesh
    {
        get
        {
            if(m_CurrentMesh)
            {
                return GameObject.Instantiate(m_CurrentMesh);
            }
            return null;
        }
    }
    private void OnDestroy()
    {
        Destroy(m_CurrentMesh);
    }

    private void Awake()
    {
        //UpdateCurrentMesh();
    }
    
    void UpdateCurrentMesh()
    {
        UpdateInitialMesh();
        if (m_InitialMesh)
        {
            m_InitialMesh.MarkDynamic();
            m_InitialMesh.UploadMeshData(true);
            m_InitialMesh.hideFlags = HideFlags.DontSave;

            if (!m_CurrentMesh)
            {
                m_CurrentMesh = new Mesh();
                m_CurrentMesh.hideFlags = HideFlags.DontSave;
                m_CurrentMesh.MarkDynamic(); //说明这个mesh是经常update的
            }
            m_CurrentMesh.Clear();
            m_CurrentMesh.UploadMeshData(true);
            m_CurrentMesh = spriteMeshData.sharedMesh;
            //m_CurrentMesh.name = m_InitialMesh.name;
            //m_CurrentMesh.vertices = m_InitialMesh.vertices;
            //m_CurrentMesh.uv = m_InitialMesh.uv;
            //m_CurrentMesh.normals = m_InitialMesh.normals;
            //m_CurrentMesh.tangents = m_InitialMesh.tangents;
            //m_CurrentMesh.boneWeights = m_InitialMesh.boneWeights;
            //m_CurrentMesh.bindposes = m_InitialMesh.bindposes;
            //m_CurrentMesh.bounds = m_InitialMesh.bounds;
            //m_CurrentMesh.colors = m_InitialMesh.colors;

            //for (int i = 0; i < m_InitialMesh.subMeshCount; ++i)
            //{
            //    m_CurrentMesh.SetTriangles(m_CurrentMesh.GetTriangles(i), i);
            //}
            m_CurrentMesh.hideFlags = HideFlags.DontSave;
        }
        else
        {
            m_InitialMesh = null;
            if(m_CurrentMesh)
            {
                m_CurrentMesh.Clear();
            }
        }
        if(m_CurrentMesh)
        {
            if(spriteMeshData && spriteMeshData.sprite && spriteMeshData.sprite.packed)
            {
                SetSpriteUVs(m_CurrentMesh,spriteMeshData.sprite);
            }
            m_CurrentMesh.UploadMeshData(false);
            m_InitialMesh.UploadMeshData(false);
        }
        UpdateRenderers();
    }

    void SetSpriteUVs(Mesh mesh,Sprite sprite)
    {
        Vector2[] spriteUVs = sprite.uv;
        if(mesh.vertexCount == spriteUVs.Length)
        {
            mesh.uv = sprite.uv;
        }
    }
    void UpdateInitialMesh()
    {
        m_InitialMesh = null;
        if(spriteMeshData && spriteMeshData.sharedMesh)
        {
            m_InitialMesh = spriteMeshData.sharedMesh;
        }
    }
    

    //更新网格
    void UpdateRenderers()
    {
        Mesh l_mesh = null;

        if(m_InitialMesh)
        {
            l_mesh = m_CurrentMesh;

        }
        if (cachedSkinnedRenderer)
        {
            cachedSkinnedRenderer.sharedMesh = l_mesh;
        }
    }

    void LateUpdate()
    {
        if(!spriteMeshData || (spriteMeshData && spriteMeshData.sharedMesh != m_InitialMesh))
        {
            UpdateCurrentMesh();
        }
    }
    void OnWillRenderObject()
    {
        UpdateRenderers();
        if (spriteTexture)
        {
            materialPropertyBlock.SetTexture("_MainTex", spriteTexture);
        }

    }



}
