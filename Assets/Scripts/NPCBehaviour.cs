using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public NPCAttributes Attributes => attributes;
    public bool IsTarget => isTarget;

    [SerializeField]
    NPCAttributes attributes;

    [SerializeField]
    bool isTarget;

    [SerializeField]
    private GameObject eyeSprite;

    [SerializeField]
    private GameObject resourceCount;

    private void Start()
    {
        isTarget = false;
        eyeSprite.SetActive(isTarget);
    }

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

    public void SetAsTarget(bool value)
    {
        isTarget = value;
        eyeSprite.SetActive(value);
    }

    public void SetAttribute(NPCAttributes attributes)
    {
        this.attributes = attributes;
    }
}
