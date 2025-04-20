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
        _polygonTriangulator.Points = _shape.Points;    
    }

    void OnDrawGizmos()
    {
        if (_shape?.Points.Count < 2)
        {
            return;
        }

        // Gizmos.color = Color.red;
        // foreach (Vector2 point in _shape.Points)
        // {
        //     Gizmos.DrawSphere(point, 0.03f);
        // }

        Gizmos.color = Color.white;
        List<Vector2> points = _shape.Points;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
        Gizmos.DrawLine(points[0], points[points.Count - 1]);
    }

}
