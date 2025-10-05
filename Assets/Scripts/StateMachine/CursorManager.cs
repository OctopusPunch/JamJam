using UnityEngine;

public enum CursorState { Normal, Hover, Grab }

public sealed class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Normal")]
    [SerializeField] Texture2D normalTexture;
    [SerializeField] Vector2 normalHotspot = Vector2.zero;
    [SerializeField] CursorMode normalMode = CursorMode.Auto;

    [Header("Hover")]
    [SerializeField] Texture2D hoverTexture;
    [SerializeField] Vector2 hoverHotspot = Vector2.zero;
    [SerializeField] CursorMode hoverMode = CursorMode.Auto;

    [Header("Grab")]
    [SerializeField] Texture2D grabTexture;
    [SerializeField] Vector2 grabHotspot = Vector2.zero;
    [SerializeField] CursorMode grabMode = CursorMode.Auto;

    [SerializeField] CursorState initialState = CursorState.Normal;

    CursorState state;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetState(initialState, true);
    }

    void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    public CursorState CurrentState => state;

    public void SetNormal() => SetState(CursorState.Normal);
    public void SetHover() => SetState(CursorState.Hover);
    public void SetGrab() => SetState(CursorState.Grab);

    public void SetState(CursorState newState) => SetState(newState, false);

    void SetState(CursorState newState, bool force)
    {
        if (!force && state == newState) return;
        state = newState;

        switch (state)
        {
            case CursorState.Hover:
                Cursor.SetCursor(hoverTexture, hoverHotspot, hoverMode);
                break;
            case CursorState.Grab:
                Cursor.SetCursor(grabTexture, grabHotspot, grabMode);
                break;
            default:
                Cursor.SetCursor(normalTexture, normalHotspot, normalMode);
                break;
        }
    }
}