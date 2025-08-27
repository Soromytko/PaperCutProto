using UnityEngine;
using System.Collections.Generic;

public class FigureProjection : MonoBehaviour
{
    [SerializeField] private Polygon _polygon;
    [SerializeField] protected Polygon _polygonPrefab;
    [SerializeField] private CutState _cutState;

    protected void Init()
    {
        _polygon.Shape.PointsChanged.AddListener(OnShapePointsChanged);
        _cutState.CutPointsChanged.AddListener(OnCutStateCutPointsChanged);
    }

    protected virtual void OnPolygonShapeChanged(PolygonShape shape)
    {
    }

    protected virtual void OnPolygonCutPointsChanged(List<Vector2> cutPoints)
    {
    }

    private void OnShapePointsChanged()
    {
        OnPolygonShapeChanged(_polygon.Shape);
    }

    private void OnCutStateCutPointsChanged(List<Vector2> cutPoints)
    {
        OnPolygonCutPointsChanged(cutPoints);
    }

}
