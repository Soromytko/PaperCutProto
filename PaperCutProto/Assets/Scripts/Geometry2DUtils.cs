using UnityEngine;

public static class Geometry2DUtils
{
    public static Vector2? GetLineIntersection(Vector2 pointA, Vector2 pointB, Vector2 pointC, Vector2 pointD)
    {
        Vector2 b = pointB - pointA;
        Vector2 d = pointD - pointC;

        float det = b.x * d.y - b.y * d.x;

        if (Mathf.Abs(det) < float.Epsilon)
        {
            return null;
        }

        Vector2 c = pointC - pointA;

        float t = (c.x * d.y - c.y * d.x) / det;
        float u = (c.x * b.y - c.y * b.x) / det;

        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            return pointA + t * b;
        }

        return null;
    }
}