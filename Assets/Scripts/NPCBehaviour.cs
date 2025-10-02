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

    [SerializeField]
    private LaneGrid currentGrid;
    [SerializeField]
    private LaneGrid targetGrid;


    private void Start()
    {
        isTarget = false;
        eyeSprite.SetActive(isTarget);
        SetResource();
        HideResource();
    }

    void Update()
    {
        if(GameManager.Instance.State != GameManager.GameState.In_Game)
        {
            return;
        }
        MoveToLaneGrid(); 
        UpdateCurrentGrid();
    }

    private void MoveToLaneGrid()
    {
        Vector3 pos = transform.position;
        pos.x += attributes.movementSpeed * Time.deltaTime;

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

        if(currentGrid.GridState != LaneGrid.State.Safe && currentGrid.GridState != LaneGrid.State.NonSelectable)
        {
            return;
        }

        if(isTarget && currentGrid.GridState == LaneGrid.State.NonSelectable)
        {
            TownResourceBehaviour.Instance.AddToHungerMeter(attributes.foodResource + Random.Range(1, 3));
            gameObject.SetActive(false);
            GameManager.Instance.WaveManager.RemoveIfTracked(this.gameObject);
            return;
        }

        if (currentGrid.GridState != LaneGrid.State.Safe)
        {
            return;
        }
        TownResourceBehaviour.Instance.AddFoodResource(attributes.foodResource);
        
        gameObject.SetActive(false);
        GameManager.Instance.WaveManager.RemoveIfTracked(this.gameObject);
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
        if (GameManager.Instance.State != GameManager.GameState.In_Game)
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
        SetAsTarget(!IsTarget);

        // push to some eye watch list
    }

    private void OnMouseEnter()
    {
        if (GameManager.Instance.State != GameManager.GameState.In_Game)
        {
            return;
        }
        ShowReseource();
    }

    private void OnMouseExit()
    {
        if (GameManager.Instance.State != GameManager.GameState.In_Game)
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
}
