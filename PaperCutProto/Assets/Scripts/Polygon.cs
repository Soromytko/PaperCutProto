using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonRasterizer))]
public class Polygon : MonoBehaviour
{
    public List<Vector2> Points {
        get => _points;
        set
        {
            _points = value;
            GetComponent<PolygonRasterizer>().Points = _points;
        }
    }

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

}
