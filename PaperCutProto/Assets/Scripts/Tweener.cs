using System.Threading.Tasks;
using UnityEngine;

public static class Tweener
{
    public static async Task MoveTo(GameObject obj, Vector3 startPosition, Vector3 endPosition, float duration = 1f)
    {
        duration = Mathf.Max(0.01f, duration);

        float progress = 0f;
        while (true)
        {
            progress += Time.deltaTime / duration;
            if (progress >= 1f)
            {
                break;
            }
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            await Task.Yield();
        }

        obj.transform.position = endPosition;
    }

    public static async Task RotateTo(GameObject obj, Quaternion startRotation, Quaternion endRotation, float duration = 1f)
    {
        duration = Mathf.Max(0.01f, duration);

        float progress = 0f;
        while (true)
        {
            progress += Time.deltaTime / duration;
            if (progress >= 1f)
            {
                break;
            }
            obj.transform.rotation = Quaternion.Slerp(startRotation, endRotation, progress);
            await Task.Yield();
        }

        obj.transform.rotation = endRotation;
    }
}
