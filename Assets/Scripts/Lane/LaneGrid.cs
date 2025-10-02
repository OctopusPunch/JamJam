using System.Collections.Generic;
using UnityEngine;

public class LaneGrid : MonoBehaviour
{
    public LaneGrid NextGrid => nextGrid;
    public LaneGrid PreviousGrid => previousGrid;
    public State GridState => gridState;
    public enum State
    {
        Passable,
        Blocked,
        Destroyed,
        NonSelectable,
        Safe
    }

    [SerializeField]
    private State gridState = State.Passable;

    HashSet<NPCBehaviour> containingNPCs = new HashSet<NPCBehaviour>();

    private LaneGrid nextGrid;
    private LaneGrid previousGrid;

    public void AddContainingNPC(NPCBehaviour containingNPC)
    {
        if(containingNPCs.Contains(containingNPC))
        {
            return;
        }
        containingNPCs.Add(containingNPC);
    }

    public void RemoveContainingNPC(NPCBehaviour containingNPC)
    {
        if (!containingNPCs.Contains(containingNPC))
        {
            return;
        }
        containingNPCs.Remove(containingNPC);
    }

    public void SetBlocked()
    {
        gridState = State.Blocked;
    }

    public void SetDestroyed()
    {
        gridState = State.Destroyed;
    }

    public void SetPassable()
    {
        gridState = State.Passable;
    }

    public void SetPreviousGrid(LaneGrid previousGrid)
    {
        this.previousGrid = previousGrid;
    }

    public void SetNextGrid(LaneGrid nextGrid)
    {
        this.nextGrid = nextGrid;
    }
}
