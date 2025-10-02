using UnityEngine;
using System.Collections.Generic;

public class Lane : MonoBehaviour
{
    public List<LaneGrid> grid = new List<LaneGrid>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SetupNextAndPreviousGrid();
    }

    void SetupNextAndPreviousGrid()
    {
        for (int i = 0; i < grid.Count; i++)
        {
            if (i > 0)
            {
                grid[i].SetPreviousGrid(grid[i - 1]);
            }

            if (i < grid.Count - 1)
            {
                grid[i].SetNextGrid(grid[i + 1]);
            }
        }
    }
}
