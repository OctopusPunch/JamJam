using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public NPCAttributes Attributes => attributes;
    public bool IsHighlighted => isHighlighted;

    [SerializeField]
    NPCAttributes attributes;

    [SerializeField]
    bool isHighlighted;

    void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector3 pos = transform.position;
        pos.x += attributes.movementSpeed * Time.deltaTime;
        transform.position = pos;
    }

    public void SetHighlighted(bool value)
    {
        isHighlighted = value;
    }

    public void SetAttribute(NPCAttributes attributes)
    {
        this.attributes = attributes;
    }
}
