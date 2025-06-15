using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PaperFolding : MonoBehaviour
{
    [SerializeField] private AudioClip[] _clips;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void OnPaperFolded()
    {
        if (_clips == null || _clips.Length == 0)
        {
            return;
        }

        int index = Random.Range(0, _clips.Length);
        _audioSource.clip = _clips[index];
        _audioSource.Play();
    }   
}
