using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteEyeFollow : MonoBehaviour
{
    public Volume volume;
    private Vignette vignette;

    [Header("Fade Settings")]
    public float fadeSpeed = 2f;

    [Header("Pulse Settings")]
    public float minIntensity = 0.2f;
    public float maxIntensity = 0.7f;
    public float pulseFrequency = 2f;


    private float targetIntensity = 0f;
    private float currentIntensity = 0f;

    private float currentRed = 0;
    private float targetRed = .601f;

    Color color;
    void Start()
    {
        if (!volume.profile.TryGet(out vignette))
        {
            Debug.LogWarning("No Vignette found in Volume Profile!");
        }
        color = vignette.color.value;
    }

    void Update()
    {
        if (vignette == null || GameManager.Instance == null) return;
        bool followMouse = false;
        if (GameManager.Instance.GodHandActive)
        {
            followMouse = true;
            float pulse = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * pulseFrequency) + 1f) / 2f;
            targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
            color.r += 4f * Time.unscaledDeltaTime;
            if(color.r > targetRed)
            {
                color.r = targetRed;
            }
        }
        else
        {
            targetIntensity = minIntensity;
            color.r -= 4f * Time.unscaledDeltaTime;
            if (color.r < 0)
            {
                color.r = 0;
            }
        }
        vignette.color.value = color;
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.unscaledDeltaTime * fadeSpeed);

        vignette.intensity.value = currentIntensity;

        if (followMouse)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector2 normalizedPos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
            vignette.center.value = normalizedPos;
        }
        else
        {
            vignette.center.value = new Vector2(0.5f, 0.5f);
        }
    }
}
