using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] SoundDataSO soundData;
    [SerializeField] AudioSource oneShotSource;
    [SerializeField] AudioSource stepSource;

    readonly Dictionary<string, AudioSource> activeSources = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Play(string soundName)
    {
        if (!soundData) return;

        var entry = soundData.GetSound(soundName);
        if (entry == null)
        {
            Debug.LogWarning($"Sound '{soundName}' not found in {soundData.name}");
            return;
        }

        float pitch = 1f;
        if (entry.pitchVariance > 0f)
            pitch = 1f + Random.Range(-entry.pitchVariance, entry.pitchVariance);

        if (entry.loop)
        {
            // Skip if already looping
            if (activeSources.ContainsKey(soundName)) return;

            var src = gameObject.AddComponent<AudioSource>();
            src.clip = entry.clip;
            src.loop = true;
            src.volume = entry.baseVolume;
            src.pitch = pitch;
            src.Play();

            activeSources[soundName] = src;
        }
        else
        {
            if (!oneShotSource)
            {
                oneShotSource = gameObject.AddComponent<AudioSource>();
                oneShotSource.playOnAwake = false;
            }

            if(entry.name == "Step")
            {
                stepSource.PlayOneShot(entry.clip, entry.baseVolume);
                return;
            }

            oneShotSource.pitch = pitch;
            oneShotSource.PlayOneShot(entry.clip, entry.baseVolume);
        }
    }

    public void Stop(string soundName)
    {
        if (activeSources.TryGetValue(soundName, out var src))
        {
            src.Stop();
            Destroy(src);
            activeSources.Remove(soundName);
        }
    }

    public void StopAll()
    {
        foreach (var src in activeSources.Values)
        {
            if (src) Destroy(src);
        }
        activeSources.Clear();
    }
}
