using UnityEngine;

public class PlayMusicWithDelay : MonoBehaviour
{
    public float delay = 1;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Invoke("PlayMusic", delay);
    }

    void PlayMusic()
    {
        audioSource.Play();
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }
}