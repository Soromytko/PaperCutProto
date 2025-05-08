using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshSubdivider : MonoBehaviour
{
    private MeshFilter _meshFilter;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    public bool Sub = false;
    private void Update()
    {
        if (Sub)
        {
            Sub = false;
            Subdivide(0.01f);
        }
    }

    public bool Subdivide(float areaLimit = 1.0f)
    {
        var mesh = _meshFilter.mesh;
        var vertices = mesh.vertices.ToList();
        var triangles = mesh.triangles.ToList();

        for (int i = 0; i <= triangles.Count - 3; i += 3)
        {
            var triangle0 = triangles[i];
            var triangle1 = triangles[i + 1];
            var triangle2 = triangles[i + 2];

            var point1 = vertices[triangle0];
            var point2 = vertices[triangle1];
            var point3 = vertices[triangle2];

            var area = CalculateTriangleArea(point1, point2, point3);
            if (area > areaLimit)
            {
                var center = CalculateTriangleCenter(point1, point2, point3);

                vertices.Add(center);
                triangles.RemoveRange(i, 3);

                var centerTriangle = vertices.Count - 1;

                triangles.AddRange(new int[3]{triangle0, triangle1, centerTriangle});
                triangles.AddRange(new int[3]{triangle1, triangle2, centerTriangle});
                triangles.AddRange(new int[3]{triangle2, triangle0, centerTriangle});
                
                // Start over
                i = 0;
            }
        }
        
        if (mesh.vertices.Length != vertices.Count)
        {
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            return true;
        }

        return false;
    }

    private float CalculateTriangleArea(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        Vector3 ab = point2 - point1;
        Vector3 ac = point3 - point1;
        
        Vector3 crossProduct = Vector3.Cross(ab, ac);
        
        float area = 0.5f * crossProduct.magnitude;
        
        return area;
    }

    private Vector3 CalculateTriangleCenter(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        return (point1 + point2 + point3) / 3.0f;
    }
}
