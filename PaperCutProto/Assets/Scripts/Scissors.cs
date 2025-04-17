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

    private Polygon _currentPolygon = null;
    private PolygonShape _currentPolygonShape = null;
    private List<int> _intersectionPointIndeces = new List<int>();
    private List<Vector2> _cutPoints = new List<Vector2>();

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
    }

    private void Reset()
    {
        _cutPoints.Clear();
        _cut.Points.Clear();
        _intersectionPointIndeces.Clear();
        _currentPolygon = null;
    }

    private bool IsRight(Vector2 pointA, Vector2 pointB, Vector2 pointC)
    {
        Vector3 v1 = pointC - pointA;
        Vector3 v2 = pointC - pointB;
        return Vector3.Cross(v1, v2).z < 0;
    }

    private Polygon FindPolygon()
    {
        return _polygon;
    }

    private void AddPoint(Vector2 point)
    {
        _cut.AddPoint(point);

        if (_currentPolygon == null)
        {
            Polygon newPolygon = FindPolygon();
            if (newPolygon != null)
            {
                _currentPolygon = newPolygon;
                _currentPolygonShape = new PolygonShape(newPolygon.Shape.Points);
            }
        }

        // Process Cut
        if (_currentPolygon == null || _cut.Points.Count < 2)
        {
            return;
        }

        var intersections = _currentPolygonShape.GetIntersections(_cut.Points[_cut.Points.Count - 2], _cut.Points[_cut.Points.Count - 1]);
        if (intersections != null && intersections.Count == 1)
        {
            var intersection = intersections[0];
            _currentPolygonShape.InsertIntersectionPoint(ref intersection);
            _cutPoints.Add(intersection.Point);
            _intersectionPointIndeces.Add(intersection.StartEdgeIndex + 1);
            
            if (_intersectionPointIndeces.Count == 2)
            {
                if (_intersectionPointIndeces[0] >= intersection.StartEdgeIndex + 1)
                {
                    _intersectionPointIndeces[0]++;
                }

                List<Vector2> newVertices = new List<Vector2>();
                newVertices.AddRange(_cutPoints);

                List<Vector2> firstPolygonPoints = Slice(_cutPoints, false);
                List<Vector2> secondPolygonPoints = Slice(_cutPoints, true);

                _polygon.Shape.Points = firstPolygonPoints;
                Polygon secondPolygon = Instantiate(_polygonPrefab);
                secondPolygon.Shape.Points = secondPolygonPoints;

                Reset();
                return;
            }
        }
        else if (_intersectionPointIndeces.Count == 1)
        {
            _cutPoints.Add(point);
        }
    }

    private List<Vector2> Slice(List<Vector2> vertices, bool reverse = false)
    {
        List<Vector2> result = new List<Vector2>();
        result.AddRange(vertices);

        int i = (int)_intersectionPointIndeces[1];
        int targetIndex = (int)_intersectionPointIndeces[0];
        int step = reverse ? -1 : 1;

        while (i != targetIndex)
        {
            result.Add(_currentPolygonShape.Points[i]);
            i += step;
            i = i < 0 ? _currentPolygonShape.Points.Count - 1 : (i % _currentPolygonShape.Points.Count);
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
