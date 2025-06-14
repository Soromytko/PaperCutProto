using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Paper : MonoBehaviour
{
    [SerializeField]private MeshRenderer _meshRenderer;
    private AudioSource _audioSource;
    private Material _material;

    private Vector3 _lastPosition;
    private Vector3 _speedDirection;

    [SerializeField] private float _sharpness = 5.0f;
    [SerializeField] private float _deflection = 10.0f;

    public async Task Move(Vector3 startPosition, Vector3 endPosition, float duration = 1.0f)
    {
        _audioSource.Play();
        await Tweener.MoveTo(gameObject, startPosition, endPosition, duration);
    }

    public async Task DoOrigami()
    {
    }

    public async Task WaitAnimationFinished()
    {
        float epsilon = float.Epsilon * 10f;
        while (_speedDirection.sqrMagnitude > float.Epsilon)
        {
            await Task.Yield();
        }
        _speedDirection = Vector3.zero;
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _material = _meshRenderer.material;
    }

    private void Start()
    {
        _lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Vector3 currentDirection = transform.position - _lastPosition;
        _lastPosition = transform.position;

        _speedDirection = Vector3.Lerp(_speedDirection, -currentDirection * _deflection, Time.fixedDeltaTime * _sharpness);

        _material.SetVector("_ForceDirection", new Vector4(_speedDirection.x, _speedDirection.y, _speedDirection.z, 0));

    }

}
