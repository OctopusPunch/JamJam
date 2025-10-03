using TMPro;
using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public NPCAttributes Attributes => attributes;
    public bool IsHeld => isHeld;

    [SerializeField]
    NPCAttributes attributes;

    [SerializeField]
    bool isHeld;

    [SerializeField]
    private TMP_Text resourceCountText;

    [SerializeField]
    private LaneGrid currentGrid;
    [SerializeField]
    private LaneGrid targetGrid;

    private Vector3 previousPosition;
    private bool wasGodHanded = false;
    private bool inFeedingRange = false;

    private void Start()
    {
        isHeld = false;
        SetResource();
        HideResource();
    }

    void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        if (GameManager.Instance.State != GameManager.GameState.In_Game)
        {
            return;
        }
        MoveToLaneGrid(); 
        UpdateCurrentGrid();
    }

    private void MoveToLaneGrid()
    {

        if (IsHeld)
        {
            return;
        }

        Vector3 pos = transform.position;
        pos.x += attributes.movementSpeed * (wasGodHanded ? 1.5f : 1) * Time.deltaTime;

        if(pos.x < targetGrid.transform.position.x)
        {
            if (targetGrid.GridState == LaneGrid.State.Blocked || targetGrid.GridState == LaneGrid.State.Destroyed)
            {
                return;
            }
            transform.position = pos;
            return;
        }

        if(targetGrid.NextGrid == null)
        {
            // Update Resources here and despawn NPC
            return;
        }

        targetGrid = targetGrid.NextGrid;

        if(targetGrid.GridState == LaneGrid.State.Blocked || targetGrid.GridState == LaneGrid.State.Destroyed)
        {
            return;
        }
        transform.position = pos;
    }

    public void UpdateCurrentGrid()
    {
        if(transform.position.x < targetGrid.transform.position.x - .5f)
        {
            return;
        }

        if(currentGrid != null && currentGrid.GridState == LaneGrid.State.Safe)
        {
            return;
        }

        currentGrid = targetGrid;


        if (currentGrid.GridState != LaneGrid.State.Safe)
        {
            return;
        }
        TownResourceBehaviour.Instance.AddFoodResource(attributes.foodResource);
        TownResourceBehaviour.Instance.AddHappinessResource(wasGodHanded ? -2 : ((attributes.foodResource > 1) ? (attributes.foodResource - 1) * 1.35f : 1));
        gameObject.SetActive(false);
        wasGodHanded = false;
        inFeedingRange = false;
        GameManager.Instance.WaveManager.RemoveIfTracked(this.gameObject);
    }

    public void SetIsHeld(bool value)
    {
        isHeld = value;
    }

    public void SetAttribute(NPCAttributes attributes)
    {
        this.attributes = attributes;
        SetResource();
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        if (GameManager.Instance.State != GameManager.GameState.In_Game)
        {
            return;
        }
        if(currentGrid == null)
        {
            return;
        }
        if (currentGrid.GridState == LaneGrid.State.NonSelectable)
        {
            return;
        }
        if (currentGrid.GridState == LaneGrid.State.Safe)
        {
            return;
        }

        previousPosition = transform.position;
        GameManager.Instance.SetGodHandActive(true);
        SetIsHeld(true);
        wasGodHanded = true;
    }
    private void OnMouseUp()
    {
        if (!IsHeld)
        {
            return;
        }

        if (inFeedingRange)
        {
            ScreenShake.Instance.TriggerShake(.2f, .35f);
            TownResourceBehaviour.Instance.AddToHungerMeter(attributes.foodResource + Random.Range(1, 3));
            TownResourceBehaviour.Instance.UseHappinessResource((attributes.foodResource - 1) * 1.55f);
            gameObject.SetActive(false);
            wasGodHanded = false;
            inFeedingRange = false;
            GameManager.Instance.WaveManager.RemoveIfTracked(this.gameObject);
        }
        GameManager.Instance.SetGodHandActive(false);
        SetIsHeld(false);
        transform.position = previousPosition;
    }

    private void OnMouseDrag()
    {
        if (!IsHeld)
        {
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mouseWorldPos.z = 0f;
        transform.position = mouseWorldPos;
    }

    private void OnMouseEnter()
    {
        if(GameManager.Instance == null)
        {
            return;
        }
        if (GameManager.Instance.State != GameManager.GameState.In_Game)
        {
            return;
        }
        if (GameManager.Instance.GodHandActive)
        {
            return;
        }
        ShowReseource();
    }

    private void OnMouseExit()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        if (GameManager.Instance.State != GameManager.GameState.In_Game)
        {
            return;
        }
        if (GameManager.Instance.GodHandActive)
        {
            return;
        }
        HideResource();
    }

    void SetResource()
    {
        resourceCountText.text = "Food: " + attributes.foodResource.ToString();
    }

    void ShowReseource()
    {
        resourceCountText.gameObject.SetActive(true);
    }

    void HideResource()
    {
        resourceCountText.gameObject.SetActive(false);
    }

    public void SetTargetGrid(LaneGrid laneGrid)
    {
        targetGrid = laneGrid;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Eye"))
        {
            return;
        }
        inFeedingRange = true;
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if(!collision.CompareTag("Eye"))
        {
            return;
        }
        inFeedingRange = false;
    }
}
