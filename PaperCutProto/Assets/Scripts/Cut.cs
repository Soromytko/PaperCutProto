using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Cut : MonoBehaviour
{
    public IReadOnlyList<Vector2> Points => _points;
    public int PointCount => _points.Count;

    private List<Vector2> _points = new List<Vector2>();
    private bool _isCorrect = false;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        UpdateLineRenderer();
    }

    public void AddPoint(Vector2 point)
    {
        _points.Add(point);
        UpdateLineRenderer();
    }

    public void RemovePoints(int count)
    {
        int normalizedCount = Mathf.Clamp(count, 0, _points.Count);
        _points.RemoveRange(_points.Count - normalizedCount, normalizedCount);
    }

    public void Reset()
    {
        _points.Clear();
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        var points = System.Array.ConvertAll(_points.ToArray(), v => new Vector3(v.x, v.y, -1.0f));
        _lineRenderer.positionCount = points.Length;
        _lineRenderer.SetPositions(points);
    }

}
