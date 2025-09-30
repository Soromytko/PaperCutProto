using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Cut : DottedLine
{
    public void RemovePoints(int count)
    {
        var curentPoints = Points;
        int normalizedCount = Mathf.Clamp(count, 0, curentPoints.Count);
        if (normalizedCount > 0)
        {
            List<Vector2> newPoints = new List<Vector2>();
            newPoints.AddRange(curentPoints.Take(curentPoints.Count - normalizedCount));
            SetPoints(newPoints);
        }
    }

}
