using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 默认是draw overlay 以及 draw pies 五彩斑斓的好看的
/// </summary>

public class DrawSkinning : MonoBehaviour
{
    static RectTransform rt;
    static Camera SkinningCamera;
    static RawImage rawImage;
    static int layer = 4;
    RenderTexture renderTargetTexture;

    static Material meshMaterial;
    static Material pieMaterial;
    static Material boneMaterial;
    static Material capMaterial;
    static List<Node> nodes = new List<Node>();
    static List<Vector2> m_TexVertices = new List<Vector2>();
    static List<Edge> edges = new List<Edge>();
    static List<BoneWeight> boneWeights = new List<BoneWeight>();
    static List<BindingInfo> bindPoses = new List<BindingInfo>();
    static List<Bone2D> Bones = new List<Bone2D>();

    static List<Color> m_BindPoseColors = new List<Color>();
    static List<Color> colors = new List<Color>();//for nodes
    static Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        CreateSkinMaterial();
        SkinningCamera = GameObject.Find("SkinningCamera").GetComponent<Camera>();
        Extra.InitCamera(SkinningCamera, layer);
        renderTargetTexture = new RenderTexture(256, 256, 24);
        SkinningCamera.targetTexture = renderTargetTexture;
        rawImage = GetComponent<RawImage>();
        rawImage.texture = renderTargetTexture;
        rt = GetComponent<RectTransform>();
    }
    public void OnRenderObject()
    {
        if (m_TexVertices.Count == 0)
            return;
        meshMaterial.SetPass(0);
        Graphics.DrawMeshNow(mesh, new Vector3(0, 0, layer), Quaternion.identity);
        foreach (Node node in nodes)
        {
            Vector3 position = m_TexVertices[node.index];
            position.z = layer;
            BoneWeight boneWeigth = boneWeights[node.index];
            Extra.DrawPie(position, boneWeigth, 0.1f, pieMaterial, m_BindPoseColors);
        }

        for (int i = Bones.Count-1; i >=0; i--)
        {
            Bone2D bone = Bones[i];
            if (bone)
            {
                Extra.DrawBoneBody(bone, boneMaterial, layer);
                Extra.DrawBoneCap(bone, capMaterial, layer);
            }
        }

    }
    public static void ResetPastCharacterInfo()
    {

    }
    public static void InitBindingInfo(SpriteMeshData spriteMeshData)
    {
        m_TexVertices.Clear();
        nodes.Clear();
        edges.Clear();
        boneWeights.Clear();
        bindPoses.Clear();
        m_BindPoseColors.Clear();

        m_TexVertices = spriteMeshData.vertices.ToList();
        nodes = m_TexVertices.ConvertAll(v => Node.Create(m_TexVertices.IndexOf(v)));
        edges = spriteMeshData.edges.ToList().ConvertAll(e => Edge.Create(nodes[e.index1], nodes[e.index2]));
        boneWeights = spriteMeshData.boneWeights.ToList();
        bindPoses = spriteMeshData.bindPoses.ToList();
        m_BindPoseColors = bindPoses.ConvertAll(b => b.color);

        //make node colors
        colors = new List<Color>(nodes.Count);
        foreach (Node node in nodes)
        {
            BoneWeight boneWeight = boneWeights[node.index];
            int boneIndex0 = boneWeight.boneIndex0;
            int boneIndex1 = boneWeight.boneIndex1;
            int boneIndex2 = boneWeight.boneIndex2;
            int boneIndex3 = boneWeight.boneIndex3;
            float weight0 = boneIndex0 < 0 ? 0f : boneWeight.weight0;
            float weight1 = boneIndex1 < 0 ? 0f : boneWeight.weight1;
            float weight2 = boneIndex2 < 0 ? 0f : boneWeight.weight2;
            float weight3 = boneIndex3 < 0 ? 0f : boneWeight.weight3;

            Color vertexColor = m_BindPoseColors[Mathf.Max(0, boneIndex0)] * weight0 +
                m_BindPoseColors[Mathf.Max(0, boneIndex1)] * weight1 +
                    m_BindPoseColors[Mathf.Max(0, boneIndex2)] * weight2 +
                    m_BindPoseColors[Mathf.Max(0, boneIndex3)] * weight3;

            colors.Add(vertexColor);

            Console.Log("Node Count: " + nodes.Count);
        }
        //set mesh
        mesh = spriteMeshData.sharedMesh;
        mesh.colors = colors.ToArray();
        //
        Bones.Clear();
        Bones = FindComponentsOfType<Bone2D>().ToList();
        for (int i = 0; i < Bones.Count; i++)
        {
            Bones[i].color = m_BindPoseColors[Bones.Count-1-i];
        }
    }
    static T[] FindComponentsOfType<T>() where T : Component
    {
        return GameObject.FindObjectsOfType<T>();
    }
    void CreateSkinMaterial()
    {
        if (!meshMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            meshMaterial = new Material(shader);
            meshMaterial.hideFlags = HideFlags.HideAndDontSave;

            //cyan color
            meshMaterial.SetColor("_Color", Color.white);

            // Turn on alpha blending
            meshMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            meshMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            meshMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            meshMaterial.SetInt("_ZWrite", 0);
        }
        if (!pieMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            pieMaterial = new Material(shader);
            pieMaterial.hideFlags = HideFlags.HideAndDontSave;

            //cyan color
            pieMaterial.SetColor("_Color", Color.white);
            // Turn on alpha blending
            pieMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            pieMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            pieMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            pieMaterial.SetInt("_ZWrite", 0);
        }
        if (!boneMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            boneMaterial = new Material(shader);
            boneMaterial.hideFlags = HideFlags.HideAndDontSave;

            //cyan color
            boneMaterial.SetColor("_Color", Color.white);

            // Turn on alpha blending
            boneMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            boneMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            boneMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            boneMaterial.SetInt("_ZWrite", 0);
        }
        if (!capMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            capMaterial = new Material(shader);
            capMaterial.hideFlags = HideFlags.HideAndDontSave;

            //cyan color
            capMaterial.SetColor("_Color", Color.white);

            // Turn on alpha blending
            capMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            capMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            capMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            capMaterial.SetInt("_ZWrite", 0);
        }
    }
}
