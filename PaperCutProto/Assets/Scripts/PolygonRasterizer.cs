using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PolygonRasterizer : MonoBehaviour
{
    public List<Vector2> Points
    {
        get => _points;
        set
        {
            if (_points != value)
            {
                _points = value;
                if (_isReady)
                {
                    TriangulatePolygon();
                }
            }
        }
    }

    private List<Vector2> _points = new List<Vector2>();
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private bool _isReady = false;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.materials[0].doubleSidedGI = true; // Для глобального освещения
        _isReady = true;
        TriangulatePolygon();
    }

    private List<Vector2> GetEdges()
    {
        List<Vector2> result = new List<Vector2>();
        for (int i = 0; i < _points.Count; i++)
        {
            result.Add(_points[i]);
        }
        result.Add(_points[0]);
        return result;
    }

    public void TriangulatePolygon()
    {
        var points = _points; 

        if (points.Count < 3)
        {
            Debug.LogWarning("Need at least 3 points to triangulate a polygon");
            return;
        }
        
        var mesh = new Mesh();
        _meshFilter.mesh = mesh;
        
        List<Vector2> polygonPoints = new List<Vector2>();
        foreach (Vector3 point in points)
        {
            polygonPoints.Add(new Vector2(point.x, point.y));
        }
        
        int[] triangles = Triangulate(polygonPoints);
        
        var res = new List<Vector3>();
        foreach (var item in polygonPoints) res.Add(item);
        mesh.vertices = res.ToArray();
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
    
    private int[] Triangulate(List<Vector2> polygon)
    {
        List<int> indices = new List<int>();
        
        int n = polygon.Count;
        if (n < 3) return indices.ToArray();
        
        int[] V = new int[n];
        if (Area(polygon) > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }
        
        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2; )
        {
            if ((count--) <= 0)
                return indices.ToArray();
            
            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;
            
            if (Snip(polygon, u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }
        
        return indices.ToArray();
    }
    
    private float Area(List<Vector2> polygon)
    {
        int n = polygon.Count;
        float A = 0.0f;
        
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = polygon[p];
            Vector2 qval = polygon[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        
        return (A * 0.5f);
    }
    
    private bool Snip(List<Vector2> polygon, int u, int v, int w, int n, int[] V)
    {
        Vector2 A = polygon[V[u]];
        Vector2 B = polygon[V[v]];
        Vector2 C = polygon[V[w]];
        
        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        
        for (int p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            
            Vector2 P = polygon[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        
        return true;
    }
    
    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;
        
        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;
        
        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;
        
        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }

}
