using UnityEngine;

public class ProcessPolygonState : State
{
    private PolygonManager _polygonManager;

    private void Awake()
    {
        _polygonManager = FindAnyObjectByType<PolygonManager>();
    }

    public override void OnEnter()
    {
        var polygons = _polygonManager.Polygons;

        if (polygons.Count < 2)
        {
            return;            
        }

        var polygon = polygons[0];
        var area = polygon.Shape.CalculateArea();

        for (int i = 1; i < polygons.Count; i++)
        {
            var currentPolygon = polygons[i];
            var currentArea = currentPolygon.Shape.CalculateArea();
            if (currentArea < area)
            {
                area = currentArea;
                polygon = currentPolygon;
            }
        }
        
        _polygonManager.DeletePolygon(polygon);

        // SwitchState("CutState");
        SwitchState("CompareState");
    }

}
