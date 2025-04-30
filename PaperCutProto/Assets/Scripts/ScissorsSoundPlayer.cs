using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ScissorsSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip[] _audioClips;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Play()
    {
        if (!_audioSource.isPlaying)
        {
            int clipIndex = Random.Range(0, _audioClips.Length);
            _audioSource.clip = _audioClips[clipIndex];
            _audioSource.Play();
        }
    }

}


