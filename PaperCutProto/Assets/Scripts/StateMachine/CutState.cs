using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.Video;
using static PolygonShape;

[RequireComponent(typeof(ScissorsSoundPlayer))]
public class CutState : State
{
    public UnityEvent<List<Vector2>> CutPointsChanged;

    [SerializeField] private float _pointDistance = 0.1f;
    [SerializeField] private float _rawHoleDistance = 0.2f;
    [SerializeField] private Cut _cut;

    private Vector2 _lastCursorPosition;

    private PolygonManager _polygonManager;
    private Polygon _currentPolygon = null;
    private PolygonShape _draftPolygonShape = new PolygonShape();
    private List<int> _intersectionPointIndeces = new List<int>();
    private List<int> _holeIntersectionPointIndeces = new List<int>();
    private List<Vector2> _cutPoints = new List<Vector2>();
    private List<Polygon> _affectedHoles = new List<Polygon>();

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
            RestCutState();
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

    private void RestCutState()
    {
        ClearCutPoints();
        _cut.Reset();
        _intersectionPointIndeces.Clear();
        _holeIntersectionPointIndeces.Clear();
        _affectedHoles.Clear();
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

    private void InsertIntersectionPoint(PolygonShape shape, ref IntersectionInfo intersection)
    {
        Debug.Assert(shape != null);
        var points = new List<Vector2>(shape.Points);
        points.Insert(intersection.StartEdgeIndex + 1, intersection.Point);
        shape.SetPoints(points, false);
    }

    private void AddPoint(Vector2 point)
    {
        Polygon polygon = GetCurrentPolygon();

        if (ProcessHole(polygon, _cut.Points, point))
        {
            RestCutState();
            SwitchState("CompareState");
            return;
        }

        if (_cut.PointCount > 2 && IsLoop(point))
        {
            processLoop(point);
            return;
        }

        _cut.AddPoint(point);

        if (polygon == null)
        {
            return;
        }

        if (polygon.ContainsPoint(point))
        {
            _scissorsSoundPlayer.Play();
        }

        if (_cut.Points.Count < 2)
        {
            return;
        }

        PolygonShape draftPolygonShape = _draftPolygonShape;

        Vector2 point1 = polygon.ConvertGlobalToLocalPoint(_cut.Points[_cut.Points.Count - 2]);
        Vector2 point2 = polygon.ConvertGlobalToLocalPoint(_cut.Points[_cut.Points.Count - 1]);

        var intersections = draftPolygonShape.GetIntersectionsByLine(point1, point2);
        if (intersections != null && intersections.Count == 1)
        {
            var intersection = intersections[0];
            InsertIntersectionPoint(draftPolygonShape, ref intersection);
            AddCutPoint(intersection.Point);
            _intersectionPointIndeces.Add(intersection.StartEdgeIndex + 1);

            if (_intersectionPointIndeces.Count == 2)
            {
                if (_intersectionPointIndeces[0] >= intersection.StartEdgeIndex + 1)
                {
                    _intersectionPointIndeces[0]++;
                }

                PolygonShape firstShape;
                PolygonShape secondShape;

                var sliceLine = new SliceLine
                {
                    startIndex = _intersectionPointIndeces[0],
                    endIndex = _intersectionPointIndeces[1],
                    CutPoints = _cutPoints.AsReadOnly()
                };
                var (sliceLines, affectedHoles) = CalculateSliceLineVariants(draftPolygonShape, polygon.transform.position, _cutPoints.AsReadOnly());
                Debug.Assert(sliceLines != null && sliceLines.Count > 0);

                if (sliceLines.Count == 1)
                {
                    sliceLine.CutPoints = sliceLines[0].AsReadOnly();
                    (firstShape, secondShape) = Slice(draftPolygonShape, ref sliceLine, SliceType.Both);
                }
                else
                {
                    sliceLine.CutPoints = sliceLines[0].AsReadOnly();
                    firstShape = Slice(draftPolygonShape, ref sliceLine, SliceType.Left).Item1;
                    sliceLine.CutPoints = sliceLines[1].AsReadOnly();
                    secondShape = Slice(draftPolygonShape, ref sliceLine, SliceType.Right).Item2;
                }

                var points = ProcessSlicedShapes(firstShape, secondShape);
                _polygonPointsStack.Push(new List<Vector2>(polygon.Shape.Points));
                polygon.Shape.SetPoints(points);

                if (affectedHoles != null && affectedHoles.Count > 0)
                {
                    foreach (var hole in affectedHoles)
                    {
                        _polygonManager.DeletePolygon(hole);
                    }
                }

                RestCutState();
                SwitchState("CompareState");
                return;
            }
        }
        else if (_intersectionPointIndeces.Count == 1)
        {
            AddCutPoint(polygon.ConvertGlobalToLocalPoint(point));
        }
    }

    private (List<List<Vector2>>, HashSet<Polygon>) CalculateSliceLineVariants(PolygonShape polygonShape, Vector2 pos, IReadOnlyList<Vector2> localCutLine)
    {
        Debug.Assert(polygonShape != null);
        Debug.Assert(localCutLine != null && localCutLine.Count > 0);

        Vector2 ConvertToLocal(Vector2 globalPoint) { return globalPoint - pos; }
        Vector2 ConvertToGlobal(Vector2 localPoint) { return localPoint + pos; }

        List<List<Vector2>> sliceLines = new List<List<Vector2>> { new List<Vector2>() };
        sliceLines[0].Add(localCutLine[0]);

        var affectedHoles = new HashSet<Polygon>();

        for (int i = 1; i < localCutLine.Count; i++)
        {
            var point1 = localCutLine[i - 1];
            var point2 = localCutLine[i];
            var intersectionsWithHoles = FindIntersectionWithHoles(ConvertToGlobal(point1), ConvertToGlobal(point2));
            Debug.Assert(intersectionsWithHoles != null);
            if (intersectionsWithHoles.Count == 0)
            {
                sliceLines.ForEach(list => list.Add(point2));
                continue;
            }
            else if (sliceLines.Count == 1)
            {
                sliceLines.Add(new List<Vector2>(sliceLines[0]));
            }
            Debug.Assert(sliceLines.Count == 2);

            // We don't handle the case where the number of holes is greater than 1,
            // because we assume this case will not happen.
            Debug.Assert(intersectionsWithHoles.Count == 1);

            var holePolygon = intersectionsWithHoles.Keys.ToArray()[0];
            var firstHoleIntersectionInfo = intersectionsWithHoles[holePolygon].ToArray()[0];
            Debug.Assert(holePolygon != null);

            Debug.Assert(affectedHoles != null);
            affectedHoles.Add(holePolygon);

            IntersectionInfo? maybeSecondHoleIntersectionInfo = null;
            for (int j = i + 1; j < localCutLine.Count; j++)
            {
                var point11 = ConvertToGlobal(localCutLine[j - 1]);
                var point22 = ConvertToGlobal(localCutLine[j]);
                var holeIntersections = holePolygon.GetIntersectionsByLine(point11, point22);
                Debug.Assert(holeIntersections != null);
                if (holeIntersections.Count != 0)
                {
                    maybeSecondHoleIntersectionInfo = holeIntersections[0];
                    i = j;
                    break;
                }
            }
            Debug.Assert(maybeSecondHoleIntersectionInfo != null);
            IntersectionInfo secondHoleIntersectionInfo = maybeSecondHoleIntersectionInfo.Value;

            int holeLength = holePolygon.Shape.Points.Count();

            // The left side of the hole.
            int holePointIndex = firstHoleIntersectionInfo.EndEdgeIndex;
            var firstLine = sliceLines[0];
            while (holePointIndex != maybeSecondHoleIntersectionInfo.Value.StartEdgeIndex)
            {
                var globalHolePoint = holePolygon.GetGlobalShapePoint(holePointIndex);
                firstLine.Add(ConvertToLocal(globalHolePoint));
                holePointIndex = (holePointIndex + 1) % holeLength;
            }
            firstLine.Add(ConvertToLocal(holePolygon.ConvertLocalToGlobalPoint(secondHoleIntersectionInfo.Point)));

            // The rigth side of the hole.
            holePointIndex = firstHoleIntersectionInfo.StartEdgeIndex;
            var secondLine = sliceLines[1];
            while (holePointIndex != maybeSecondHoleIntersectionInfo.Value.EndEdgeIndex)
            {
                var globalHolePoint = holePolygon.GetGlobalShapePoint(holePointIndex);
                secondLine.Add(ConvertToLocal(globalHolePoint));
                holePointIndex = (holePointIndex - 1 + holeLength) % holeLength;
            }
            firstLine.Add(ConvertToLocal(holePolygon.ConvertLocalToGlobalPoint(secondHoleIntersectionInfo.Point)));
        }

        return (sliceLines, affectedHoles);
    }

    enum SliceType
    {
        Left, Right, Both,
    }

    struct SliceLine
    {
        public int startIndex;
        public int endIndex;
        public IReadOnlyList<Vector2> CutPoints;
    }

    private (PolygonShape, PolygonShape) Slice(PolygonShape polygonShape, ref SliceLine sliceLine, SliceType sliceType)
    {
        PolygonShape firstShape = null;
        PolygonShape secondShape = null;

        var polygonPoints = polygonShape.Points;

        int IncreaseIndex(int index, int count) { return (index + 1) % count; }
        int DecreaseIndex(int index, int count) { return (index - 1 + count) % count; }

        if (sliceType == SliceType.Left || sliceType == SliceType.Both)
        {
            List<Vector2> points = new List<Vector2>(sliceLine.CutPoints);
            int i = DecreaseIndex(sliceLine.endIndex, polygonPoints.Count);
            while (i != sliceLine.startIndex)
            {
                points.Add(polygonPoints[i]);
                i = DecreaseIndex(i, polygonPoints.Count);
            }
            points.Reverse();
            firstShape = new PolygonShape(points);
        }

        if (sliceType == SliceType.Right || sliceType == SliceType.Both)
        {
            List<Vector2> points = new List<Vector2>(sliceLine.CutPoints);
            int i = IncreaseIndex(sliceLine.endIndex, polygonPoints.Count);
            while (i != sliceLine.startIndex)
            {
                points.Add(polygonPoints[i]);
                i = IncreaseIndex(i, polygonPoints.Count);
            }
            secondShape = new PolygonShape(points);
        }

        return (firstShape, secondShape);
    }

    private Dictionary<Polygon, List<IntersectionInfo>> FindIntersectionWithHoles(Vector2 point1, Vector2 point2)
    {
        var result = new Dictionary<Polygon, List<IntersectionInfo>>();
        var holes = _polygonManager.HolePolygons;
        if (holes == null || holes.Count == 0)
        {
            return result;
        }

        foreach (var hole in holes)
        {
            var intersections = hole.GetIntersectionsByLine(point1, point2);
            if (intersections == null || intersections.Count == 0)
            {
                continue;
            }
            result[hole] = intersections;
        }

        return result;
    }

    private IReadOnlyList<Vector2> ProcessSlicedShapes(PolygonShape firstShape, PolygonShape secondShape)
    {
        float firstArea = firstShape.CalculateArea();
        float secondArea = secondShape.CalculateArea();

        PolygonShape shape = firstArea > secondArea ? firstShape : secondShape;

        return shape.Points;
    }

    private int FindNearestPointIndex(IReadOnlyList<Vector2> points, Vector2 newPoint, float minDistance, int skip = 3)
    {
        for (int i = 0; i < points.Count - skip; i++)
        {
            var currentPoint = points[i];
            float currentDistance = Vector2.Distance(newPoint, currentPoint);
            if (currentDistance <= minDistance)
            {
                return i;
            }
        }
        return -1;
    }

    private bool ProcessHole(Polygon polygon, IReadOnlyList<Vector2> points, Vector2 newPoint)
    {
        if (points.Count < 3 || _HasPolygonIntersections || !polygon || !polygon.ContainsPoint(newPoint))
        {
            return false;
        }

        int nearestPointIndex = FindNearestPointIndex(points, newPoint, _rawHoleDistance);
        if (nearestPointIndex < 0)
        {
            return false;
        }

        Vector2[] holePoints = new Vector2[points.Count - nearestPointIndex];
        for (int i = 0; i < holePoints.Length; i++)
        {
            holePoints[i] = points[i + nearestPointIndex];
        }

        Polygon hole = _polygonManager.CreateHolePolygon(holePoints);

        return true;
    }

    private void OnDrawGizmos()
    {
        if (_currentPolygon != null && _cutPoints != null && _cutPoints.Count > 0)
        {
            Gizmos.color = Color.red;
            foreach (var point in _cutPoints)
            {
                Gizmos.DrawSphere(_currentPolygon.ConvertLocalToGlobalPoint(point), 0.01f);
            }
        }

    }

}
