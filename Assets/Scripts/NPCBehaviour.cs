using TMPro;
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
    private TMP_Text resourceCountText;



    private void Start()
    {
        isTarget = false;
        eyeSprite.SetActive(isTarget);
        SetResource();
        HideResource();
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
        SetResource();
    }

    private void OnMouseDown()
    {
        SetAsTarget(!IsTarget);

        // push to some eye watch list
    }

    private void OnMouseEnter()
    {
        ShowReseource();
    }

    private void OnMouseExit()
    {
        HideResource();
    }

    void SetResource()
    {
        resourceCountText.text = "Water: " + attributes.waterResource.ToString();
        resourceCountText.text += "<br>Food: " + attributes.foodResource.ToString();
    }

    void ShowReseource()
    {
        resourceCountText.gameObject.SetActive(true);
    }

    void HideResource()
    {
        resourceCountText.gameObject.SetActive(false);
    }
}
