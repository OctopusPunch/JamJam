using UnityEngine;

public class ParallaxMouseSimple : MonoBehaviour
{
    public Transform[] layers;
    public float[] distances;
    public float strength = 0.5f;
    public float speed = 10f;
    public bool flipX;
    public bool flipY;

    Vector3[] basePos;
    Vector2[] offs;

    void Start()
    {
        CaptureBase();
    }

    public void CaptureBase()
    {
        int n = layers != null ? layers.Length : 0;
        basePos = new Vector3[n];
        offs = new Vector2[n];
        for (int i = 0; i < n; i++)
            basePos[i] = layers[i] ? layers[i].localPosition : Vector3.zero;
    }

    void OnDisable()
    {
        if (layers == null || basePos == null) return;
        int n = Mathf.Min(layers.Length, basePos.Length);
        for (int i = 0; i < n; i++)
            if (layers[i]) layers[i].localPosition = basePos[i];
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (layers == null || distances == null) return;
        int n = Mathf.Min(layers.Length, distances.Length);
        if (n == 0) return;
        if (basePos == null || basePos.Length != n) CaptureBase();
        if (offs == null || offs.Length != n) offs = new Vector2[n];

        float nx = (Input.mousePosition.x / Mathf.Max(1f, Screen.width) - 0.5f) * 2f;
        float ny = (Input.mousePosition.y / Mathf.Max(1f, Screen.height) - 0.5f) * 2f;
        nx = Mathf.Clamp(nx, -1f, 1f);
        ny = Mathf.Clamp(ny, -1f, 1f);
        if (flipX) nx = -nx;
        if (flipY) ny = -ny;

        Vector2 dir = new Vector2(nx, ny);
        float t = speed <= 0f ? 1f : 1f - Mathf.Exp(-speed * Time.deltaTime);

        for (int i = 0; i < n; i++)
        {
            var tr = layers[i];
            if (!tr) continue;

            float d = distances[i] <= 0f ? 1f : distances[i];
            Vector2 target = dir * (strength / d);
            offs[i] = Vector2.Lerp(offs[i], target, t);

            Vector3 p = basePos[i];
            p.x += offs[i].x;
            p.y += offs[i].y;
            tr.localPosition = p;
        }
    }
}
