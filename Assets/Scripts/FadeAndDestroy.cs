using UnityEngine;

public class FadeAndDestroy : MonoBehaviour
{

    SpriteRenderer spriteRenderer;
    float fadeDuration = 60f;
    float fadeTimer = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        fadeTimer  += Time.deltaTime;

        float alpha = Mathf.Lerp(1.0f, 0.0f, fadeTimer / fadeDuration);

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

        if(fadeTimer >= fadeDuration) 
        {
            Destroy(gameObject);
        }   
    }
}
