using UnityEngine;

public sealed class EyeBob : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float amplitude = 0.08f;
    [SerializeField] float frequency = 0.8f;
    [SerializeField] float smoothTime = 0.12f;
    [SerializeField] bool useUnscaledTime = false;

    Vector3 baseLocal;
    float velY;
    bool initialized;

    void OnEnable()
    {
        if (!target) return;
        baseLocal = target.localPosition;
        velY = 0f;
        initialized = true;
    }

    void Update()
    {
        if (!initialized || !target) return;

        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        float y = amplitude * Mathf.Sin(t * Mathf.PI * 2f * frequency);
        float targetY = baseLocal.y + y;
        float newY = Mathf.SmoothDamp(target.localPosition.y, targetY, ref velY, smoothTime);

        var lp = target.localPosition;
        lp.y = newY;
        target.localPosition = lp;
    }

    public void SetTarget(Transform t)
    {
        target = t;
        if (enabled && t)
        {
            baseLocal = t.localPosition;
            velY = 0f;
            initialized = true;
        }
    }

    public void ResetBaseToCurrent()
    {
        if (!target) return;
        baseLocal = target.localPosition;
    }
}
