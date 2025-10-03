using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimation : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 10f;

    SpriteRenderer sr;
    int index;
    float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer = 0f;
            index = (index + 1) % frames.Length;
            sr.sprite = frames[index];
        }
    }
}
