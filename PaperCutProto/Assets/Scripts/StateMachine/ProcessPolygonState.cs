using UnityEngine;

public class ProcessPolygonState : State
{
    public override void OnEnter()
    {
        var polygons = GetPolygons();

        if (polygons.Length == 0)
        {
            return;            
        }

        var polygon = polygons[0];
        var area = polygon.Shape.CalculateArea();

        for (int i = 1; i < polygons.Length; i++)
        {
            var currentPolygon = polygons[i];
            var currentArea = currentPolygon.Shape.CalculateArea();
            if (currentArea < area)
            {
                area = currentArea;
                polygon = currentPolygon;
            }
        }

        Destroy(polygon.gameObject);

        SwitchState("CutState");
    }

    private Polygon[] GetPolygons()
    {
        return FindObjectsByType<Polygon>(FindObjectsSortMode.None);
    }

}
