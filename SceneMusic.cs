using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SceneMusic : MonoBehaviour
{
    public AudioSource AudioSource { get; private set; }
    public AudioClip[] songs;

    // Keeps track of what has already played in the current cycle
    private static readonly List<int> playedIndexes = new();
    private static int currentIndex = -1;

    private void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
        AudioSource.loop = false;
    }

    private void Start()
    {
        PlayNextRandomSong();
    }

    private void Update()
    {
        if (!IsReady()) return;

        // Song finished -> play next
        if (currentIndex != -1 && !AudioSource.isPlaying && AudioSource.clip != null)
            PlayNextRandomSong();
    }

    public void PlayNextRandomSong()
    {
        if (!IsReady()) return;

        int index = GetNextUnplayedIndex();
        currentIndex = index;

        AudioSource.clip = songs[index];
        AudioSource.Play();
    }

    public void PlayIndexSong(int index)
    {
        if (!IsReady()) return;
        if (index < 0 || index >= songs.Length) return;

        // Mark as played so it won't repeat until the pool resets
        if (!playedIndexes.Contains(index))
            playedIndexes.Add(index);

        currentIndex = index;
        AudioSource.clip = songs[index];
        AudioSource.Play();
    }

    private int GetNextUnplayedIndex()
    {
        // Reset cycle once all songs have been played
        if (playedIndexes.Count >= songs.Length)
            playedIndexes.Clear();

        // Build candidate pool (unplayed only)
        List<int> candidates = new();
        for (int i = 0; i < songs.Length; i++)
        {
            if (songs[i] == null) continue; // skip empty slots safely
            if (!playedIndexes.Contains(i))
                candidates.Add(i);
        }

        // If everything left is null or weird, fall back safely
        if (candidates.Count == 0)
        {
            // If there are songs but all are null, return 0 (will no-op elsewhere if null)
            return 0;
        }

        int pick = candidates[Random.Range(0, candidates.Count)];
        playedIndexes.Add(pick);
        return pick;
    }

    private bool IsReady()
    {
        // Note: your original code used && which was inverted.
        // We want to return false if AudioSource is missing OR there are no songs.
        if (AudioSource == null) return false;
        if (songs == null || songs.Length == 0) return false;
        return true;
    }
}