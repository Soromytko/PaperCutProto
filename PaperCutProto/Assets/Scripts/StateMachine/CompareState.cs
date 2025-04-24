using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompareState : State
{
    [SerializeField] private TextMeshProUGUI _text;

    public Vector2[] _res;

    public override void OnEnter()
    {
        var _polgyonManager = FindAnyObjectByType<PolygonManager>();
        var firstPolygon = _polgyonManager.MainPolygon;
        var secondPolygon = _polgyonManager.TargetPolygon;
        var offset = _polgyonManager.TargetPolygon.transform.position - _polgyonManager.MainPolygon.transform.position;
        var result = CompareByIoU(firstPolygon.Shape, secondPolygon.Shape, offset);

        _text.text = result.ToString();

        SwitchState("CutState");
    }

    private float CompareByHausdorffDistance(PolygonShape firstShape, PolygonShape secondShape)
    {
        return HausdorffDistance.Compare(firstShape.Points.ToArray(), secondShape.Points.ToArray());
    }

    private float CompareByIoU(PolygonShape firstShape, PolygonShape secondShape, Vector2 offset)
    {
        var intersectionShape = firstShape.CutOff(secondShape, offset);

        var firstArea = firstShape.CalculateArea();
        var secondArea = secondShape.CalculateArea();
        var intersectionArea = intersectionShape.CalculateArea();

        Debug.Log(intersectionArea);

        float unionArea = firstArea + secondArea - intersectionArea;

        if (unionArea < 0.0001f)
        {
            return 0f;
        }

        return intersectionArea / unionArea;
 

        //return IoU.CalculateIoU(firstShape.Points.ToArray(), secondShape.Points.ToArray());
    }
}
