using System.Collections.Generic;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    public List<Vector2> Points {
        get => _points;
        set
        {
            _points = value;
            TriangulatePolygon();
        }
    }

    [SerializeField] private Transform _point1;
    [SerializeField] private Transform _point2;
    [SerializeField] private List<Vector2> _points = new List<Vector2>();
    private bool _hasIntersection = false;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }

    private void Start()
    {
        _points.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            _points.Add(transform.GetChild(i).position);
        }

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

    public bool IsIntersection(Vector2 point)
    {
        _hasIntersection = true;

        if (_points.Count < 3)
        {
            return false;
        }

        List<Vector2> edges = GetEdges();

        int intersectCount = 0;
        for (int i = 0; i < edges.Count - 1; i++)
        {
            if (point.y < Mathf.Min(edges[i].y, edges[i + 1].y) || point.y > Mathf.Max(edges[i].y, edges[i + 1].y))
            {
                continue;
            }
            if (point.x > Mathf.Max(edges[i].x, edges[i + 1].x))
            {
                continue;
            }

            float t = (point.y - edges[i].y) / (edges[i + 1].y - edges[i].y);
            if (t >= 0 && t <= 1)
            {
                float x = edges[i].x + t * (edges[i + 1].x - edges[i].x);
                if (x > point.x)
                {
                    intersectCount++;
                }
            }
        }
        _hasIntersection = intersectCount % 2 != 0;

        return intersectCount % 2 != 0;
    }

    private void Update()
    {
        
    }

    public List<Vector4> GetIntersections(Vector2 point1, Vector2 point2)
    {
        List<Vector4> result = new List<Vector4>();

        var edges = GetEdges();
        for (int i = 0; i < edges.Count - 1; i++)
        {
            var maybeIntersection = IsLineIntersection(edges[i], edges[i + 1], point1, point2);
            if (maybeIntersection != null)
            {
                Vector4 item = (Vector2)maybeIntersection;
                item.z = i;
                item.w = i + 1;
                if (i + 1 == Points.Count) {
                    item.w = 0;
                }
                result.Add(item);
            }
        }

        return result;
    }

    private Vector2? IsLineIntersection(Vector2 pointA, Vector2 pointB, Vector2 pointC, Vector2 pointD)
    {
        Vector2 b = pointB - pointA; // Вектор AB
        Vector2 d = pointD - pointC; // Вектор CD
        
        // Вычисляем знаменатель (определитель матрицы)
        float det = b.x * d.y - b.y * d.x;
        
        // Если определитель близок к нулю, прямые параллельны или совпадают
        if (Mathf.Abs(det) < float.Epsilon)
        {
            return null; // Пересечения нет
        }
        
        // Вектор от точки A до точки C
        Vector2 c = pointC - pointA;
        
        // Находим параметры t и u
        float t = (c.x * d.y - c.y * d.x) / det;
        float u = (c.x * b.y - c.y * b.x) / det;
        
        // Проверяем, лежат ли параметры в пределах отрезков
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            // Находим точку пересечения
            return pointA + t * b;
        }
        
        return null; // Отрезки не пересекаются
    
    }

    void OnDrawGizmos()
    {
        // if (_point1 != null && _point2 != null)
        // {
        //     var intersections = GetIntersections((Vector2)_point1.position, (Vector2)_point2.position);
        //     foreach (var intersectionPoint in intersections)
        //     {
        //         Gizmos.color = Color.red;
        //         Gizmos.DrawSphere(intersectionPoint, 0.05f);
        //     }
        // }

        if (_points?.Count < 2)
        {
            return;
        }

        Gizmos.color = _hasIntersection ? Color.red : Color.yellow;
        for (int i = 0; i < _points.Count - 1; i++)
        {
            Gizmos.DrawLine(_points[i], _points[i + 1]);
        }
        Gizmos.DrawLine(_points[0], _points[_points.Count - 1]);

       
    }






   public void TriangulatePolygon()
    {
        var points = _points; 

        if (points.Count < 3)
        {
            Debug.LogWarning("Need at least 3 points to triangulate a polygon");
            return;
        }
        
        // Создаем новый меш
        var mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        // Конвертируем Vector3 в Vector2 (игнорируем Z координату)
        List<Vector2> polygonPoints = new List<Vector2>();
        foreach (Vector3 point in points)
        {
            polygonPoints.Add(new Vector2(point.x, point.y));
        }
        
        // Триангулируем полигон
        int[] triangles = Triangulate(polygonPoints);
        
        // Устанавливаем данные меша
        var res = new List<Vector3>();
        foreach (var item in polygonPoints) res.Add(item);
        mesh.vertices = res.ToArray();
        mesh.triangles = triangles;
        
        // Пересчитываем нормали и границы
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
