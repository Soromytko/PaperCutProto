using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DottedLine : MonoBehaviour
{
    public IReadOnlyList<Vector2> Points => _points;
    public int PointCount => _points.Count;

    private List<Vector2> _points = new List<Vector2>();

    private LineRenderer _lineRenderer;

    private float _lineLength = 0;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        UpdateLineRenderer();
    }

    public void SetPoints(IEnumerable<Vector2> points)
    {
        _lineLength = 0;
        _points.Clear();
        _points.AddRange(points);
        for (int i = 1; i < _points.Count; i++)
        {
            _lineLength += Vector2.Distance(_points[i - 1], _points[i]);
        }
        UpdateLineRenderer();
    }

    public void AddPoint(Vector2 point)
    {
        if (_points.Count > 0)
        {
            _lineLength += Vector2.Distance(_points[_points.Count - 1], point);
        }
        _points.Add(point);
        UpdateLineRenderer();
    }

    public void Reset()
    {
        _lineLength = 0;
        _points.Clear();
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        var points = System.Array.ConvertAll(_points.ToArray(), v => new Vector3(v.x, v.y, -1.0f));
        _lineRenderer.positionCount = points.Length;
        _lineRenderer.SetPositions(points);
        _lineRenderer.textureScale = Vector2.one * _lineLength;
    }

}
