using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    private bool _isPlayingOneTimeSound = false;
    private static SoundManager _instance;

    // Singleton instance
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SoundManager>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject(nameof(SoundManager));
                    _instance = singleton.AddComponent<SoundManager>();
                    DontDestroyOnLoad(singleton);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Play a sound once
    public void PlaySound(AudioClip audioSource, Transform transform, float volume)
    {
        AudioSource audio = Instantiate(_audioSource, transform.position, Quaternion.identity);
        audio.clip = audioSource;
        audio.volume = volume;
        audio.Play();

        float duration = audio.clip.length;
        Destroy(audio.gameObject, duration);
    }

    // Play a one-time sound, ensuring no overlap
    public void PlayOneTimeSound(AudioClip audioSource, Transform transform, float volume)
    {
        if (_isPlayingOneTimeSound)
            return;

        _isPlayingOneTimeSound = true;
        AudioSource audio = Instantiate(_audioSource, transform.position, Quaternion.identity);
        audio.clip = audioSource;
        audio.volume = volume;
        audio.Play();

        float duration = audio.clip.length;
        Destroy(audio.gameObject, duration);

        StartCoroutine(ResetOneTimeSoundFlag(duration));
    }

    // Reset the one-time sound flag after duration
    private IEnumerator ResetOneTimeSoundFlag(float duration)
    {
        yield return new WaitForSeconds(duration);
        _isPlayingOneTimeSound = false;
    }

    // Play a looping sound
    public void PlayLoopSound(AudioClip audioSource, Transform transform, float volume)
    {
        AudioSource audio = Instantiate(_audioSource, transform.position, Quaternion.identity);
        audio.clip = audioSource;
        audio.volume = volume;
        audio.loop = true;
        audio.Play();
    }

    // Stop a specific sound
    public void StopSound(AudioClip audioSource)
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        
        foreach (var audio in allAudioSources)
        {
            if (audio.clip == audioSource)
            {
                audio.Stop();
                Destroy(audio.gameObject);
                break;
            }
        }
    }
}
