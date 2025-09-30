using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SnowflakeFigureProjection : FigureProjection
{
    [SerializeField] private int _layer;
    [SerializeField] private DottedLine _dottedLinePrefab;
    private Polygon[] _polygons;
    private DottedLine[] _dottedLines;

    private void Awake()
    {
        Init();

        _dottedLines = new DottedLine[12];

        _polygons = Enumerable
            .Range(0, 12)
            .Select((i, index) =>
            {
                Polygon result = Instantiate(_polygonPrefab);
                result.transform.parent = transform;
                result.transform.localPosition = Vector3.zero;
                result.gameObject.layer = _layer;

                float angle = 360f / 12f * index;
                result.transform.eulerAngles = new Vector3(0f, 0f, angle);

                float xScale = i % 2 != 0 ? -1 : 1;
                result.transform.localScale = new Vector3(xScale, 1, 1);

                var dottedLine = Instantiate(_dottedLinePrefab);
                dottedLine.transform.parent = result.transform;
                dottedLine.transform.localPosition = Vector3.zero;
                dottedLine.transform.localRotation= Quaternion.identity;
                dottedLine.transform.localScale = Vector3.one;
                dottedLine.GetComponent<LineRenderer>().useWorldSpace = false;
                _dottedLines[i] = dottedLine;

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

    protected override void OnPolygonCutPointsChanged(List<Vector2> points)
    {
        foreach (var dottedLine in _dottedLines)
        {
            dottedLine.SetPoints(points);
        }
    }
}
