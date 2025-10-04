using UnityEngine;
using UnityEngine.UI;

public class VignetteEyeFollow : MonoBehaviour
{
    public Image image;
    public Transform target;
    public Camera cam;

    [Header("Fade")]
    public float fadeSpeed = 2f;

    [Header("Pulse")]
    public float minIntensity = 0.2f;
    public float maxIntensity = 0.7f;
    public float pulseFrequency = 2f;

    [Header("Shape")]
    public float radius = 0.35f;
    public float softness = 0.25f;

    [Header("Color")]
    public float targetRed = 0.601f;

    Material mat;
    float targetIntensity;
    float currentIntensity;
    Color color;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!image) image = GetComponent<Image>();
        if (!image) enabled = false;

        if (image.material != null) mat = Instantiate(image.material);
        else
        {
            var sh = Shader.Find("Unlit/WebGLVignetteFast");
            mat = new Material(sh);
        }

        image.material = mat;
        image.raycastTarget = false;

        color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : new Color(0, 0, 0, 1);
    }

    void OnDestroy()
    {
        if (mat) Destroy(mat);
    }

    void Update()
    {
        bool followMouse = GameManager.Instance != null && GameManager.Instance.GodHandActive;

        if (followMouse)
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * pulseFrequency) + 1f) * 0.5f;
            targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
            color.r = Mathf.MoveTowards(color.r, targetRed, 4f * Time.unscaledDeltaTime);
        }
        else
        {
            targetIntensity = minIntensity;
            color.r = Mathf.MoveTowards(color.r, 0f, 4f * Time.unscaledDeltaTime);
        }

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.unscaledDeltaTime * fadeSpeed);

        Vector2 center = new Vector2(0.5f, 0.5f);
        if (followMouse)
        {
            Vector3 mp = Input.mousePosition;
            center.x = Mathf.Clamp01(mp.x / Screen.width);
            center.y = Mathf.Clamp01(mp.y / Screen.height);
        }

        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

        mat.SetVector("_Center", new Vector4(center.x, center.y, 0, 0));
        mat.SetFloat("_Radius", radius);
        mat.SetFloat("_Softness", softness);
        mat.SetFloat("_Intensity", currentIntensity);
        mat.SetColor("_Color", color);
        mat.SetFloat("_Aspect", aspect);
    }
}
