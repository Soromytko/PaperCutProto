using System.Collections.Generic;
using UnityEngine;

public static class HausdorffDistance
{
    public static float Compare(Vector2[] firstPolygon, Vector2[] secondPolygon)
    {
        float maxDistA = GetDirectedHausdorff(firstPolygon, secondPolygon);
        float maxDistB = GetDirectedHausdorff(secondPolygon, firstPolygon);
        return Mathf.Max(maxDistA, maxDistB);   
    }

    private static float GetDirectedHausdorff(Vector2[] polyA, Vector2[] polyB)
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

public static class IoU
{
    // Основная функция для вычисления IoU
    public static float CalculateIoU(Vector2[] polyA, Vector2[] polyB)
    {
        // Вычисляем площадь пересечения
        List<Vector2> intersection = GetIntersection(new List<Vector2>(polyA), new List<Vector2>(polyB));
        float intersectionArea = CalculatePolygonArea(intersection);

        // Вычисляем площадь объединения: Area(A) + Area(B) - Area(Intersection)
        float areaA = CalculatePolygonArea(new List<Vector2>(polyA));
        float areaB = CalculatePolygonArea(new List<Vector2>(polyB));

        float unionArea = areaA + areaB - intersectionArea;

        // Избегаем деления на ноль
        if (unionArea < 0.0001f)
            return 0f;

        return intersectionArea / unionArea;
    }

    // Вычисление площади полигона (по формуле шнурков)
    private static float CalculatePolygonArea(List<Vector2> polygon)
    {
        if (polygon.Count < 3)
            return 0f;

        float area = 0f;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 current = polygon[i];
            Vector2 next = polygon[(i + 1) % polygon.Count];
            area += (current.x * next.y) - (next.x * current.y);
        }
        return Mathf.Abs(area / 2f);
    }





    public static List<Vector2> GetIntersection(List<Vector2> polygonA, List<Vector2> polygonB)
    {
        // Проверка на null или пустые полигоны
        if (polygonA == null || polygonB == null || polygonA.Count < 3 || polygonB.Count < 3)
        {
            Debug.LogWarning("Один из полигонов пуст или содержит менее 3 точек");
            return new List<Vector2>();
        }

        // Начинаем с первого полигона
        List<Vector2> output = new List<Vector2>(polygonA);

        // Проходим по всем ребрам второго полигона и последовательно обрезаем
        for (int i = 0; i < polygonB.Count; i++)
        {
            int next = (i + 1) % polygonB.Count;
            Vector2 edgeStart = polygonB[i];
            Vector2 edgeEnd = polygonB[next];

            // Если выходной список пуст - прерываемся
            if (output.Count == 0)
                break;

            // Обрезаем текущий выходной полигон этим ребром
            List<Vector2> input = new List<Vector2>(output);
            output.Clear();

            Vector2 prevPoint = input[input.Count - 1];
            bool prevInside = IsInside(prevPoint, edgeStart, edgeEnd);

            for (int j = 0; j < input.Count; j++)
            {
                Vector2 currentPoint = input[j];
                bool currentInside = IsInside(currentPoint, edgeStart, edgeEnd);

                if (currentInside)
                {
                    if (!prevInside)
                    {
                        Vector2 intersection = FindIntersection(prevPoint, currentPoint, edgeStart, edgeEnd);
                        output.Add(intersection);
                    }
                    output.Add(currentPoint);
                }
                else if (prevInside)
                {
                    Vector2 intersection = FindIntersection(prevPoint, currentPoint, edgeStart, edgeEnd);
                    output.Add(intersection);
                }

                prevPoint = currentPoint;
                prevInside = currentInside;
            }
        }

        return output;
    }

    // Проверка, находится ли точка внутри ребра (для алгоритма обрезки)
    private static bool IsInside(Vector2 point, Vector2 edgeStart, Vector2 edgeEnd)
    {
        // Векторное произведение для определения положения точки относительно ребра
        return (edgeEnd.x - edgeStart.x) * (point.y - edgeStart.y) > 
               (edgeEnd.y - edgeStart.y) * (point.x - edgeStart.x);
    }

    // Нахождение точки пересечения двух отрезков
    private static Vector2 FindIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        // Параметры уравнений прямых
        float A1 = a2.y - a1.y;
        float B1 = a1.x - a2.x;
        float C1 = A1 * a1.x + B1 * a1.y;

        float A2 = b2.y - b1.y;
        float B2 = b1.x - b2.x;
        float C2 = A2 * b1.x + B2 * b1.y;

        // Определитель
        float det = A1 * B2 - A2 * B1;

        if (Mathf.Approximately(det, 0))
        {
            // Прямые параллельны или совпадают
            return Vector2.Lerp(a1, a2, 0.5f); // Возвращаем середину в случае параллельности
        }
        else
        {
            // Точка пересечения
            float x = (B2 * C1 - B1 * C2) / det;
            float y = (A1 * C2 - A2 * C1) / det;
            return new Vector2(x, y);
        }
    }

}
