using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ScissorsSoundPlayer))]
public class CutState : State
{
    [SerializeField] private float _pointDistance = 0.1f;
    [SerializeField] private Cut _cut;

    private Vector2 _lastCursorPosition;

    private PolygonManager _polygonManager;
    private Polygon _currentPolygon = null;
    private PolygonShape _currentPolygonShape = null;
    private List<int> _intersectionPointIndeces = new List<int>();
    private List<Vector2> _cutPoints = new List<Vector2>();

    private ScissorsSoundPlayer _scissorsSoundPlayer;

    private void Awake()
    {
        _polygonManager = FindAnyObjectByType<PolygonManager>();
        _scissorsSoundPlayer = GetComponent<ScissorsSoundPlayer>();
    }

    private Vector2 getCursorPosition()
    {
        Vector3 screenPoint = Input.mousePosition;
        Vector3 result = Camera.main.ScreenToWorldPoint(screenPoint);
        return result;
    }

    public override void OnTick()
    {
        Vector2 cursorPosition = getCursorPosition();

        if (Input.GetMouseButtonDown(0))
        {
            AddPoint(cursorPosition);
            _lastCursorPosition = cursorPosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Reset();
        }

        if (!Input.GetMouseButton(0))
        {
            return;
        }

        float distance = Vector2.Distance(cursorPosition, _lastCursorPosition);
        if (distance >= _pointDistance)
        {
            Vector2 cursorDirection = (cursorPosition - _lastCursorPosition).normalized;
            while (distance >= _pointDistance)
            {
                AddPoint(_lastCursorPosition + cursorDirection * _pointDistance);
                _lastCursorPosition += cursorDirection * _pointDistance;
                distance -= _pointDistance;
            }
        }
        
    }

    private void Reset()
    {
        _cutPoints.Clear();
        _cut.Reset();
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
        return _polygonManager.MainPolygon;
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

        Vector2 point1 = _currentPolygon.GetLocalPoint(_cut.Points[_cut.Points.Count - 2]);
        Vector2 point2 = _currentPolygon.GetLocalPoint(_cut.Points[_cut.Points.Count - 1]);

        var intersections = _currentPolygonShape.GetIntersectionsByLine(point1, point2);
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

                var firstShape = new PolygonShape(firstPolygonPoints);
                var secondShape = new PolygonShape(secondPolygonPoints);

                ProcessSlicedShapes(firstShape, secondShape);

                Reset();
                SwitchState("CompareState");
                return;
            }
        }
        else if (_intersectionPointIndeces.Count == 1)
        {
            _cutPoints.Add(_currentPolygon.GetLocalPoint(point));
            _scissorsSoundPlayer.Play();
        }
    }

    private List<Vector2> Slice(List<Vector2> vertices, bool reverse = false)
    {
        List<Vector2> result = new List<Vector2>();
        result.AddRange(vertices);

        if (reverse)
        {
            result.Reverse();
        }

        int i = reverse ? _intersectionPointIndeces[0] : _intersectionPointIndeces[1];
        int targetIndex = reverse ? _intersectionPointIndeces[1] : _intersectionPointIndeces[0];

        while (true)
        {
            i = (i + 1) % _currentPolygonShape.Points.Count;
            if (i == targetIndex)
            {
                break;
            }
            result.Add(_currentPolygonShape.Points[i]);
        }

        return result;
    }

    private void ProcessSlicedShapes(PolygonShape firstShape, PolygonShape secondShape)
    {
        float firstArea = firstShape.CalculateArea();
        float secondArea = secondShape.CalculateArea();

        PolygonShape shape = firstArea > secondArea ? firstShape : secondShape;

        _currentPolygon.Shape.Points = shape.Points;
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
