using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
    [Header("Procedural Mesh Settings")]
    public bool autoUpdate = true;
    public Material material;
    public bool showBoundingBox = false;
    public bool showNormal = false;
    [ReadOnly] public MeshBoundingBox boundingBox;
    [ReadOnly] public MeshInfo meshInfo;

    protected MeshRenderer meshRenderer;
    protected MeshFilter meshFilter;
    protected Mesh mesh;

    protected virtual void Start() { Generate(); }

    protected virtual void OnValidate()
    {
        if (autoUpdate) 
        {
            UpdateGenerate();
        } 
    }

    protected virtual void Generate()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;
    }

    protected virtual void OnDrawGizmos()
    {
        ShowNormal();
        ShowBound();
    }

    protected void ShowBound()
    {
        if (showBoundingBox)
        {
            boundingBox.DrawBoundingBox();
        }
    }

    protected void ShowNormal(float size = 1) 
    {
        if (showNormal) 
        {
            for (int i = 0; i < meshFilter.sharedMesh.normals.Length; i++)
            {
                Gizmos.DrawRay(transform.position + meshFilter.sharedMesh.vertices[i] * size, meshFilter.sharedMesh.normals[i]);
            }
        }
    }

    public void UpdateGenerate() 
    {
        Generate();

        if (!Application.isPlaying) 
        {
            meshInfo = new MeshInfo(mesh);
            if (showBoundingBox)
            {
                boundingBox = new MeshBoundingBox(transform,
                meshFilter.sharedMesh.bounds.center + transform.position,
                meshFilter.sharedMesh.bounds.size / 2);
            }
        }
        
    }
}

public class ProceduralMeshPart
{
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Color> colors = new List<Color>();

    public Vector3[] Vertices;
    public int[] Triangles;
    public Vector2[] UVs;
    public Color[] Colors;

    public void FillArrays()
    {
        Vertices = vertices.ToArray();
        UVs = uvs.ToArray();
        Triangles = triangles.ToArray();
        Colors = colors.ToArray();
    }

    public ProceduralMeshPart() { }

    //路线1：直接作为一个Submesh
    public ProceduralMeshPart(Vector3[] vertices, int[] triangles, Vector2[] uvs, Color[] colors)
    {
        Vertices = vertices;
        Triangles = triangles;
        UVs = uvs;
        Colors = colors;
    }

    //路线2：拥有多个Submesh的Mesh
    public void AddMeshPart(ProceduralMeshPart meshPart)
    {
        for (int i = 0; i < meshPart.Vertices.Length; i++)
        {
            vertices.Add(meshPart.Vertices[i]);
        }

        for (int i = 0; i < meshPart.Triangles.Length; i++)
        {
            triangles.Add(meshPart.Triangles[i]);
        }

        for (int i = 0; i < meshPart.UVs.Length; i++)
        {
            uvs.Add(meshPart.UVs[i]);
        }

        for (int i = 0; i < meshPart.Colors.Length; i++)
        {
            colors.Add(meshPart.Colors[i]);
        }
    }

    //路线3：从内部构建(无UV版)
    public void AddTriangle(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(point1);
        vertices.Add(point2);
        vertices.Add(point3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    public void AddQuad(Vector3 lb, Vector3 lt, Vector3 rt, Vector3 rb)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(lb);
        vertices.Add(lt);
        vertices.Add(rt);
        vertices.Add(rb);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    public void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);
        vertices.Add(e);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 3);
        triangles.Add(vertexIndex + 4);
    }

    public void AddTriangle(Vector3 point1, Vector3 point2, Vector3 point3, float height)
    {
        Vector3 h = new Vector3(0, height, 0);
        Vector3 point1h = point1 + h;
        Vector3 point2h = point2 + h;
        Vector3 point3h = point3 + h;
        AddTriangle(point1, point3, point2);
        AddTriangle(point1h, point2h, point3h);
        AddQuad(point1, point2, point2h, point1h);
        AddQuad(point2, point3, point3h, point2h);
        AddQuad(point3, point1, point1h, point3h);
    }

    public void AddQuad(Vector3 lb, Vector3 lt, Vector3 rt, Vector3 rb, float height)
    {
        Vector3 h = new Vector3(0, height, 0);
        Vector3 lbh = lb + h;
        Vector3 lth = lt + h;
        Vector3 rth = rt + h;
        Vector3 rbh = rb + h;
        AddQuad(lb, rb, rt, lt);
        AddQuad(lbh, lth, rth, rbh);
        AddQuad(lb, lbh, rbh, rb);
        AddQuad(rb, rbh, rth, rt);
        AddQuad(rt, rth, lth, lt);
        AddQuad(lt, lth, lbh, lb);
    }

    public void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e, float height)
    {
        Vector3 h = new Vector3(0, height, 0);
        Vector3 ah = a + h;
        Vector3 bh = b + h;
        Vector3 ch = c + h;
        Vector3 dh = d + h;
        Vector3 eh = e + h;
        AddPentagon(a, e, d, c, b);
        AddPentagon(ah, bh, ch, dh, eh);
        AddQuad(a, ah, eh, e);
        AddQuad(e, eh, dh, d);
        AddQuad(d, dh, ch, c);
        AddQuad(c, ch, bh, b);
        AddQuad(b, bh, ah, a);
    }


}

[Serializable]
public class MeshInfo 
{
    public int verticeCount;
    public int triangleCount;

    public MeshInfo(Mesh mesh) 
    {
        verticeCount = mesh.vertexCount;
        triangleCount = mesh.triangles.Length / 3;
    }
}