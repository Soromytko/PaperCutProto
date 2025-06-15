using System.Threading.Tasks;
using UnityEngine;

public class LevelLauncher : MonoBehaviour
{
    [SerializeField] private Transform _paperStartPoint;
    [SerializeField] private Transform _paperEndPoint;
    [SerializeField] private Transform _paperStackStartPoint;
    [SerializeField] private Transform _paperStackEndPoint;
    [SerializeField] private SheetStack _paperStack;
    [SerializeField] private Paper _paperPrefab;

    private Paper _paper;

    public void StartLevel(int levelNumber)
    {
        StartlevelAsync(levelNumber);
    }

    private async void StartlevelAsync(int levelNumber)
    {
        LevelSO levelData = LoadLevelData(levelNumber);

        _paperStack.SetCount(levelData.Attempts);
        await Tweener.MoveTo(_paperStack.gameObject, _paperStackStartPoint.position, _paperStackEndPoint.position, 0.2f);
        await Task.Delay(500);
        var paper = _paperStack.Pop();
        await Task.WhenAll(
            paper.Move(paper.transform.position, _paperEndPoint.position, 0.5f),
            Tweener.RotateTo(paper.gameObject, paper.transform.rotation, Quaternion.Euler(-90f, 0f, 0f), 0.5f)
        );
        // // Wait a second to complete the animation.
        await Task.Delay(1000);
        await paper.DoOrigami();

        

        // _paper = CreatePaper();

        // await _paper.Move(_paperStartPoint.position, _paperEndPoint.position, 0.5f);
        // // Wait a second to complete the animation.
        // await Task.Delay(1000);
        // await _paper.DoOrigami();

        print("Moving Completed");
    }

    private LevelSO LoadLevelData(int levelNumber)
    {
        string path = $"Levels/Level{levelNumber}";
        return Resources.Load<LevelSO>(path);
    }

    private Paper CreatePaper()
    {
        Paper paper = Instantiate(_paperPrefab);
        paper.transform.position = _paperStartPoint.position;
        return paper;
    }
    

}
   
