using UnityEngine;

[DefaultExecutionOrder(-10)]
public sealed class EyeAnimationController : MonoBehaviour
{
    public static EyeAnimationController Instance { get; private set; }

    [SerializeField] private EyeAnimator eyeAnimator;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Play(EyeState state)
    {
        if (eyeAnimator)
            eyeAnimator.Play(state);
        else
            Debug.LogWarning("EyeController: Missing EyeShaderAnimator reference.");
    }

    public void Idle() => Play(EyeState.Idle);
    public void TriggerGrab() => Play(EyeState.TriggerGrab);
    public void Grabbed() => Play(EyeState.Grabbed);
    public void Eat()
    {
        Play(EyeState.Eat);
        StartCoroutine(ScreenShake.Instance.TriggerShakeAfterSeconds(.25f, .35f, .5f));
    }
    public void Close() => Play(EyeState.Close);
}
