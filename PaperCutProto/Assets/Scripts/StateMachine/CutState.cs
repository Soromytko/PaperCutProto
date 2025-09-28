using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ScissorsSoundPlayer))]
public class CutState : State
{
    public UnityEvent<List<Vector2>> CutPointsChanged;

    [SerializeField] private float _pointDistance = 0.1f;
    [SerializeField] private Cut _cut;

    private Vector2 _lastCursorPosition;

    private PolygonManager _polygonManager;
    private Polygon _currentPolygon = null;
    private PolygonShape _draftPolygonShape = new PolygonShape();
    private List<int> _intersectionPointIndeces = new List<int>();
    private List<int> _holeIntersectionPointIndeces = new List<int>();
    private List<Vector2> _cutPoints = new List<Vector2>();

    private bool _HasPolygonIntersections => _intersectionPointIndeces.Count > 0;

    private ScissorsSoundPlayer _scissorsSoundPlayer;

    private Stack<List<Vector2>> _polygonPointsStack = new Stack<List<Vector2>>();

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

    public void Undo()
    {
        if (_polygonPointsStack.Count > 0)
        {
            List<Vector2> points = _polygonPointsStack.Pop();
            Polygon polygon = GetCurrentPolygon();
            if (polygon != null)
            {
                polygon.Shape.SetPoints(points);
            }
        }
    }

    private void Reset()
    {
        ClearCutPoints();
        _cut.Reset();
        _intersectionPointIndeces.Clear();
        _holeIntersectionPointIndeces.Clear();
        _currentPolygon = null;
    }

    private Polygon FindPolygon()
    {
        return _polygonManager.MainPolygon;
    }

    private void AddCutPoint(Vector2 point)
    {
        _cutPoints.Add(point);
        CutPointsChanged?.Invoke(_cutPoints);
    }

    private void ClearCutPoints()
    {
        if (_cutPoints.Count > 0)
        {
            _cutPoints.Clear();
            CutPointsChanged?.Invoke(_cutPoints);
        }
    }

    private void removePoints(int count)
    {
        _cut.RemovePoints(count);
        if (_cutPoints.Count > 0)
        {
            int cutPointCount = _cutPoints.Count;
            int cutCount = Mathf.Clamp(count, 0, _cutPoints.Count);
            _cutPoints.RemoveRange(_cutPoints.Count - cutCount, cutCount);
            if (_cutPoints.Count == 0)
            {
                _intersectionPointIndeces.Clear();
            }
            if (cutPointCount != _cutPoints.Count)
            {
                CutPointsChanged?.Invoke(_cutPoints);
            }
        }
    }

    private Polygon GetCurrentPolygon()
    {
        if (_currentPolygon == null)
        {
            Polygon newPolygon = FindPolygon();
            if (newPolygon != _currentPolygon && newPolygon != null)
            {
                _draftPolygonShape.SetPoints(newPolygon.Shape.Points);
            }
            _currentPolygon = newPolygon;
        }
        return _currentPolygon;
    }

    private int getStartPointIndexOfLoop(Vector2 newPoint)
    {
        var points = _cut.Points;
        if (points.Count < 3)
        {
            return -1;
        }

        Vector2 lastPoint = points[points.Count - 1];
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 point1 = points[i - 1];
            Vector2 point2 = points[i];
            if (Geometry2DUtils.GetLineIntersection(point1, point2, lastPoint, newPoint) != null)
            {
                return i - 1;
            }
        }

        return -1;
    }

    private bool IsLoop(Vector2 newPoint)
    {
        return getStartPointIndexOfLoop(newPoint) >= 0;
    }

    private bool processLoop(Vector2 newPoint)
    {
        int index = getStartPointIndexOfLoop(newPoint);
        if (index < 0)
        {
            return false;
        }

        int countToDelete = _cut.Points.Count - index;
        removePoints(countToDelete);

        return true;
    }

    private void AddPoint(Vector2 point)
    {
        Polygon polygon = GetCurrentPolygon();

        if (_cut.PointCount > 2 && IsLoop(point))
        {
            if (!_HasPolygonIntersections && polygon && polygon.ContainsPoint(point))
            {
                ProcessHole(_cut.Points, point);
                Reset();
                return;
            }
            else
            {
                processLoop(point);
                return;
            }
        }

        _cut.AddPoint(point);

        if (polygon == null || _cut.Points.Count < 2)
        {
            return;
        }

        PolygonShape draftPolygonShape = _draftPolygonShape;

        Vector2 point1 = polygon.GetLocalPoint(_cut.Points[_cut.Points.Count - 2]);
        Vector2 point2 = polygon.GetLocalPoint(_cut.Points[_cut.Points.Count - 1]);

        var intersections = draftPolygonShape.GetIntersectionsByLine(point1, point2);
        if (intersections != null && intersections.Count == 1)
        {
            var intersection = intersections[0];
            draftPolygonShape.InsertIntersectionPoint(ref intersection);
            AddCutPoint(intersection.Point);
            _intersectionPointIndeces.Add(intersection.StartEdgeIndex + 1);

            if (_intersectionPointIndeces.Count == 2)
            {
                if (_intersectionPointIndeces[0] >= intersection.StartEdgeIndex + 1)
                {
                    _intersectionPointIndeces[0]++;
                }

                List<Vector2> newVertices = new List<Vector2>();
                newVertices.AddRange(_cutPoints);

                List<Vector2> firstPolygonPoints = Slice(draftPolygonShape, _cutPoints, false);
                List<Vector2> secondPolygonPoints = Slice(draftPolygonShape, _cutPoints, true);

                var firstShape = new PolygonShape(firstPolygonPoints);
                var secondShape = new PolygonShape(secondPolygonPoints);

                var points = ProcessSlicedShapes(firstShape, secondShape);
                _polygonPointsStack.Push(new List<Vector2>(polygon.Shape.Points));
                polygon.Shape.SetPoints(points);

                Reset();
                SwitchState("CompareState");
                return;
            }
        }
        else if (_intersectionPointIndeces.Count == 1)
        {
            AddCutPoint(polygon.GetLocalPoint(point));
            _scissorsSoundPlayer.Play();
        }
    }

    private List<Vector2> Slice(PolygonShape polygonShape, List<Vector2> vertices, bool reverse = false)
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
            i = (i + 1) % polygonShape.Points.Count;
            if (i == targetIndex)
            {
                break;
            }
            result.Add(polygonShape.Points[i]);
        }

        return result;
    }

    private IReadOnlyList<Vector2> ProcessSlicedShapes(PolygonShape firstShape, PolygonShape secondShape)
    {
        float firstArea = firstShape.CalculateArea();
        float secondArea = secondShape.CalculateArea();

        PolygonShape shape = firstArea > secondArea ? firstShape : secondShape;

        _currentPolygon.Shape.SetPoints(shape.Points);
        return shape.Points;
    }

    private void ProcessHole(IReadOnlyList<Vector2> points, Vector2 closingPoint)
    {
        int startIndex = getStartPointIndexOfLoop(closingPoint);
        if (startIndex < 0)
        {
            return;
        }

        Vector2[] holePoints = new Vector2[points.Count - startIndex];
        for (int i = 0; i < holePoints.Length; i++)
        {
            holePoints[i] = points[i + startIndex];
        }

        _polygonManager.CreateHolePolygon(holePoints);
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
