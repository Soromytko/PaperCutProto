using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PolygonManager : MonoBehaviour
{
    public List<Polygon> Polygons => _polygons;
    public IReadOnlyList<Polygon> HolePolygons => _holePolygons.AsReadOnly();
    public Polygon MainPolygon => _mainPolygon;
    public Polygon TargetPolygon => _targetPolygon;

    [SerializeField] private Polygon _polygonPrefab;
    [SerializeField] private Polygon _holePolygonPrefab;
    [SerializeField] private Polygon _mainPolygon;
    [SerializeField] private Polygon _targetPolygon;

    private List<Polygon> _polygons = new List<Polygon>();
    private List<Polygon> _holePolygons = new List<Polygon>();

    private void Awake()
    {
        if (_mainPolygon != null)
        {
            _polygons.Add(_mainPolygon);
        }
    }

    public void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetMainPolygon(Polygon polygon)
    {
        if (!_polygons.Contains(polygon))
        {
            _polygons.Add(polygon);
        }
        _mainPolygon = polygon;
    }

    public Polygon CreatePolygon(Vector2[] points)
    {
        Polygon result = Instantiate(_polygonPrefab);
        result.Shape.SetPoints(points);
        result.transform.parent = transform;
        _polygons.Add(result);
        return result;
    }

    public Polygon CreateHolePolygon(Vector2[] points, bool merge = true)
    {
        Polygon hole = Instantiate(_holePolygonPrefab);
        hole.Shape.SetPoints(points.ToList());
        hole.transform.parent = transform;
        _holePolygons.Add(hole);
        return merge ? MergeHoles(hole) : hole;
    }

    public void DeletePolygon(Polygon polygon)
    {
        bool isMainPolygon = polygon == _mainPolygon;

        _polygons.Remove(polygon);
        Destroy(polygon.gameObject);

        if (isMainPolygon)
        {
            _mainPolygon = _polygons.Count > 0 ? _polygons[0] : null;
        }
    }

    private Polygon MergeHoles(Polygon hole)
    {
        Debug.Assert(hole != null);
        for (int i = 0; i < _holePolygons.Count; i++)
        {
            var currentHole = _holePolygons[i];
            if (hole == currentHole)
            {
                continue;
            }
            var polygonRelation = hole.DeterminePolygonRelation(currentHole);

            if (polygonRelation == Polygon.Relation.Intersection)
            {
                if (hole.Merge(currentHole))
                {
                    _holePolygons.RemoveAt(i);
                    Destroy(currentHole.gameObject);
                    i--;
                }
            }
            else if (polygonRelation == Polygon.Relation.Within)
            {
                Destroy(hole.gameObject);
                return currentHole;
            }
            else if (polygonRelation == Polygon.Relation.Outside)
            {
                Destroy(currentHole.gameObject);
                _holePolygons.RemoveAt(i);
                i--;
            }
        }

        return hole;
    }

}
