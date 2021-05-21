using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 记载骨骼和网格的绑定信息
/// </summary>
public class BindInfo
{
    [Serializable]
    public class BindingInfo : ICloneable
    {
        public Matrix4x4 bindPose;
        public float boneLength;

        public Vector3 position { get { return bindPose.inverse * new Vector4(0f, 0f, 0f, 1f); } }
        public Vector3 endPoint { get { return bindPose.inverse * new Vector4(boneLength, 0f, 0f, 1f); } }

        public string path;
        public string name;

        public Color color;
        public int zOrder;

        public object Clone()
        {
            return this.MemberwiseClone();
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
}
