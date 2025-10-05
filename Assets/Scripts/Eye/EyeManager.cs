using System.Collections;
using UnityEngine;

public sealed class EyeManager : MonoBehaviour
{
    public static EyeManager Instance { get; private set; }

    public enum State { Wander, Follow, CenterHold }

    [Header("References")]
    [SerializeField] EyeFollow eye;
    [SerializeField] EyeBob bob;
    [SerializeField] Transform eyeHolder;
    [SerializeField] Transform followTarget;
    [SerializeField] Camera cam;

    [Header("Wander Settings")]
    [SerializeField] Vector2 wanderRetargetInterval = new Vector2(0.8f, 1.8f);
    [SerializeField] float bottomBias = 0.6f;
    [SerializeField] float wanderEdgeClamp = 0.95f;

    [Header("Lags")]
    [SerializeField] float wanderLag = 1.6f;
    [SerializeField] float followLag = 1.0f;

    [Header("Bob Influence")]
    [SerializeField] float bobTextureLagTime = 0.18f;
    [SerializeField] float bobTextureInfluence = 1.0f;

    [Header("Center Hold")]
    [SerializeField] float centerHoldDuration = 1.5f;

    Transform ghost;
    Vector2 desiredV;
    float nextRetargetAt;
    State state;

    Vector3 bobLagPos;
    Vector3 bobLagVel;
    Coroutine centerHoldRoutine;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (!cam) cam = Camera.main;

        ghost = new GameObject("EyeGhost").transform;
        ghost.hideFlags = HideFlags.HideInHierarchy;

        if (bob)
        {
            bob.enabled = false;
            if (eyeHolder) bob.SetTarget(eyeHolder);
        }

        EnterWander();

        if (followTarget)
            SetTarget(followTarget);
        else
            eye.SetFollowTarget(ghost);
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
            if (!followTarget)
            {
                ClearTarget();
                return;
            }

            eye.SetFollowTarget(followTarget);
            return;
        }

        if (state == State.CenterHold)
        {
            eye.SetFollowTarget(ghost);
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

        if (eyeHolder)
        {
            bobLagPos = Vector3.SmoothDamp(bobLagPos, eyeHolder.position, ref bobLagVel, Mathf.Max(0.0001f, bobTextureLagTime));
            Vector3 lagVec = bobLagPos - eyeHolder.position;
            targetWS += lagVec * bobTextureInfluence;
        }

        ghost.position = targetWS;
        eye.SetFollowTarget(ghost);
    }

    public void SetTarget(Transform t)
    {
        CancelCenterHold();
        followTarget = t;

        if (!followTarget)
        {
            ClearTarget();
            return;
        }

        state = State.Follow;
        if (bob) bob.enabled = false;
        eye.SetLagMultiplier(followLag);
        eye.SetFollowTarget(followTarget);
    }

    public void ClearTarget()
    {
        followTarget = null;

        CancelCenterHold();
        centerHoldRoutine = StartCoroutine(CenterHoldAndWander());
    }

    void EnterWander()
    {
        CancelCenterHold();

        state = State.Wander;
        desiredV = NextWanderV(bottomBias, wanderEdgeClamp);
        nextRetargetAt = Time.time + Random.Range(wanderRetargetInterval.x, wanderRetargetInterval.y);

        if (bob)
        {
            if (eyeHolder) bob.SetTarget(eyeHolder);
            bob.enabled = true;
            bob.ResetBaseToCurrent();
        }

        if (eyeHolder)
        {
            bobLagPos = eyeHolder.position;
            bobLagVel = Vector3.zero;
        }

        eye.SetLagMultiplier(wanderLag);
        eye.SetFollowTarget(ghost);
    }
    IEnumerator CenterHoldAndWander()
    {
        state = State.CenterHold;

        if (cam && eye)
        {
            Vector3 eyeWS = eye.transform.position;
            Vector3 eyeSS = cam.WorldToScreenPoint(eyeWS);
            Vector3 centerSS = new Vector3(eyeSS.x, eyeSS.y, eyeSS.z);
            Vector3 centerWS = cam.ScreenToWorldPoint(centerSS);
            ghost.position = centerWS;
        }

        if (bob) bob.enabled = false;

        eye.SetLagMultiplier(0.1f);
        yield return new WaitForSecondsRealtime(0.15f);

        // Hold it steady
        eye.SetLagMultiplier(0.01f);
        yield return new WaitForSecondsRealtime(centerHoldDuration);

        if (state == State.CenterHold)
            EnterWander();

        centerHoldRoutine = null;
    }

    void CancelCenterHold()
    {
        if (centerHoldRoutine != null)
        {
            StopCoroutine(centerHoldRoutine);
            centerHoldRoutine = null;
        }
    }

    static Vector2 NextWanderV(float bottomBias, float edgeClamp)
    {
        Vector2 v;
        do
        {
            v = Random.insideUnitCircle;
            if (Random.value < bottomBias)
                v.y = -Mathf.Abs(v.y);
            v *= edgeClamp;
        } while (v.sqrMagnitude < 0.01f);
        return v;
    }
}
