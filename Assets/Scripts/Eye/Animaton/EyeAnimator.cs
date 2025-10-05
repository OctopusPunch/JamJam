using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Renderer))]
public sealed class EyeAnimator : MonoBehaviour
{
    [SerializeField] EyeAnimationSet animationSet;
    [SerializeField] EyeState startState = EyeState.Idle;
    [SerializeField] bool playOnAwake = true;

    public UnityEvent<EyeState> OnStateStarted;
    public UnityEvent<EyeState> OnStateFinished;

    Renderer rend;
    MaterialPropertyBlock mpb;

    readonly Dictionary<EyeState, EyeAnimClip> map = new();
    readonly Dictionary<EyeState, Texture2DArray> arrays = new();

    EyeAnimClip clip;
    int frame;
    float timer;
    float startDelay;
    bool playing;

    static Texture2DArray BuildArray(Texture2D[] src)
    {
        if (src == null || src.Length == 0) return null;
        var w = src[0].width;
        var h = src[0].height;
        var fmt = src[0].format;

        var arr = new Texture2DArray(w, h, src.Length, fmt, false, false);
        arr.filterMode = FilterMode.Point;
        arr.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < src.Length; i++)
        {
            if (!src[i]) continue;
            if (src[i].width != w || src[i].height != h)
            {
                Debug.LogError($"EyeAnimator: Frame {i} size mismatch. All frames must be {w}x{h}.");
                return null;
            }
            Graphics.CopyTexture(src[i], 0, 0, arr, i, 0);
        }
        arr.Apply(false, false);
        return arr;
    }

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();

        map.Clear();
        arrays.Clear();

        if (animationSet != null)
        {
            foreach (var c in animationSet.clips)
            {
                if (!c || map.ContainsKey(c.state)) continue;
                map.Add(c.state, c);

                Texture2DArray arr = null;
                if (c.source == EyeFramesSource.Array)
                    arr = c.framesArray;
                else
                    arr = BuildArray(c.frames);

                arrays[c.state] = arr;
            }
        }

        if (playOnAwake) Play(startState);
    }

    void Update()
    {
        if (!playing || clip == null) return;

        float dt = Time.unscaledDeltaTime;

        if (startDelay > 0f)
        {
            startDelay -= dt;
            return;
        }

        if (clip.fps <= 0f) return;

        int frameCount = GetFrameCount(clip);
        if (frameCount <= 0) return;

        timer += dt;
        var step = 1f / clip.fps;

        if (timer >= step)
        {
            timer -= step;
            frame++;

            if (frame >= frameCount)
            {
                if (clip.loop)
                {
                    frame = 0;
                }
                else
                {
                    var finished = clip.state;
                    OnStateFinished?.Invoke(finished);
                    Play(clip.nextOnComplete);
                    return;
                }
            }
            Push();
        }
    }

    int GetFrameCount(EyeAnimClip c)
    {
        if (c.source == EyeFramesSource.Array)
        {
            int depth = arrays.TryGetValue(c.state, out var arr) && arr ? arr.depth : 0;
            return (c.frameCountOverride > 0 && c.frameCountOverride <= depth)
                ? c.frameCountOverride
                : depth;
        }
        return c.frames != null ? c.frames.Length : 0;
    }

    void Push()
    {
        if (!arrays.TryGetValue(clip.state, out var arr) || !arr) return;
        rend.GetPropertyBlock(mpb);
        mpb.SetTexture("_EyeTexArray", arr);
        mpb.SetFloat("_EyeFrame", frame);
        rend.SetPropertyBlock(mpb);
    }

    public void Play(EyeState state)
    {
        if (!map.TryGetValue(state, out var c)) { playing = false; return; }

        clip = c;
        frame = 0;
        timer = 0f;
        Push();
        playing = true;

        OnStateStarted?.Invoke(state);
    }

    public bool IsPlaying(EyeState state) => playing && clip && clip.state == state;
    public void Stop() => playing = false;
}
