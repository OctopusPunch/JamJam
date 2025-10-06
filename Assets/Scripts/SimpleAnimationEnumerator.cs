using UnityEngine;

public class SimpleAnimationEnumerator : MonoBehaviour
{
    SpriteRenderer sr;

    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 10f;

    private Coroutine animationCoroutine;

    private void Awake()
    {
       sr = GetComponent<SpriteRenderer>();
    }
    public void PlayAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(PlayAnimationCoroutine());
    }

    private System.Collections.IEnumerator PlayAnimationCoroutine()
    {
        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];

            yield return new WaitForSeconds(1f / fps);
        }

        if (frames != null && frames.Length > 0)
            sr.sprite = frames[0];

        animationCoroutine = null;
    }
}
