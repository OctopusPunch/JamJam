using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class RenderTextureToSprite : MonoBehaviour
{
    public RenderTexture source;
    public SpriteRenderer targetRenderer;
    public Image targetImage;
    public bool continuous;
    public int pixelsPerUnit = 100;
    public FilterMode spriteFilterMode = FilterMode.Point;

    Texture2D tex;
    Sprite sprite;
    AsyncGPUReadbackRequest request;
    bool pending;

    void OnEnable() { SyncOnce(); }
    void OnDisable() { Cleanup(); }
    void OnDestroy() { Cleanup(); }
    void Update() { if (continuous) SyncOnce(); }

    void Cleanup()
    {
        if (sprite != null)
        {
            if (targetRenderer && targetRenderer.sprite == sprite) targetRenderer.sprite = null;
            if (targetImage && targetImage.sprite == sprite) targetImage.sprite = null;
            DestroyImmediate(sprite);
            sprite = null;
        }
        if (tex != null)
        {
            DestroyImmediate(tex);
            tex = null;
        }
        if (pending && request.done == false) request.WaitForCompletion();
        pending = false;
    }

    static GraphicsFormat TargetFormat(GraphicsFormat src)
    {
        var wanted = QualitySettings.activeColorSpace == ColorSpace.Linear
            ? GraphicsFormat.R8G8B8A8_UNorm
            : GraphicsFormat.R8G8B8A8_SRGB;
        return SystemInfo.IsFormatSupported(src, GraphicsFormatUsage.Sample) &&
               SystemInfo.IsFormatSupported(src, GraphicsFormatUsage.Linear) &&
               SystemInfo.IsFormatSupported(src, GraphicsFormatUsage.Render)
            ? src
            : wanted;
    }

    void EnsureTex()
    {
        if (source == null || source.width <= 0 || source.height <= 0) return;

        var fmt = TargetFormat(source.graphicsFormat);
        var needNew = tex == null || tex.width != source.width || tex.height != source.height || tex.graphicsFormat != fmt;

        if (needNew)
        {
            if (tex != null) DestroyImmediate(tex);
            tex = new Texture2D(source.width, source.height, fmt, TextureCreationFlags.None);
            tex.filterMode = spriteFilterMode;
        }

        if (sprite == null || sprite.texture != tex)
        {
            if (sprite != null) DestroyImmediate(sprite);
            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        if (targetRenderer && targetRenderer.sprite != sprite) targetRenderer.sprite = sprite;
        if (targetImage && targetImage.sprite != sprite) targetImage.sprite = sprite;
    }

    public void SyncOnce()
    {
        if (source == null) return;
        EnsureTex();

        if (!SystemInfo.supportsAsyncGPUReadback) { SyncCPU(); return; }
        if (pending) { if (!request.done) return; ApplyRequest(); }

        var srcFmt = TargetFormat(source.graphicsFormat);
        if (!SystemInfo.IsFormatSupported(srcFmt, GraphicsFormatUsage.Sample))
        {
            SyncCPU();
            return;
        }

        pending = true;
        request = AsyncGPUReadback.Request(source, 0, OnReadback);
    }

    void OnReadback(AsyncGPUReadbackRequest req)
    {
        if (req.hasError) { pending = false; SyncCPU(); return; }
        request = req;
        ApplyRequest();
    }

    void ApplyRequest()
    {
        if (!pending || request.done == false || request.hasError || tex == null) return;
        var data = request.GetData<byte>();
        if (!data.IsCreated) { pending = false; return; }
        tex.LoadRawTextureData(data);
        tex.Apply(false, false);
        pending = false;
    }

    void SyncCPU()
    {
        if (tex == null || source == null) return;

        var needsConvert = source.graphicsFormat != tex.graphicsFormat;
        RenderTexture blitRT = null;

        if (needsConvert)
        {
            blitRT = RenderTexture.GetTemporary(source.width, source.height, 0, tex.graphicsFormat);
            Graphics.Blit(source, blitRT);
        }

        var src = needsConvert ? blitRT : source;
        var prev = RenderTexture.active;
        RenderTexture.active = src;

        var rect = new Rect(0, 0, src.width, src.height);
        tex.ReadPixels(rect, 0, 0, false);
        tex.Apply(false, false);

        RenderTexture.active = prev;
        if (blitRT) RenderTexture.ReleaseTemporary(blitRT);
    }
}
