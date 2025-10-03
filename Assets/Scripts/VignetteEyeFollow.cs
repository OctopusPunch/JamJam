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

    [SerializeField]
    private bool followMouse = true;

    private float targetIntensity = 0f;
    private float currentIntensity = 0f;

    void Start()
    {
        if (!volume.profile.TryGet(out vignette))
        {
            Debug.LogWarning("No Vignette found in Volume Profile!");
        }
    }

    void Update()
    {
        if (vignette == null || GameManager.Instance == null) return;

        if (GameManager.Instance.GodHandActive)
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * pulseFrequency) + 1f) / 2f;
            targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        }
        else
        {
            targetIntensity = 0f;
        }

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.unscaledDeltaTime * fadeSpeed);

        vignette.intensity.value = currentIntensity;

        if (followMouse)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector2 normalizedPos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
            vignette.center.value = normalizedPos;
        }
    }
}
