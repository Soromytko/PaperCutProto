using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonTriangulator))]
public class Polygon : MonoBehaviour
{
    public PolygonShape Shape => _shape;

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

    public Vector2 GetLocalPoint(Vector2 globalPoint)
    {
        return globalPoint - (Vector2)transform.position;
    }

    public bool IsPointInside(Vector2 globalPoint)
    {
        return _shape.IsPointInside(GetLocalPoint(globalPoint));
    }

    public List<Intersection> GetIntersectionsByLine(Vector2 lineStartGlobalPoint, Vector2 lineEndGlobalPoint)
    {
        var firstLocalPoint = GetLocalPoint(lineStartGlobalPoint);
        var secondLocalPoint = GetLocalPoint(lineEndGlobalPoint);

        return _shape.GetIntersectionsByLine(firstLocalPoint, secondLocalPoint);
    }

    void OnDrawGizmos()
    {
        if (_shape?.Points.Count < 2)
        {
            return;
        }

        Vector2 pos = transform.position;
        Vector2 scale = transform.localScale;

        // Gizmos.color = Color.red;
        // foreach (Vector2 point in _shape.Points)
        // {
        //     Gizmos.DrawSphere(point * scale + pos, 0.03f * Mathf.Min(scale.x, scale.y));
        // }

        // Gizmos.color = Color.white;
        // List<Vector2> points = _shape.Points;
        // for (int i = 0; i < points.Count - 1; i++)
        // {
        //     Gizmos.DrawLine(points[i] * scale + pos, points[i + 1] * scale + pos);
        // }
        // Gizmos.DrawLine(points[0] * scale + pos, points[points.Count - 1] * scale + pos);
    }

}
