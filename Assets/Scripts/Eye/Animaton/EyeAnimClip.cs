using UnityEngine;

public enum EyeState { Idle, TriggerGrab, Grabbed, Eat, Close }

public enum EyeFramesSource { Frames, Array }

[CreateAssetMenu(fileName = "EyeAnimClip", menuName = "Eye/Eye Anim Clip")]
public sealed class EyeAnimClip : ScriptableObject
{
    public EyeState state = EyeState.Idle;

    public EyeFramesSource source = EyeFramesSource.Array;
    public Texture2DArray framesArray;
    public Texture2D[] frames;
    public int frameCountOverride = 0;

    public float fps = 12f;
    public bool loop = true;
    public EyeState nextOnComplete = EyeState.Idle;
}
