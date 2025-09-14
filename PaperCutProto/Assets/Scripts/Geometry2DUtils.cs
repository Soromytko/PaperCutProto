using System.Collections.Generic;
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

    public enum WindingOrder
    {
        Clockwise,
        Counterclockwise,
        Collinear,
        Degenerate,
    }

    public static WindingOrder GetPointWindingOrder(IReadOnlyList<Vector2> points)
    {
        if (points == null || points.Count < 2)
        {
            return WindingOrder.Collinear;
        }

        double area = 0;
        int n = points.Count;

        for (int i = 0; i < n; i++)
        {
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % n];
            area += (current.x * next.y) - (next.x * current.y);
        }

        area /= 2;

        if (area > 0)
        {
            return WindingOrder.Counterclockwise;
        }
        else if (area < 0)
        {
            return WindingOrder.Clockwise;
        }
        return WindingOrder.Degenerate;
    }
}