using UnityEngine;

public class ZeroTest : MonoBehaviour
{
    public GameObject SomeGameObject;

    private GameObject _someGameObjectRef;

    void Start()
    {
        _someGameObjectRef = SomeGameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Destroy(SomeGameObject);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            print(_someGameObjectRef == null);
        }
    }
}
