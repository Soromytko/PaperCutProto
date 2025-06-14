using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
public class LevelSO : ScriptableObject
{
    public int Attempts = 3;
    [Range(0, 100)]
    public int Accuracy = 50;
}
