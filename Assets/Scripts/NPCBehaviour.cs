using System.Collections.Generic;
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
    private LaneGrid currentGrid;
    [SerializeField]
    private LaneGrid targetGrid;

    private Vector3 previousPosition;
    private bool wasGodHanded = false;
    private bool perfectRunSpeed = false;
    private bool inFeedingRange = false;

    private float waitToStartMoving;

    public Item item1;
    public Item item2;

    public GameObject ResourceCounterDisplay;
    public Transform itemOneResourceDisplay;
    [SerializeField]
    private TMP_Text resourceOneCountText;
    public Transform itemTwoResourceDisplay;
    [SerializeField]
    private TMP_Text resourceTwoCountText;

    [SerializeField]
    private Sprite food;
    [SerializeField]
    private Sprite water;
    [SerializeField]
    private Sprite gold;

    private float posYResource1 = .17f;
    private float posYResource2 = -.08f;

    private void OnEnable()
    {
        perfectRunSpeed = false;
        wasGodHanded = false;
        isHeld = false;
        SetResource();
        HideResource();
        GetComponent<SimpleSpriteAnimator>().SetAnimation("Walk");
    }

    private void OnDisable()
    {
        wasGodHanded = false;
        inFeedingRange = false;
        if (!DeckManager.clearingList)
            GameManager.Instance.WaveManager.RemoveIfTracked(this.gameObject);
    }

    void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        if (GameManager.Instance.State != GameManager.GameState.In_Game && !perfectRunSpeed)
        {
            return;
        }

        waitToStartMoving -= Time.deltaTime;
        if(waitToStartMoving > 0)
        {
            return;
        }
        MoveToLaneGrid(); 
        UpdateCurrentGrid();
    }

    public void PerfectRun()
    {
        perfectRunSpeed = true;
    }

    private void MoveToLaneGrid()
    {

        if (IsHeld)
        {
            return;
        }

        Vector3 pos = transform.position;
        pos.x += attributes.movementSpeed * (perfectRunSpeed ? 7 : (wasGodHanded ? 2.3f : 1)) * Time.deltaTime;

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

        if (item1.spriteRenderer.sprite != null) 
        { 
            item1.ThrowInArc();
        }

        if (item2.spriteRenderer.sprite != null) 
        { 
            item2.ThrowInArc();
        }

        CollectResources();

        gameObject.SetActive(false);
        wasGodHanded = false;
        inFeedingRange = false;
        GameManager.Instance.WaveManager.RemoveIfTracked(this.gameObject);
    }

    public void SetIsHeld(bool value)
    {
        isHeld = value;
    }

    public void AdjustResources()
    {
        switch(attributes.resourceType1)
        {
            case NPCAttributes.ResourceType.Food:
                TownResourceBehaviour.Instance.AdjustFoodResource(attributes.resourceAmount1);
                break;
            case NPCAttributes.ResourceType.Water:
                TownResourceBehaviour.Instance.AdjustWaterResource(attributes.resourceAmount1);
                break;
            case NPCAttributes.ResourceType.Gold:
                TownResourceBehaviour.Instance.AdjustGoldMeter(attributes.resourceAmount1);
                break;
            default:
            case NPCAttributes.ResourceType.None:
                break;
        }

        switch(attributes.resourceType2)
        {
            case NPCAttributes.ResourceType.Food:
                TownResourceBehaviour.Instance.AdjustFoodResource(attributes.resourceAmount2);
                break;
            case NPCAttributes.ResourceType.Water:
                TownResourceBehaviour.Instance.AdjustWaterResource(attributes.resourceAmount2);
                break;
            case NPCAttributes.ResourceType.Gold:
                TownResourceBehaviour.Instance.AdjustGoldMeter(attributes.resourceAmount2);
                break;
            default:
            case NPCAttributes.ResourceType.None:
                break;
        }
    }
    public void CollectResources()
    {
        switch (attributes.resourceType1)
        {
            case NPCAttributes.ResourceType.Food:
                SoundManager.Instance.Play("Collect_Food");
                break;
            case NPCAttributes.ResourceType.Water:
                SoundManager.Instance.Play("Collect_Water");
                break;
            case NPCAttributes.ResourceType.Gold:
                SoundManager.Instance.Play("Collect_Gold");
                break;
            default:
            case NPCAttributes.ResourceType.None:
                break;
        }

        switch (attributes.resourceType2)
        {
            case NPCAttributes.ResourceType.Food:
                SoundManager.Instance.Play("Collect_Food");
                break;
            case NPCAttributes.ResourceType.Water:
                SoundManager.Instance.Play("Collect_Water");
                break;
            case NPCAttributes.ResourceType.Gold:
                SoundManager.Instance.Play("Collect_Gold");
                break;
            default:
            case NPCAttributes.ResourceType.None:
                break;
        }
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
        if(perfectRunSpeed)
        {
            return;
        }

        CursorManager.Instance.SetGrab();
        previousPosition = transform.position;
        GameManager.Instance.SetGodHandActive(true);
        EyeAnimationController.Instance.TriggerGrab();
        SoundManager.Instance.Play("NPC_Grab");
        SoundManager.Instance.Play("HeartBeat");
        EyeManager.Instance.SetTarget(transform);
        SetIsHeld(true);
        GetComponent<SimpleSpriteAnimator>().SetAnimation("Wiggle"); 
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
            gameObject.SetActive(false);
            wasGodHanded = false;
            inFeedingRange = false;
            SoundManager.Instance.Play("Eat");
            EyeAnimationController.Instance.Eat();
            AdjustResources();
            GameManager.Instance.WaveManager.RemoveIfTracked(this.gameObject);
            TownResourceBehaviour.Instance.CheckWinOrFail();
        }
        else
        {
            SoundManager.Instance.Play("NPC_Drop");
            EyeAnimationController.Instance.Close();
            transform.position = previousPosition;
        }

        CursorManager.Instance.SetNormal();
        SoundManager.Instance.Stop("HeartBeat");
        GameManager.Instance.SetGodHandActive(false);
        EyeManager.Instance.ClearTarget();

        SetIsHeld(false);
        GetComponent<SimpleSpriteAnimator>().SetAnimation("Walk");
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
        if (perfectRunSpeed)
        {
            CursorManager.Instance.SetNormal();
            HideResource();
            return;
        }
        if (currentGrid == null)
        {
            CursorManager.Instance.SetNormal();
            HideResource();
            return;
        }
        if(currentGrid.GridState == LaneGrid.State.Safe)
        {
            CursorManager.Instance.SetNormal();
            HideResource();
            return;
        }
        if(currentGrid.GridState == LaneGrid.State.NonSelectable)
        {
            CursorManager.Instance.SetNormal();
            HideResource();
            return;
        }
        ShowReseource();
        CursorManager.Instance.SetHover();
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
        CursorManager.Instance.SetNormal();
        HideResource();
    }

    void SetResource()
    {
        if(attributes == null)
        {
            return;
        }
        resourceOneCountText.text = "x " + attributes.resourceAmount1;
        resourceTwoCountText.text = "x " + attributes.resourceAmount2;

        if(attributes.resourceType2 != NPCAttributes.ResourceType.None)
        {
            Vector3 r_1 = itemOneResourceDisplay.transform.localPosition;
            r_1.y = posYResource1;
            itemOneResourceDisplay.transform.localPosition = r_1;
            itemOneResourceDisplay.GetComponent<SpriteRenderer>().sprite = SetSprite(attributes.resourceType1);

            Vector3 r_2 = itemTwoResourceDisplay.transform.localPosition;
            r_2.y = posYResource2;
            itemTwoResourceDisplay.transform.localPosition = r_2;
            itemTwoResourceDisplay.gameObject.SetActive(true);

            itemTwoResourceDisplay.GetComponent<SpriteRenderer>().sprite = SetSprite(attributes.resourceType2);
            return;
        }
        Vector3 r1 = itemOneResourceDisplay.transform.localPosition;
        r1.y = 0;
        itemOneResourceDisplay.transform.localPosition = r1;
        itemOneResourceDisplay.GetComponent<SpriteRenderer>().sprite = SetSprite(attributes.resourceType1);

        itemTwoResourceDisplay.gameObject.SetActive(false);

    }

    Sprite SetSprite(NPCAttributes.ResourceType resourceType)
    {
        switch (resourceType)
        {
            case NPCAttributes.ResourceType.Food:
                return food;
            case NPCAttributes.ResourceType.Gold:
                return gold;
            case NPCAttributes.ResourceType.Water:
                return water;
            case NPCAttributes.ResourceType.None:
                break;
        }
        return null;
    }

    void ShowReseource()
    {
        ResourceCounterDisplay.SetActive(true);
    }

    void HideResource()
    {
        ResourceCounterDisplay.gameObject.SetActive(false);
    }

    public void SetTargetGrid(LaneGrid laneGrid)
    {
        targetGrid = laneGrid;
    }
    public void SetCurrentGrid(LaneGrid laneGrid)
    {
        currentGrid = laneGrid;
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

    public void SetWaitToStartMovement(float waitToStartMovement)
    {
        waitToStartMoving = waitToStartMovement;
    }
}
