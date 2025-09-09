using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PolygonManager : MonoBehaviour
{
    public List<Polygon> Polygons => _polygons;
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

    public Polygon CreateHolePolygon(Vector2[] points)
    {
        Polygon result = Instantiate(_holePolygonPrefab);
        result.Shape.SetPoints(points.ToList());
        result.transform.parent = transform;
        _holePolygons.Add(result);
        return result;
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

}
