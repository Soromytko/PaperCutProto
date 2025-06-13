using UnityEngine;

public class Paper : MonoBehaviour
{
    private Material _material;

    private Vector3 _lastPosition;
    private Vector3 _speedDirection;

    [SerializeField] private float _sharpness = 5.0f;
    [SerializeField] private float _deflection = 10.0f;

    private void Start()
    {
        _material = GetComponent<MeshRenderer>().material;

        _lastPosition = transform.position ;
    }

    private void FixedUpdate()
    {
        Vector3 currentDirection = transform.position - _lastPosition;
        _lastPosition = transform.position;

        _speedDirection = Vector3.Lerp(_speedDirection, -currentDirection * _deflection, Time.fixedDeltaTime * _sharpness);

        _material.SetVector("_ForceDirection", new Vector4(_speedDirection.x, _speedDirection.y, _speedDirection.z, 0));

    }
}
