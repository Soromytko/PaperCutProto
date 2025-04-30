using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ScissorsSoundPlayer : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Play()
    {
        if (!_audioSource.isPlaying)
        {
            _audioSource.Play();
        }
    }

}


