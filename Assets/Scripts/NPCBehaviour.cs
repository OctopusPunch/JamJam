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
    private TMP_Text resourceCountText;

    [SerializeField]
    private LaneGrid currentGrid;
    [SerializeField]
    private LaneGrid targetGrid;

    private Vector3 previousPosition;
    private bool wasGodHanded = false;
    private bool perfectRunSpeed = false;
    private bool inFeedingRange = false;

    private float waitToStartMoving;

    public List<GameObject> resources;

    private void OnEnable()
    {
        perfectRunSpeed = false;
        wasGodHanded = false;
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

        foreach(var resource in resources)
        {
            if(resource == null)
            {
                continue;
            }

            resource.GetComponent<Item>().ThrowInArc();
        }

        resources.Clear();

        AdjustResources();


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
                SoundManager.Instance.Play("Collect_Food");
                break;
            case NPCAttributes.ResourceType.Water:
                TownResourceBehaviour.Instance.AdjustWaterResource(attributes.resourceAmount1);
                SoundManager.Instance.Play("Collect_Water");
                break;
            case NPCAttributes.ResourceType.Gold:
                TownResourceBehaviour.Instance.AdjustGoldMeter(attributes.resourceAmount1);
                SoundManager.Instance.Play("Collect_Gold");
                break;
            default:
            case NPCAttributes.ResourceType.None:
                break;
        }

        switch(attributes.resourceType2)
        {
            case NPCAttributes.ResourceType.Food:
                TownResourceBehaviour.Instance.AdjustFoodResource(attributes.resourceAmount2);
                SoundManager.Instance.Play("Collect_Food");
                break;
            case NPCAttributes.ResourceType.Water:
                TownResourceBehaviour.Instance.AdjustWaterResource(attributes.resourceAmount2);
                SoundManager.Instance.Play("Collect_Water");
                break;
            case NPCAttributes.ResourceType.Gold:
                TownResourceBehaviour.Instance.AdjustGoldMeter(attributes.resourceAmount2);
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

        previousPosition = transform.position;
        GameManager.Instance.SetGodHandActive(true);
        EyeAnimationController.Instance.TriggerGrab();
        SoundManager.Instance.Play("NPC_Grab");
        SoundManager.Instance.Play("HeartBeat");
        EyeManager.Instance.SetTarget(transform);
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
            gameObject.SetActive(false);
            wasGodHanded = false;
            inFeedingRange = false;
            SoundManager.Instance.Play("Eat");
            EyeAnimationController.Instance.Eat();
            GameManager.Instance.WaveManager.RemoveIfTracked(this.gameObject);
            GameManager.Instance.WaveManager.CheckTrackedAllMatchTargets();
        }
        else
        {
            SoundManager.Instance.Play("NPC_Drop");
            EyeAnimationController.Instance.Close();
            transform.position = previousPosition;
        }

        SoundManager.Instance.Stop("HeartBeat");
        GameManager.Instance.SetGodHandActive(false);
        EyeManager.Instance.ClearTarget();

        SetIsHeld(false);
        
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
            HideResource();
            return;
        }
        if (currentGrid == null)
        {
            HideResource();
            return;
        }
        if(currentGrid.GridState == LaneGrid.State.Safe)
        {
            HideResource();
            return;
        }
        if(currentGrid.GridState == LaneGrid.State.NonSelectable)
        {
            HideResource();
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
        if(attributes == null)
        {
            return;
        }
        resourceCountText.text = attributes.GetResourceInfo();
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
