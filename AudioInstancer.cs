using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioInstancer : MonoBehaviour
{
    static AudioInstancer manager;
    private AudioSource audioSource;
    public Vector2 modulation = new(0.9f, 1.1f);

    private void Awake()
    {
        if (manager && manager != this)
            Destroy(gameObject);
        else
        { 
            manager = this;
            audioSource = GetComponent<AudioSource>();
        }
    }

    public static void PlayAudioInstance(AudioClip clip, float volume = 1f, bool modulate = true)
    {
        if (clip == null || !manager) return; 
        manager.PlayNonRepeat(clip, volume, modulate);
    }

    private void PlayNonRepeat(AudioClip clip, float volume, bool modulate)
    {
        if (!audioSource) return;

        audioSource.pitch = modulate ? Random.Range(modulation.x, modulation.y) : 1f; 

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private void PlayOneShot(AudioClip clip, float volume, bool modulate)
    {
        if (!audioSource) return;

        audioSource.pitch = modulate ? Random.Range(modulation.x, modulation.y) : 1f;

        audioSource.PlayOneShot(clip, volume);
    }
}
