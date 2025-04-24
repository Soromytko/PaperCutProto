using UnityEngine;

public class Tester : MonoBehaviour
{
    public Polygon FirstPolygon;
    public Polygon SecondPolygon;
    public Polygon IntersectionPolygon;

    public Transform point1;
    public Transform point2;

    private void Update()
    {
        IntersectionPolygon.Shape.Points = FirstPolygon.CutOff(SecondPolygon).Points;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(point1.position, point2.position);

        var maybeIntersections = FirstPolygon.GetIntersectionsByLine(point1.position, point2.position);
        foreach (var item in maybeIntersections) {
            Gizmos.DrawSphere(item.Point + (Vector2)FirstPolygon.transform.position, 0.02f);
        }
    }
}
