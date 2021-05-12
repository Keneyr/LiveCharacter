using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

[Serializable]
public class SpriteMeshData : ScriptableObject
{
    [SerializeField]
    string m_Name = "";

    [SerializeField]
    Vector2[] m_Vertices = new Vector2[0];

    [SerializeField]
    IndexedEdge[] m_Edges = new IndexedEdge[0];

    
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
}
