using UnityEngine;
using TMPro;

public class CompareState : State
{
    [SerializeField] private TextMeshProUGUI _text;

    public override void OnEnter()
    {
        var _polgyonManager = FindAnyObjectByType<PolygonManager>();
        var firstPolygon = _polgyonManager.MainPolygon;
        var secondPolygon = _polgyonManager.TargetPolygon;
        var result = Compare(firstPolygon.Shape, secondPolygon.Shape);

        _text.text = result.ToString();

        SwitchState("CutState");

    }

    private float Compare(PolygonShape firstShape, PolygonShape secondShape)
    {
        return HausdorffDistance.Compare(firstShape.Points, secondShape.Points);
    }
}
