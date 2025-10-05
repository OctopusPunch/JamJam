using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

public class Item : MonoBehaviour
{
    float arcHeight;
    float arcDuration;

    Vector3 target = new Vector3(9.044f, -0.328f, 0);

    public SpriteRenderer spriteRenderer;

    public Sprite foodSprite;
    public Sprite waterSprite;
    public Sprite moneySprite;

    private Vector2 originalLocalPosition;

    private Transform originalParent;

    void Start()
    {
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;

    }

    public void SetSprite(NPCAttributes.ResourceType resourceType) 
    {
        if (resourceType == NPCAttributes.ResourceType.Food)
        {
            spriteRenderer.sprite = foodSprite;
        }
        else if (resourceType == NPCAttributes.ResourceType.Water)
        {
            spriteRenderer.sprite = waterSprite;
        }
        else if (resourceType == NPCAttributes.ResourceType.Gold)
        {
            spriteRenderer.sprite = moneySprite;
        }
        else if (resourceType == NPCAttributes.ResourceType.None) 
        {
            spriteRenderer.sprite = null;
        }
    }

    public void ThrowInArc() 
    {
        arcHeight = Random.Range(2.0f, 3.5f);
        arcDuration = Random.Range(0.6f, 0.9f);
        StartCoroutine(ThrowArcCoroutine());
    }

    private System.Collections.IEnumerator ThrowArcCoroutine()
    {
        transform.parent = null;

        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < arcDuration)
        {
            float t = elapsed / arcDuration;
            Vector3 current = Vector3.Lerp(start, target, t);
            current.y += arcHeight * 4 * (t - t * t);
            transform.position = current;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;

        spriteRenderer.sprite = null;
        transform.parent = originalParent;
        transform.localPosition = originalLocalPosition;
        TownResourceBehaviour.Instance.chestAnimator.PlayAnimation();
    }
}
