using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class EyeFollow : MonoBehaviour
{
    [SerializeField] string propertyName = "_EyeOffsetPX";
    [SerializeField] int center = 64;
    [SerializeField] int halfRange = 32;
    [SerializeField] float smoothTime = 0.12f;
    [SerializeField] float radiusPixels = 200f;
    [SerializeField] bool flipX = true;
    [SerializeField] bool flipY = true;
    [SerializeField] Transform target;
    [SerializeField] bool useUnscaledTime = true;
    [SerializeField] float maxSpeed = Mathf.Infinity;

    SpriteRenderer sr;
    MaterialPropertyBlock mpb;
    Camera cam;
    Vector2 current;
    Vector2 velocity;
    float lagMul = 1f;

    public float RadiusPixels => radiusPixels;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        cam = Camera.main;
        current = new Vector2(center, center);
        Apply(current);
    }

    void Update()
    {
        if (!sr || !cam || !target) return;

        Vector2 eyeSS = cam.WorldToScreenPoint(transform.position);
        Vector2 targetSS = cam.WorldToScreenPoint(target.position);
        Vector2 delta = targetSS - eyeSS;

        if (flipX) delta.x = -delta.x;
        if (flipY) delta.y = -delta.y;

        float m = delta.magnitude;
        if (m > radiusPixels) delta *= radiusPixels / m;

        Vector2 desired = new Vector2(
            center + Mathf.Clamp(delta.x, -radiusPixels, radiusPixels) * (halfRange / radiusPixels),
            center + Mathf.Clamp(delta.y, -radiusPixels, radiusPixels) * (halfRange / radiusPixels)
        );

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        dt = Mathf.Min(dt, 0.05f);

        float st = Mathf.Max(0.0001f, smoothTime * lagMul);
        current = Vector2.SmoothDamp(current, desired, ref velocity, st, maxSpeed, dt);

        Apply(current);
    }

    void Apply(Vector2 uv)
    {
        sr.GetPropertyBlock(mpb);
        mpb.SetVector(propertyName, new Vector4(uv.x, uv.y, 0f, 0f));
        sr.SetPropertyBlock(mpb);
    }

    public void SetFollowTarget(Transform t) => target = t;
    public void SetLagMultiplier(float m) => lagMul = Mathf.Max(0.1f, m);
}
