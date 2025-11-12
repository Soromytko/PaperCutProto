using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonTriangulator))]
public class Polygon : MonoBehaviour
{
    public PolygonShape Shape => _shape;

#if UNITY_EDITOR
    [SerializeField] private bool _drawPoints = false;
#endif
    [SerializeField] private PolygonShape _shape = new PolygonShape();

    PolygonTriangulator _polygonTriangulator;

    private void Awake()
    {
        _shape.PointsChanged.AddListener(OnPointsChanged);
        _polygonTriangulator = GetComponent<PolygonTriangulator>();
        OnPointsChanged();
    }

    private void OnPointsChanged()
    {
        _polygonTriangulator.Points = new List<Vector2>(_shape.Points);
    }

    public bool ContainsPoint(Vector2 globalPoint)
    {
        Vector2 localPoint = ConvertGlobalToLocalPoint(globalPoint);
        return _shape.IsPointInside(localPoint);
    }

    public Vector2 ConvertGlobalToLocalPoint(Vector2 globalPoint)
    {
        return globalPoint - (Vector2)transform.position;
    }

    public Vector2 ConvertLocalToGlobalPoint(Vector2 localPoint)
    {
        return (Vector2)transform.position + localPoint;
    }

    public Vector2 GetGlobalShapePoint(int index)
    {
        Debug.Assert(index >= 0 && index < _shape.Points.Count);
        return _shape.Points[index];
    }

    public bool IsPointInside(Vector2 globalPoint)
    {
        return _shape.IsPointInside(ConvertGlobalToLocalPoint(globalPoint));
    }

    public List<Intersection> GetIntersectionsByLine(Vector2 lineStartGlobalPoint, Vector2 lineEndGlobalPoint)
    {
        var firstLocalPoint = ConvertGlobalToLocalPoint(lineStartGlobalPoint);
        var secondLocalPoint = ConvertGlobalToLocalPoint(lineEndGlobalPoint);

        return _shape.GetIntersectionsByLine(firstLocalPoint, secondLocalPoint);
    }

    public bool ContainsPolygon(Polygon other)
    {
        Debug.Assert(other != null);

        if (other == this)
        {
            return false;
        }

        for (int i = 0; i < other._shape.Points.Count; i++)
        {
            if (!ContainsPoint(other.GetGlobalShapePoint(i)))
            {
                return false;
            }
        }

        return true;
    }

    public bool IntersectsPolygon(Polygon other)
    {
        Debug.Assert(other != null);

        if (other == this)
        {
            return false;
        }

        var otherPoints = other.Shape.Points;
        var thisPoints = _shape.Points;
        if (otherPoints.Count < 3 || thisPoints.Count < 3)
        {
            return false;
        }

        for (int i = 1; i < otherPoints.Count; i++)
        {
            var point1 = other.GetGlobalShapePoint(i - 1);
            var point2 = other.GetGlobalShapePoint(i);
            var intersections = GetIntersectionsByLine(point1, point2);
            if (intersections != null && intersections.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public enum Relation
    {
        Intersection,
        Within,
        Outside,
        None,
    }

    public Relation DeterminePolygonRelation(Polygon other)
    {
        Debug.Assert(other != null);

        if (other == this)
        {
            return Relation.None;
        }
        if (IntersectsPolygon(other))
        {
            return Relation.Intersection;
        }
        if (ContainsPolygon(other))
        {
            return Relation.Outside;
        }
        if (other.ContainsPolygon(this))
        {
            return Relation.Within;
        }

        return Relation.None;
    }

    public bool Merge(Polygon other)
    {
        if (other == null || other == this)
        {
            return false;
        }

        if (other.Shape.Points.Count < 3 || _shape.Points.Count < 3)
        {
            return false;
        }

        var thisPoints = _shape.Points;
        var otherPoints = other.Shape.Points;

        int startIndex = -1;
        for (int i = 0; i < thisPoints.Count; i++)
        {
            if (!other.ContainsPoint(GetGlobalShapePoint(i)))
            {
                startIndex = i;
                break;
            }
        }
        Debug.Assert(startIndex > -1);

        List<Vector2> mergedPoints = new List<Vector2>();

        // Collect some of the points in the first shape.
        int iterator = startIndex;
        while (true)
        {
            int nextIterator = (iterator + 1) % thisPoints.Count;
            var point1 = GetGlobalShapePoint(iterator);
            var point2 = GetGlobalShapePoint(nextIterator);
            var intersections = other.GetIntersectionsByLine(point1, point2);
            if (intersections != null && intersections.Count > 0)
            {
                var intersectionPoint = other.ConvertLocalToGlobalPoint(intersections[0].Point);
                mergedPoints.Add(ConvertGlobalToLocalPoint(intersectionPoint));
                iterator = intersections[0].EndEdgeIndex;
                break;
            }
            else
            {
                mergedPoints.Add(point1);
            }
            iterator = nextIterator;
        }

        // Collect all the points in the second shape.
        while (true)
        {
            int nextIterator = (iterator + 1) % otherPoints.Count;
            var point1 = other.GetGlobalShapePoint(iterator);
            var point2 = other.GetGlobalShapePoint(nextIterator);
            var intersections = GetIntersectionsByLine(point1, point2);
            if (intersections != null && intersections.Count > 0)
            {
                mergedPoints.Add(intersections[0].Point);
                iterator = intersections[0].EndEdgeIndex;
                break;
            }
            else
            {
                mergedPoints.Add(ConvertGlobalToLocalPoint(point1));
            }
            iterator = nextIterator;
        }

        // Collect the remaining points in the first shape.
        while (iterator != startIndex)
        {
            mergedPoints.Add(thisPoints[iterator]);
            iterator = (iterator + 1) % thisPoints.Count;
        }

        _shape.SetPoints(mergedPoints);
        return true;
    }

    void OnDrawGizmos()
    {
        if (_shape == null || _shape.Points == null || _shape?.Points.Count < 2)
        {
            return;
        }

        Vector2 pos = transform.position;
        Vector2 scale = transform.localScale;

        if (_drawPoints)
        {
            Gizmos.color = Color.red;
            foreach (var point in _shape.Points)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)point, 0.01f);
            }
        }
    }

}
