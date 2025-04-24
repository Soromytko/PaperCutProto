using System.Linq;
using UnityEngine;

public class SnowflakeFigureProjection : FigureProjection
{
    private Polygon[] _polygons;

    private void Awake()
    {
        Init();

        _polygons = Enumerable
            .Range(0, 12)
            .Select((i, index) => {
                Polygon result = Instantiate(_polygonPrefab);
                result.transform.parent = transform;
                result.transform.localPosition = Vector3.zero;
                
                float angle = 360f / 12f * index;
                result.transform.eulerAngles = new Vector3(0f, 0f, angle);
                
                float xScale = i % 2 != 0 ? -1 : 1;
                result.transform.localScale = new Vector3(xScale, 1, 1);

                return result;
            }).ToArray();
    }

    protected override void OnPolygonShapeChanged(PolygonShape shape)
    {
        foreach (var polygon in _polygons)
        {
            polygon.Shape.Points = shape.Points;
        }
    }
}
