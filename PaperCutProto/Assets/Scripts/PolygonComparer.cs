using System.Collections.Generic;
using UnityEngine;

public static class HausdorffDistance
{
    public static float Compare(IReadOnlyList<Vector2> firstPolygon, IReadOnlyList<Vector2> secondPolygon)
    {
        float maxDistA = GetDirectedHausdorff(firstPolygon, secondPolygon);
        float maxDistB = GetDirectedHausdorff(secondPolygon, firstPolygon);
        return Mathf.Max(maxDistA, maxDistB);   
    }

    private static float GetDirectedHausdorff(IReadOnlyList<Vector2> polyA, IReadOnlyList<Vector2> polyB)
    {
        float maxDist = 0f;
        foreach (Vector2 pointA in polyA)
        {
            float minDist = float.MaxValue;
            foreach (Vector2 pointB in polyB)
            {
                float dist = Vector2.Distance(pointA, pointB);
                if (dist < minDist)
                    minDist = dist;
            }
            if (minDist > maxDist)
                maxDist = minDist;
        }
        return maxDist;
    }
}
