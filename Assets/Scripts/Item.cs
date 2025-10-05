using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

public class Item : MonoBehaviour
{
    float arcHeight;
    float arcDuration;

    Vector3 target = new Vector3(8.75f, -0.75f, 0);

    void Start()
    {
        arcHeight = Random.Range(2.0f, 3.5f);
        arcDuration = Random.Range(0.6f, 0.9f);
    }

    public void ThrowInArc() 
    {
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
        gameObject.SetActive(false);
        GameManager.Instance.WaveManager.RemoveItemIfTracked(this.gameObject);
    }
}
