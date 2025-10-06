using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimator : MonoBehaviour
{
    [System.Serializable]
    public class SpriteAnimation
    {
        public string name;
        public Sprite[] frames;
        public float fps = 10f;
    }

    [SerializeField] private SpriteAnimation[] animations;
    [SerializeField] private bool playSound;
    [SerializeField] private string clipName;
    [SerializeField] private float startDelayMax = 0.5f;

    private SpriteRenderer sr;
    private int currentAnimIndex;
    private int frameIndex;
    private float timer;
    private float startDelay;
    private bool started;
    private bool offsetSound;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startDelay = Random.Range(0f, startDelayMax);
    }

    void Update()
    {
        if (animations == null || animations.Length == 0) return;
        var anim = animations[currentAnimIndex];
        if (anim.frames == null || anim.frames.Length == 0) return;

        // Randomized initial delay
        if (!started)
        {
            startDelay -= Time.deltaTime;
            if (startDelay > 0f) return;
            started = true;
            timer = 0f;
        }

        timer += Time.deltaTime;
        if (timer >= 1f / anim.fps)
        {
            timer = 0f;
            frameIndex = (frameIndex + 1) % anim.frames.Length;
            sr.sprite = anim.frames[frameIndex];

            offsetSound = !offsetSound;
            if (offsetSound && playSound)
                SoundManager.Instance.Play(clipName);
        }
    }

    public void SetAnimation(int index)
    {
        if (index < 0 || index >= animations.Length) return;
        if (index == currentAnimIndex) return;

        currentAnimIndex = index;
        frameIndex = 0;
        timer = 0f;
        started = true;
        sr.sprite = animations[index].frames.Length > 0 ? animations[index].frames[0] : null;
    }

    public void SetAnimation(string name)
    {
        for (int i = 0; i < animations.Length; i++)
        {
            if (animations[i].name == name)
            {
                SetAnimation(i);
                return;
            }
        }
    }

    public int GetCurrentAnimationIndex() => currentAnimIndex;
    public string GetCurrentAnimationName() => animations[currentAnimIndex].name;
}