using UnityEngine;

public sealed class EyeManager : MonoBehaviour
{
    public static EyeManager Instance { get; private set; }

    public enum State { Wander, Follow }

    [SerializeField] EyeFollow eye;
    [SerializeField] Vector2 wanderRetargetInterval = new Vector2(0.8f, 1.8f);
    [SerializeField] float bottomBias = 0.6f;
    [SerializeField] float wanderEdgeClamp = 0.95f;
    [SerializeField] Transform followTarget;

    Transform ghost;
    Vector2 desiredV;
    float nextRetargetAt;
    State state;
    [SerializeField] Camera cam;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        ghost = new GameObject("EyeGhost").transform;
        ghost.hideFlags = HideFlags.HideInHierarchy;

        EnterWander();

        if (followTarget) SetTarget(followTarget);
        else eye.SetFollowTarget(ghost);
    }

    void OnDestroy()
    {
        if (ghost) Destroy(ghost.gameObject);
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (state == State.Follow)
        {
            if (!followTarget) { EnterWander(); return; }
            eye.SetFollowTarget(followTarget);
            return;
        }

        float t = Time.time;
        if (t >= nextRetargetAt)
        {
            desiredV = NextWanderV(bottomBias, wanderEdgeClamp);
            nextRetargetAt = t + Random.Range(wanderRetargetInterval.x, wanderRetargetInterval.y);
        }

        Vector3 eyeWS = eye.transform.position;
        Vector3 eyeSS = cam.WorldToScreenPoint(eyeWS);
        Vector2 deltaSS = desiredV * eye.RadiusPixels;
        Vector3 targetSS = new Vector3(eyeSS.x + deltaSS.x, eyeSS.y + deltaSS.y, eyeSS.z);
        Vector3 targetWS = cam.ScreenToWorldPoint(targetSS);

        ghost.position = targetWS;
        eye.SetFollowTarget(ghost);
    }

    public void SetTarget(Transform t)
    {
        followTarget = t;
        if (!followTarget) { EnterWander(); return; }
        state = State.Follow;
        eye.SetFollowTarget(followTarget);
    }

    public void ClearTarget()
    {
        followTarget = null;
        EnterWander();
    }

    void EnterWander()
    {
        state = State.Wander;
        desiredV = NextWanderV(bottomBias, wanderEdgeClamp);
        nextRetargetAt = Time.time + Random.Range(wanderRetargetInterval.x, wanderRetargetInterval.y);
        eye.SetFollowTarget(ghost);
    }

    static Vector2 NextWanderV(float bottomBias, float edgeClamp)
    {
        Vector2 v;
        do
        {
            v = Random.insideUnitCircle;
            if (Random.value < bottomBias) v.y = -Mathf.Abs(v.y);
            v *= edgeClamp;
        } while (v.sqrMagnitude < 0.01f);
        return v;
    }
}
