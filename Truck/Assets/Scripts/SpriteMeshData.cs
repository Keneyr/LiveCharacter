using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class Node : ScriptableObject
{
    public int index = -1;

    public static Node Create(int index)
    {
        Node node = ScriptableObject.CreateInstance<Node>();
        node.hideFlags = HideFlags.DontSave;
        //Node node = new Node();
        node.index = index;
        return node;
    }
}

[Serializable]
public class Edge : ScriptableObject
{
    public Node node1;
    public Node node2;

    public static Edge Create(Node vertex1, Node vertex2)
    {
        Edge edge = ScriptableObject.CreateInstance<Edge>();
        //Edge edge = new Edge();
        edge.hideFlags = HideFlags.DontSave;
        edge.node1 = vertex1;
        edge.node2 = vertex2;

        return edge;
    }

    public bool ContainsNode(Node node)
    {
        return node1 == node || node2 == node;
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Edge p = (Edge)obj;

        return (node1 == p.node1) && (node2 == p.node2) || (node1 == p.node2) && (node2 == p.node1);
    }

    public override int GetHashCode()
    {
        return node1.GetHashCode() ^ node2.GetHashCode();
    }

    public static implicit operator bool(Edge e)
    {
        return e != null;
    }
}


[Serializable]
public struct IndexedEdge
{
    public int index1;
    public int index2;

    public IndexedEdge(int index1, int index2)
    {
        this.index1 = index1;
        this.index2 = index2;
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        IndexedEdge p = (IndexedEdge)obj;

        return (index1 == p.index1) && (index2 == p.index2) || (index1 == p.index2) && (index2 == p.index1);
    }

    public override int GetHashCode()
    {
        return index1.GetHashCode() ^ index2.GetHashCode();
    }
}

//一个顶点最多被4根骨骼所影响，对应的每根骨骼都有其对该顶点的影响力
[SerializeField]
public struct BoneWeight
{
    public float weight0;
    public float weight1;
    public float weight2;
    public float weight3;

    public int boneIndex0;
    public int boneIndex1;
    public int boneIndex2;
    public int boneIndex3;

    public static BoneWeight Create()
    {
        BoneWeight boneWeight = new BoneWeight();
        boneWeight.boneIndex0 = -1;
        boneWeight.boneIndex1 = -1;
        boneWeight.boneIndex2 = -1;
        boneWeight.boneIndex3 = -1;

        return boneWeight;
    }
    public float GetBoneWeight(int boneIndex)
    {
        if (boneIndex0 == boneIndex)
        {
            return weight0;

        }
        else if (boneIndex1 == boneIndex)
        {
            return weight1;
        }
        else if (boneIndex2 == boneIndex)
        {
            return weight2;
        }
        else if (boneIndex3 == boneIndex)
        {
            return weight3;
        }

        return 0f;
    }

    public void SetBoneIndexWeight(int boneIndex,float weight,bool createInfluence = true, bool unassignIfWeightZero = false)
    {
        int weightIndex = GetWeightIndex(boneIndex);
        weight = Mathf.Clamp01(weight);

        if (createInfluence && weightIndex < 0)
        {
            weightIndex = GetMinWeightIndex();
        }

        SetWeight(weightIndex, boneIndex, weight);

        if (unassignIfWeightZero)
        {
            if (weight0 <= 0f)
            {
                Unassign(boneIndex0);
            }
            if (weight1 <= 0f)
            {
                Unassign(boneIndex1);
            }
            if (weight2 <= 0f)
            {
                Unassign(boneIndex2);
            }
            if (weight3 <= 0f)
            {
                Unassign(boneIndex3);
            }
        }
    }
    public void Unassign(int boneIndex)
    {
        SetWeight(GetWeightIndex(boneIndex), -1, 0f);
    }
    public int GetWeightIndex(int boneIndex)
    {
        if (boneIndex >= 0)
        {
            List<int> boneIndexes = new List<int>() { boneIndex0, boneIndex1, boneIndex2, boneIndex3 };

            return boneIndexes.IndexOf(boneIndex);
        }

        return -1;
    }
    public int GetMinWeightIndex()
    {
        List<int> boneIndexes = new List<int>() { boneIndex0, boneIndex1, boneIndex2, boneIndex3 };

        int weightIndex = boneIndexes.IndexOf(boneIndexes.Min()); //判断元素第一次出现索引的位置，从0开始

        return weightIndex;
    }
    public void SetWeight(int weightIndex, int boneIndex, float weight)
    {
        weight = Mathf.Clamp01(weight);

        if (weightIndex == 0)
        {
            boneIndex0 = boneIndex;
            weight0 = weight;

        }
        else if (weightIndex == 1)
        {
            boneIndex1 = boneIndex;
            weight1 = weight;

        }
        else if (weightIndex == 2)
        {
            boneIndex2 = boneIndex;
            weight2 = weight;

        }
        else if (weightIndex == 3)
        {
            boneIndex3 = boneIndex;
            weight3 = weight;
        }

        Normalize(weightIndex);
    }
    public void DeleteBoneIndex(int boneIndex)
    {
        if (boneIndex0 >= boneIndex)
        {
            boneIndex0--;
        }
        if (boneIndex1 >= boneIndex)
        {
            boneIndex1--;
        }
        if (boneIndex2 >= boneIndex)
        {
            boneIndex2--;
        }
        if (boneIndex3 >= boneIndex)
        {
            boneIndex3--;
        }
    }

