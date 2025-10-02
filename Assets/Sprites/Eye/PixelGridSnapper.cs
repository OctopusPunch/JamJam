using UnityEngine;

[ExecuteAlways]
public class PixelGridSnapper : MonoBehaviour
{
    public Camera cam;
    public int pixelsPerUnit = 64;   // your eye art is 64×64, 1 unit wide -> PPU 64
    public int zoom = 1;             // 1,2,3… integer screen scale

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (!cam.orthographic) return;

        float worldUnitsPerScreenPixel = (cam.orthographicSize * 2f) / (Screen.height / (float)zoom);
        Vector3 p = transform.position;
        p.x = Mathf.Round(p.x / worldUnitsPerScreenPixel) * worldUnitsPerScreenPixel;
        p.y = Mathf.Round(p.y / worldUnitsPerScreenPixel) * worldUnitsPerScreenPixel;
        transform.position = p;

        Vector3 s = transform.localScale;
        float snap = 1f / (float)pixelsPerUnit;
        s.x = Mathf.Round(s.x / snap) * snap;
        s.y = Mathf.Round(s.y / snap) * snap;
        transform.localScale = s;
    }
}
