using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private bool playSound;
    [SerializeField] private bool offsetSound = false;
    [SerializeField] private string clipName;
    [SerializeField] private float fps = 10f;
    [SerializeField] private float startDelayMax = 0.5f;

    SpriteRenderer sr;
    int index;
    float timer;
    float startDelay;
    bool started;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startDelay = Random.Range(0f, startDelayMax);
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        // Wait out the random start delay first
        if (!started)
        {
            startDelay -= Time.deltaTime;
            if (startDelay > 0f) return;
            started = true;
            timer = 0f;
        }

        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer = 0f;
            index = (index + 1) % frames.Length;
            sr.sprite = frames[index];
            offsetSound = !offsetSound;

            if (offsetSound && playSound)
                SoundManager.Instance.Play(clipName);
        }
    }
}
