﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// bbw权重绑定算法，用c++封成了dll，供c#调用
/// </summary>

public class BbwPlugin
{

#if true
    [DllImport("Anima2D")]
    private static extern int Bbw(int iterations,
        [In, Out] IntPtr vertices, int vertexCount, int originalVertexCount,
        [In, Out] IntPtr indices, int indexCount,
        [In, Out] IntPtr controlPoints, int controlPointsCount,
        [In, Out] IntPtr boneEdges, int boneEdgesCount,
        [In, Out] IntPtr pinIndices, int pinIndexCount,
        [In, Out] IntPtr weights
        );
#endif


    /*
             * 第一个参数：顶点
             * 第个参数：边
             * 第三个参数：骨骼的两个端点做控制点
             * 第四个参数：骨骼
             * 第五个参数：
        */

    //自己写一个权重自动计算的方法
    //private static void Bbw(int iterations,
    //    [In, Out] IntPtr vertices, int vertexCount, int originalVertexCount,
    //    [In, Out] IntPtr indices, int indexCount,
    //    [In, Out] IntPtr controlPoints, int controlPointsCount,
    //    [In, Out] IntPtr boneEdges, int boneEdgesCount,
    //    [In, Out] IntPtr pinIndices, int pinIndexCount,
    //    [In, Out] IntPtr weights)
    //{

    //}

    public static UnityEngine.BoneWeight[] CalculateBbw(Vector2[] vertices, IndexedEdge[] edges, Vector2[] controlPoints, IndexedEdge[] controlPointEdges, int[] pins)
    {
        Vector2[] sampledEdges = SampleEdges(controlPoints, controlPointEdges, 10);

        List<Vector2> verticesAndSamplesList = new List<Vector2>(vertices.Length + sampledEdges.Length);

        //AddRange类似resize，重新分配这么大的内存空间
        verticesAndSamplesList.AddRange(vertices);
        verticesAndSamplesList.AddRange(controlPoints);
        verticesAndSamplesList.AddRange(sampledEdges);

        List<IndexedEdge> edgesList = new List<IndexedEdge>(edges);
        List<Hole> holes = new List<Hole>();
        List<int> indicesList = new List<int>();

        CharacterPreprocessing.Tessellate(verticesAndSamplesList, edgesList, holes, indicesList, 4f);

        Vector2[] verticesAndSamples = verticesAndSamplesList.ToArray();
        int[] indices = indicesList.ToArray();

        UnityEngine.BoneWeight[] weights = new UnityEngine.BoneWeight[vertices.Length];

        //封装好的指针
        GCHandle verticesHandle = GCHandle.Alloc(verticesAndSamples, GCHandleType.Pinned);
        GCHandle indicesHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
        GCHandle controlPointsHandle = GCHandle.Alloc(controlPoints, GCHandleType.Pinned);
        GCHandle boneEdgesHandle = GCHandle.Alloc(controlPointEdges, GCHandleType.Pinned);
        GCHandle pinsHandle = GCHandle.Alloc(pins, GCHandleType.Pinned);
        GCHandle weightsHandle = GCHandle.Alloc(weights, GCHandleType.Pinned);

        Bbw(-1,
            verticesHandle.AddrOfPinnedObject(), verticesAndSamples.Length, vertices.Length,
            indicesHandle.AddrOfPinnedObject(), indices.Length,
            controlPointsHandle.AddrOfPinnedObject(), controlPoints.Length,
            boneEdgesHandle.AddrOfPinnedObject(), controlPointEdges.Length,
            pinsHandle.AddrOfPinnedObject(), pins.Length,
            weightsHandle.AddrOfPinnedObject());

        verticesHandle.Free();
        indicesHandle.Free();
        controlPointsHandle.Free();
        boneEdgesHandle.Free();
        pinsHandle.Free();
        weightsHandle.Free();

        return weights;
    }
    static Vector2[] SampleEdges(Vector2[] controlPoints, IndexedEdge[] controlPointEdges, int samplesPerEdge)
    {
        List<Vector2> sampledVertices = new List<Vector2>();

        for (int i = 0; i < controlPointEdges.Length; i++)
        {
            IndexedEdge edge = controlPointEdges[i];

            Vector2 tip = controlPoints[edge.index1];
            Vector2 tail = controlPoints[edge.index2];

            for (int s = 0; s < samplesPerEdge; s++)
            {
                float f = (s + 1f) / (float)(samplesPerEdge + 1f);
                sampledVertices.Add(f * tail + (1f - f) * tip);
            }
        }

        return sampledVertices.ToArray();
    }
}
