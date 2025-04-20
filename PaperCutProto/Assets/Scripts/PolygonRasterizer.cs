using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PolygonRasterizer : MonoBehaviour
{
    public List<Vector2> Points
    {
        get => _points;
        set
        {
            if (_points != value)
            {
                _points = value;
                if (_isReady)
                {
                    TriangulatePolygon();
                }
            }
        }
    }

    // Counter-Clockwise (CCW)
    private List<Vector2> _points = new List<Vector2>();
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private bool _isReady = false;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.materials[0].doubleSidedGI = true;
        _isReady = true;
        TriangulatePolygon();
    }

    private void TriangulatePolygon()
    {
        var polygonPoints = _points.ToArray();

        int[] triangles = Triangulate(polygonPoints);

        Mesh mesh = new Mesh();
        mesh.vertices = System.Array.ConvertAll(polygonPoints, v => new Vector3(v.x, v.y, 0));
        mesh.triangles = triangles;

        GetComponent<MeshFilter>().mesh = mesh;
    }

    // EarClipping
    public int[] Triangulate(Vector2[] polygon)
    {
        List<int> indices = new List<int>();
        
        // Если полигон имеет менее 3 вершин, триангуляция невозможна
        if (polygon.Length < 3)
            return indices.ToArray();

        // Создаём список индексов вершин
        List<int> vertexIndices = new List<int>();
        for (int i = 0; i < polygon.Length; i++)
            vertexIndices.Add(i);

        // Пока остаются вершины для обработки
        while (vertexIndices.Count > 3)
        {
            bool earFound = false;

            // Перебираем все вершины в поисках "уха"
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                int prev = (i == 0) ? vertexIndices.Count - 1 : i - 1;
                int current = i;
                int next = (i == vertexIndices.Count - 1) ? 0 : i + 1;

                Vector2 a = polygon[vertexIndices[prev]];
                Vector2 b = polygon[vertexIndices[current]];
                Vector2 c = polygon[vertexIndices[next]];

                // Проверяем, является ли текущая вершина "ухом"
                if (IsEar(a, b, c, polygon, vertexIndices))
                {
                    // Добавляем треугольник в результат
                    indices.Add(vertexIndices[prev]);
                    indices.Add(vertexIndices[current]);
                    indices.Add(vertexIndices[next]);

                    // Удаляем текущую вершину (она больше не нужна)
                    vertexIndices.RemoveAt(current);
                    earFound = true;
                    break;
                }
            }

            // Если "ухо" не найдено, полигон не простой (возможно, самопересекающийся)
            if (!earFound)
            {
                Debug.LogError("Triangulation failed: Polygon may be self-intersecting or degenerate.");
                return new int[0];
            }
        }

        // Добавляем последний оставшийся треугольник
        indices.Add(vertexIndices[0]);
        indices.Add(vertexIndices[1]);
        indices.Add(vertexIndices[2]);

        return indices.ToArray();
    }

    // Проверяет, является ли треугольник ABC "ухом" (не содержит других вершин внутри)
    private static bool IsEar(Vector2 a, Vector2 b, Vector2 c, Vector2[] polygon, List<int> vertexIndices)
    {
        // Если угол выпуклый (> 180°), это не "ухо"
        if (!IsConvex(a, b, c))
            return false;

        // Проверяем, что внутри треугольника ABC нет других вершин
        for (int i = 0; i < vertexIndices.Count; i++)
        {
            Vector2 p = polygon[vertexIndices[i]];
            if (p == a || p == b || p == c)
                continue;

            if (IsPointInTriangle(a, b, c, p))
                return false;
        }

        return true;
    }

    // Проверяет, является ли угол ABC выпуклым (поворот против часовой стрелки)
    private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        return cross > 0; // Если > 0 — выпуклый (CCW)
    }

    // Проверяет, находится ли точка P внутри треугольника ABC
    private static bool IsPointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    {
        // Используем барицентрические координаты
        float alpha = ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) /
                      ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y));
        float beta = ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) /
                     ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y));
        float gamma = 1 - alpha - beta;

        return alpha > 0 && beta > 0 && gamma > 0;
    }
}
