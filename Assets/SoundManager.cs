using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    private bool _isPlayingOneTimeSound = false;
    private static SoundManager _instance;
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

    public void PlaySound(AudioClip audioSource, Transform transform, float volume, bool oneTime = false)
    {
        if (_isPlayingOneTimeSound && oneTime)
            return;

        _isPlayingOneTimeSound = true;
        AudioSource audio = Instantiate(_audioSource, transform.position, Quaternion.identity);
        audio.clip = audioSource;
        audio.volume = volume;
        audio.Play();

        float duration = audio.clip.length;
        Destroy(audio.gameObject, duration);

        if (oneTime)
        {
            StartCoroutine(ResetOneTimeSoundFlag(duration));
        }
    }

    private IEnumerator ResetOneTimeSoundFlag(float duration)
    {
        yield return new WaitForSeconds(duration);
        _isPlayingOneTimeSound = false;
    }

    public void PlayLoopSound(AudioClip audioSource, Transform transform, float volume)
    {
        AudioSource audio = Instantiate(_audioSource, transform.position, Quaternion.identity);
        audio.clip = audioSource;
        audio.volume = volume;
        audio.loop = true;
        audio.Play();
    }

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

    public void PlayerAmbientSound(AudioClip audioSource, float volume)
    {
        AudioSource audio = Instantiate(_audioSource, Vector3.zero, Quaternion.identity);
        audio.clip = audioSource;
        audio.volume = volume;
        audio.loop = true;
        audio.Play();
    }
}
