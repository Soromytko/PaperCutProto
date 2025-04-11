using System;
using System.Collections.Generic;
using UnityEngine;

public class Scissors : MonoBehaviour
{
    [SerializeField] private float _pointDistance = 0.1f;
    [SerializeField] private Transform _cursor;
    [SerializeField] private Cut _cut;
    [SerializeField] private Polygon _polygon;
    [SerializeField] private bool _isIntersection;
    [SerializeField] private Polygon _polygonPrefab;

    private Vector3 _lastCursorPosition;


    private Vector3 getCursorPosition()
    {
        Vector3 screenPoint = Input.mousePosition;
        Vector3 result = Camera.main.ScreenToWorldPoint(screenPoint);
        result.z = transform.position.z;
        return result;
    }

    private void Update()
    {
        _cursor.position = getCursorPosition();

        _isIntersection = _polygon.IsIntersection(_cursor.position);

        if (Input.GetMouseButtonDown(0))
        {
            AddPoint(_cursor.position);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Reset();
        }

        if (!Input.GetMouseButton(0))
        {
            return;
        }

        float distance = Vector3.Distance(_cursor.position, _lastCursorPosition);
        if (distance >= _pointDistance)
        {
            _lastCursorPosition = _cursor.position;
            AddPoint(_lastCursorPosition);
        }
        else
        {
            //_cut.SetPoint(_cut.PointCount - 1, _cursor.position);
        }
    }

    private List<Vector2> _cutPoints = new List<Vector2>();
    private List<Vector2> _inter = new List<Vector2>();

    private void Reset()
    {
        _cutPoints.Clear();
        _cut.Points.Clear();
        _inter.Clear();
    }

    private bool IsRight(Vector2 pointA, Vector2 pointB, Vector2 pointC)
    {
        Vector3 v1 = pointC - pointA;
        Vector3 v2 = pointC - pointB;
        return Vector3.Cross(v1, v2).z < 0;
    }

    private void AddPoint(Vector2 point)
    {
        _cut.AddPoint(point);
        if (_cut.Points.Count < 2)
        {
            return;
        }

        var inter = _polygon.GetIntersections(_cut.Points[_cut.Points.Count - 2], _cut.Points[_cut.Points.Count - 1]);
        if (inter.Count == 1)
        {
            _inter.Add(new Vector2(inter[0].z, inter[0].w));
            _cutPoints.Add((Vector2)inter[0]);

            if (_inter.Count == 2)
            {
                List<Vector2> newPoints = new List<Vector2>();
                newPoints.AddRange(_cutPoints);

                List<Vector2> firstPolygonPoints = Slice(_cutPoints);
                List<Vector2> secondPolygonPoints = Slice(_cutPoints, true);

                _polygon.Points = firstPolygonPoints;
                Polygon secondPolygon = Instantiate(_polygonPrefab);
                secondPolygon.Points = secondPolygonPoints;

                Reset();
                return;
            }
        }

        if (_inter.Count == 1)
        {
            _cutPoints.Add(point);
        }
    }

    private List<Vector2> Slice(List<Vector2> points, bool reverse = false)
    {
        List<Vector2> result = new List<Vector2>();
        result.AddRange(points);

        int i = (int)(reverse ? _inter[1].x : _inter[1].y);
        int targetIndex = (int)(reverse ? _inter[0].x : _inter[0].y);
        int step = reverse ? -1 : 1;

        while (i != targetIndex)
        {
            result.Add(_polygon.Points[i]);
            i += step;
            i = (i == _polygon.Points.Count ? 0 : (i < 0 ? _polygon.Points.Count - 1 : i));
        }

        return result;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var point in _cutPoints)
        {
            //Gizmos.DrawSphere(point, 0.01f);
        }

    }
  
}
