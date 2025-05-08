using System.Linq;
using UnityEngine;

public class CheckTriangle : MonoBehaviour
{
    [SerializeField] private Transform[] _points;


    private Vector3 GetCenter()
    {
        return _points
            .Select(x => x.position)
            .Aggregate((acc, val) => acc + val) / _points.Length;
    }
    
    private void OnDrawGizmos()
    {
        if (_points == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < _points.Length; i++)
        {
            Gizmos.DrawLine(_points[i].position, _points[(i + 1) % _points.Length].position);
        }

        Vector3 center = GetCenter();
        Gizmos.DrawSphere(center, 0.1f);
    }
}
