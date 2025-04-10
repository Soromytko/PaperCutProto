using System.Collections.Generic;
using UnityEngine;

public class Cut : MonoBehaviour
{
    public List<Vector2> Points => _points;
    public int PointCount => _points.Count;

    private List<Vector2> _points = new List<Vector2>();
    private bool _isCorrect = false;

    // public void EndCutting()
    // {
    //     int intersectionCount = 0;

    //     for (int i = 0; i < _points.Count - 1; i ++)
    //     {
    //         Vector2 point1 = (Vector2)_points[i];
    //         Vector2 point2 = (Vector2)_points[i + 1];

    //         var currentIntersections = _polygon.GetIntersections(point1, point2);
    //         intersectionCount += currentIntersections.Count;
    //         _intersectionPoints.AddRange(currentIntersections);

    //         _intersectionEdges.AddRange(currentIntersections);

    //         if (_intersectionPoints.Count > 0 && intersectionCount % 2 != 0) {
    //             _intersectionPoints.Add(point2);
    //         }
    //     }

    //     if (_intersectionEdges.Count == 0 || _intersectionEdges.Count % 2 != 0)
    //     {
    //         return;
    //     }

    //     List<Vector3> newPoints = new List<Vector3>();
    //     foreach (var intersection in _intersectionPoints)
    //     {
    //         newPoints.Add((Vector3)intersection);
    //     }

    //     int j = (int)_intersectionEdges[0].z;
    //     do {
    //         newPoints.Add(_polygon.Points[j]);
    //         j++;

    //     } while(j <= _intersectionEdges[_intersectionEdges.Count - 1].w);

    //     _polygon.Points = newPoints;

    // }

    public void AddPoint(Vector2 point)
    {
        _points.Add(point);
    }

    public void Reset()
    {
        _points.Clear();
    }

    private void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // foreach (var point in _intersectionPoints)
        // {
        //     Gizmos.DrawSphere(point, 0.05f);
        // }


        Gizmos.color = Color.white;
        for (int i = 0; i < _points.Count - 1; i++)
        {
            Gizmos.DrawLine(_points[i], _points[i + 1]);
        }

        Gizmos.color = !_isCorrect ? Color.green : Color.red;
        foreach (var point in _points)
        {
            //Gizmos.DrawSphere(point, 0.02f);
        }
    }
}
