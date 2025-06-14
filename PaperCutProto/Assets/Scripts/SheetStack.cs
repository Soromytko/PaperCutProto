using System.Collections.Generic;
using UnityEngine;

public class SheetStack : MonoBehaviour
{
    [SerializeField] private float _angle = 50f;
    [SerializeField] private Paper _paperPrefab;

    private List<Paper> _papers = new List<Paper>();

    public void SetCount(int count)
    {
        if (count < 0 || count == _papers.Count)
        {
            return;
        }

        foreach (var paper in _papers)
        {
            Destroy(paper.gameObject);
        }

        _papers.Clear();

        for (int i = 0; i < count; i++)
        {
            Paper paper = Instantiate(_paperPrefab, transform);
            _papers.Add(paper);
        }

        UpdatePlacement();
    }

    public Paper Pop()
    {
        if (_papers.Count > 0)
        {
            var result = _papers[_papers.Count - 1];
            _papers.RemoveAt(_papers.Count - 1);
            return result;
        }

        return null;
    }

    private void UpdatePlacement()
    {
        if (_papers.Count == 0)
        {
            return;
        }

        float deltaAngle = _angle / _papers.Count;
        float currentAngle = 0f;
        foreach (var paper in _papers)
        {
            paper.transform.localEulerAngles = new Vector3(currentAngle, 90f, -90);
            currentAngle += deltaAngle;
        }
    }
}
