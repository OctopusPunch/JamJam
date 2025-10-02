using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EyeFollow: MonoBehaviour
{
    [SerializeField] string propertyName = "_EyeOffsetPX";
    [SerializeField] int center = 64;
    [SerializeField] int halfRange = 32;
    [SerializeField] float smoothTime = 0.15f;
    [SerializeField] float radiusPixels = 200f;
    [SerializeField] bool flipX = true;
    [SerializeField] bool flipY = true;
    [SerializeField] bool followTarget = false;
    [SerializeField] Transform target;

    SpriteRenderer sr;
    MaterialPropertyBlock mpb;
    Camera cam;
    Vector2 current;
    Vector2 velocity;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        cam = Camera.main;
        current = new Vector2(center, center);
    }

    void Update()
    {
        if (!sr || !cam) return;

        Vector2 eyeScreen = cam.WorldToScreenPoint(transform.position);
        Vector2 inputScreen;

        if (followTarget && target)
            inputScreen = cam.WorldToScreenPoint(target.position);
        else
            inputScreen = Input.mousePosition;

        Vector2 delta = inputScreen - eyeScreen;

        Vector2 v = delta / Mathf.Max(1f, radiusPixels);
        if (flipX) v.x = -v.x;
        if (flipY) v.y = -v.y;
        v = Vector2.ClampMagnitude(v, 1f);

        Vector2 targetPx = new Vector2(center, center) + v * halfRange;
        current = Vector2.SmoothDamp(current, targetPx, ref velocity, smoothTime);

        sr.GetPropertyBlock(mpb);
        mpb.SetVector(propertyName, new Vector4(current.x, current.y, 0f, 0f));
        sr.SetPropertyBlock(mpb);
    }
}
