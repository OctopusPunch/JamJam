using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Audio/Sound Data")]
public class SoundDataSO : ScriptableObject
{
    [System.Serializable]
    public class SoundEntry
    {
        public string name;
        public AudioClip clip;
        public bool loop;
        [Range(0f, 1f)] public float baseVolume = 1f;
        [Range(0f, 1f)] public float pitchVariance = 0f; // ± range
    }

    public SoundEntry[] sounds;

    public SoundEntry GetSound(string soundName)
    {
        foreach (var s in sounds)
        {
            if (s.name == soundName)
                return s;
        }
        return null;
    }
}
