using UnityEngine;

public class FigureProjection : MonoBehaviour
{
    [SerializeField] private Polygon _polygon;
    [SerializeField] protected Polygon _polygonPrefab;

    protected void Init()
    {
        _polygon.Shape.PointsChanged.AddListener(OnShapePointsChanged);
    }

    protected virtual void OnPolygonShapeChanged(PolygonShape shape)
    {

    }

    private void OnShapePointsChanged()
    {
        OnPolygonShapeChanged(_polygon.Shape);
    }
}
