using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct Intersection
{
    public Intersection(Vector2 point, int startEdgeIndex, int endEdgeIndex)
    {
        Point = point;
        StartEdgeIndex = startEdgeIndex;
        EndEdgeIndex = endEdgeIndex;
    }

    public Vector2 Point { get; }
    public int StartEdgeIndex { get; }
    public int EndEdgeIndex { get; }
}

[System.Serializable]
public class PolygonShape
{
    public UnityEvent PointsChanged;

    public List<Vector2> Points
    {
        get => _points;
        set
        {
            if (_points != value)
            {
                _points = value;
                PointsChanged?.Invoke();
            }
        }
    }

    [SerializeField] private List<Vector2> _points = new List<Vector2>();

    public PolygonShape()
    {
        _points = new List<Vector2>();
    }

    public PolygonShape(List<Vector2> points)
    {
        _points = points;
    }

    // Shoelace formula (Gauss's area formula)
    public float CalculateArea()
    {
        var vertices = _points;

        if (vertices == null || vertices.Count < 3)
            return 0;

        float area = 0;
        int n = vertices.Count;

        for (int i = 0; i < n; i++)
        {
            Vector2 current = vertices[i];
            Vector2 next = vertices[(i + 1) % n];

            area += (current.x * next.y) - (next.x * current.y);
        }

        return Mathf.Abs(area / 2);
    }

    public void InsertIntersectionPoint(ref Intersection intersection)
    {
        _points.Insert(intersection.StartEdgeIndex + 1, intersection.Point);
        PointsChanged?.Invoke();
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

    public bool IsIntersection(Vector2 localPoint)
    {
        Vector2 point = localPoint;
        
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
        return intersectCount % 2 != 0;
    }

    public List<Intersection> GetIntersections(Vector2 firstLocalPoint, Vector2 secondLocalPoint)
    {
        Vector2 point1 = firstLocalPoint;
        Vector2 point2 = secondLocalPoint;

        List<Intersection> result = new List<Intersection>();

        var edges = GetEdges();
        for (int i = 0; i < edges.Count - 1; i++)
        {
            var maybeIntersection = IsLineIntersection(edges[i], edges[i + 1], point1, point2);
            if (maybeIntersection != null)
            {
                Intersection intersection = new Intersection(
                    (Vector2)maybeIntersection,
                    i,
                    ++i % Points.Count);
                result.Add(intersection);
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

}
