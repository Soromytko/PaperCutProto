using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class SnowflakeFigureProjection : FigureProjection
{
    private Polygon[] _polygons;
    private LineRenderer[] _lineRenderers;

    private void Awake()
    {
        Init();

        _lineRenderers = new LineRenderer[12];

        _polygons = Enumerable
            .Range(0, 12)
            .Select((i, index) =>
            {
                Polygon result = Instantiate(_polygonPrefab);
                result.transform.parent = transform;
                result.transform.localPosition = Vector3.zero;

                float angle = 360f / 12f * index;
                result.transform.eulerAngles = new Vector3(0f, 0f, angle);

                float xScale = i % 2 != 0 ? -1 : 1;
                result.transform.localScale = new Vector3(xScale, 1, 1);

                var lineRenderer = result.gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.useWorldSpace = false;
                _lineRenderers[index] = lineRenderer;

                return result;
            }).ToArray();
    }

    protected override void OnPolygonShapeChanged(PolygonShape shape)
    {
        foreach (var polygon in _polygons)
        {
            polygon.Shape.SetPoints(shape.Points);
        }
    }

    protected override void OnPolygonCutPointsChanged(List<Vector2> cutPoints)
    {
        var points = System.Array.ConvertAll(cutPoints.ToArray(), v => new Vector3(v.x, v.y, -1.0f));
        foreach (var lineRenderer in _lineRenderers)
        {
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }
    }
}