    void Normalize(int masterIndex)
    {
        if (masterIndex >= 0 && masterIndex < 4)
        {
            float sum = 0f;

            float[] weights = new float[] { weight0, weight1, weight2, weight3 };
            int[] indices = new int[] { boneIndex0, boneIndex1, boneIndex2, boneIndex3 };

            int numValidIndices = 0;

            for (int i = 0; i < 4; ++i)
            {
                if (indices[i] >= 0)
                {
                    if (i != masterIndex)
                    {
                        numValidIndices++;
                        sum += weights[i];
                    }
                }
            }

            float targetSum = 1f - weights[masterIndex];

            if (numValidIndices > 0)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (i != masterIndex && indices[i] >= 0)
                    {
                        if (sum > 0f)
                        {
                            weights[i] = weights[i] * targetSum / sum;
                        }
                        else if (numValidIndices > 0)
                        {
                            weights[i] = targetSum / numValidIndices;
                        }
                    }
                }

                weight0 = weights[0];
                weight1 = weights[1];
                weight2 = weights[2];
                weight3 = weights[3];
            }
        }
    }
}
/// <summary>
/// 记载骨骼和网格的绑定信息
/// </summary>

[Serializable]
public class BindingInfo : ICloneable
{
    public Matrix4x4 bindPose; //引用类型
    public float boneLength;

    public Vector3 position { get { return bindPose.inverse * new Vector4(0f, 0f, 0f, 1f); } }
    public Vector3 endPoint { get { return bindPose.inverse * new Vector4(boneLength, 0f, 0f, 1f); } }

    public string path; //可以看做值类型
    public string name;

    public Color color;
    public int zOrder; //值类型

    public object Clone()
    {
        return this.MemberwiseClone(); //浅拷贝
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        BindingInfo p = (BindingInfo)obj;

        return Mathf.Approximately((position - p.position).sqrMagnitude, 0f) && Mathf.Approximately((endPoint - p.endPoint).sqrMagnitude, 0f);
    }

    public override int GetHashCode()
    {
        return position.GetHashCode() ^ endPoint.GetHashCode();
    }

    public static implicit operator bool(BindingInfo b)
    {
        return b != null;
    }
}

public class Bone2D : MonoBehaviour
{
    [SerializeField]
    private Color m_Color = Color.white;

    [SerializeField]
    private float m_Length = 1f;

    [SerializeField]
    private Vector3 m_StartPointPosition; //骨骼的起始端点

    [SerializeField]
    private Vector3 m_EndPointPosition; //骨骼的结束端点

    [SerializeField]
    private Transform m_ChildTransform;

    //[SerializeField]
    //private Bone2D m_CachedChild;

    //[SerializeField]
    //private Bone2D m_ParentBone = null;

    public Color color
    {
        get
        {
            return m_Color;
        }
        set
        {
            m_Color = value;
        }
    }
    public Vector3 globalstartPosition
    {
        get { return transform.position; }
        set { m_StartPointPosition = value; }
    }
    public Vector3 globalendPosition
    {
        get { return m_EndPointPosition; }
        set { m_EndPointPosition = value; }
    }
    public Vector3 localEndPosition
    {
        get
        {
            return Vector3.right * localLength;
        }
    }
    public float localLength
    {
        get
        {
            return m_Length;
        }
        set
        {
            m_Length = value;
        }
    }
    public float length
    {
        get
        {
            return transform.TransformVector(localEndPosition).magnitude;
        }
    }
}

[Serializable]
public class Hole : ICloneable
{
    public Vector3 vertex = Vector3.zero;

    public Hole(Vector3 vertex)
    {
        this.vertex = vertex;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }

    public static implicit operator bool(Hole h)
    {
        return h != null;
    }
}

[Serializable]
public class SpriteMeshData : ScriptableObject
{
    [SerializeField]
    string m_Name = ""; //ce_Data

    [SerializeField]
    string m_MeshName = ""; //ce

    [SerializeField]
    Vector2[] m_Vertices = new Vector2[0]; //顶点

    [SerializeField]
    IndexedEdge[] m_Edges = new IndexedEdge[0]; //轮廓边

    [SerializeField]
    int[] m_Indices = new int[0]; //三角网格边

    [SerializeField]
    BoneWeight[] m_BoneWeights = new BoneWeight[0]; //每一个网格顶点都会有一个蒙皮权重信息

    [SerializeField]
    BindingInfo[] m_BindPoses = new BindingInfo[0]; //绑定骨骼的一些信息

    [SerializeField]
    Mesh m_SharedMesh = new Mesh(); //用于和Unity自带的骨骼动画的一些参数 及 渲染组件 交涉

    public Vector2[] vertices
    {
        get
        {
            return m_Vertices;
        }
        set
        {
            m_Vertices = value;
        }
    }
    
    public IndexedEdge[] edges
    {
        get
        {
            return m_Edges;
        }
        set
        {
            m_Edges = value;
        }
    }
    public string name
    {
        get
        {
            return m_Name;
        }
        set
        {
            m_Name = value;
        }
    }
    public string meshname
    {
        get
        {
            return m_MeshName;
        }
        set
        {
            m_MeshName = value;
        }
    }
    public int[] indices
    {
        get
        {
            return m_Indices;
        }
        set
        {
            m_Indices = value;
        }
    }
    public BoneWeight[] boneWeights
    {
        get
        {
            return m_BoneWeights;
        }
        set
        {
            m_BoneWeights = value;
        }
    }
    public BindingInfo[] bindPoses
    {
        get
        {
            return m_BindPoses;
        }
        set
        {
            m_BindPoses = value;
        }
    }

    public Mesh sharedMesh
    {
        get
        {
            return m_SharedMesh;
        }
        set
        {
            m_SharedMesh = value;
        }
    }
}
