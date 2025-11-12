using System;
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

    public IReadOnlyList<Vector2> Points => _points.AsReadOnly();

    [Tooltip("Min distance between vertices for simplification")]
    [SerializeField] private float _simplifyTolerance = 0.03f;
    [SerializeField] private List<Vector2> _points = new List<Vector2>();

    public PolygonShape()
    {
        _points = new List<Vector2>();
    }

    public PolygonShape(IEnumerable<Vector2> points)
    {
        SetPoints(points);
    }

    public void SetPoints(IEnumerable<Vector2> newPoints, bool shouldSimplifyOutline = true)
    {
        if (newPoints == null)
        {
            throw new ArgumentNullException(nameof(newPoints));
        }

        var validatedPoints = new List<Vector2>(newPoints);

        if (shouldSimplifyOutline)
        {
            validatedPoints = SimplifyOutline(validatedPoints);
        }

        validatedPoints = ValidatePointOrder(validatedPoints);

        _points = validatedPoints;
        PointsChanged?.Invoke();
    }

    private List<Vector2> ValidatePointOrder(List<Vector2> points)
    {
        Geometry2DUtils.WindingOrder order = Geometry2DUtils.GetPointWindingOrder(points.AsReadOnly());
        if (order == Geometry2DUtils.WindingOrder.Degenerate)
        {
            Debug.LogError("This is wrong order of polygon points");
            return null;
        }
        if (order == Geometry2DUtils.WindingOrder.Clockwise)
        {
            points.Reverse();
        }

        return points;
    }

    private List<Vector2> SimplifyOutline(List<Vector2> outline)
    {
        Debug.Assert(outline != null && _simplifyTolerance > 0);
        if (outline.Count <= 3)
        {
            return outline;
        }

        for (int i = 0; i < outline.Count; i++)
        {
            var firstPoint = outline[(i - 1 + outline.Count) % outline.Count];
            var middlePoint = outline[i];
            var firstDistance = Vector2.Distance(firstPoint, middlePoint);
            if (firstDistance < _simplifyTolerance)
            {
                var secondPoint = outline[(i + 1) % outline.Count];
                var secondDistance = Vector2.Distance(middlePoint, secondPoint);
                if (secondDistance > _simplifyTolerance * 2)
                {
                    middlePoint += (secondPoint - middlePoint).normalized * _simplifyTolerance;
                    outline[i] = middlePoint;
                    continue;
                }

                outline.RemoveAt(i);
                if (outline.Count == 3)
                {
                    break;
                }
                i--;
            }
        }

        return outline;
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

    public bool IsPointInside(Vector2 localPoint)
    {
        Vector2 point = localPoint;

        if (_points.Count < 3)
        {
            return false;
        }

        int intersectCount = 0;
        for (int i = 0; i < _points.Count; i++)
        {
            var edgeStart = _points[i];
            var edgeEnd = _points[(i + 1) % _points.Count];

            if (point.y < Mathf.Min(edgeStart.y, edgeEnd.y) || point.y > Mathf.Max(edgeStart.y, edgeEnd.y))
            {
                continue;
            }
            if (point.x > Mathf.Max(edgeStart.x, edgeEnd.x))
            {
                continue;
            }

            float t = (point.y - edgeStart.y) / (edgeEnd.y - edgeStart.y);
            if (t >= 0 && t <= 1)
            {
                float x = edgeStart.x + t * (edgeEnd.x - edgeStart.x);
                if (x > point.x)
                {
                    intersectCount++;
                }
            }
        }
        return intersectCount % 2 != 0;
    }

    public List<Intersection> GetIntersectionsByLine(Vector2 firstLocalPoint, Vector2 secondLocalPoint)
    {
        Vector2 point1 = firstLocalPoint;
        Vector2 point2 = secondLocalPoint;

        List<Intersection> result = new List<Intersection>();

        for (int i = 0; i < _points.Count; i++)
        {
            var edgeStart = _points[i];
            var edgeEnd = _points[(i + 1) % _points.Count];
            var maybeIntersection = Geometry2DUtils.GetLineIntersection(edgeStart, edgeEnd, point1, point2);
            if (maybeIntersection != null)
            {
                Intersection intersection = new Intersection(
                    (Vector2)maybeIntersection,
                    i,
                    (i + 1) % Points.Count);
                result.Add(intersection);
            }
        }

        return result;
    }
}
