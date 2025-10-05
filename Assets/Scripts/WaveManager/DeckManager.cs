using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DeckManager
{
    //[SerializeField]
    //public List<NPCAttributes> npcDeck = new List<NPCAttributes>();

    public int currentWave = 0;

    public Dictionary<string, GameObject> trackedNPCs = new Dictionary<string, GameObject>(250);
    public Dictionary<string, GameObject> trackedItems = new Dictionary<string, GameObject>(250);

    public GameObject NpcPrefab;

    public GameObject goldPrefab;
    public GameObject foodPrefab;
    public GameObject waterPrefab;

    public Queue<GameObject> npcPools = new Queue<GameObject>(25);

    public int resourcePoolSize = 20;

    public Queue<GameObject> GoldPool;
    public Queue<GameObject> FoodPool;
    public Queue<GameObject> WaterPool;



    public void Initialise()
    {
        GameObject holder = new GameObject("#NPC Holder");
        MonoBehaviour.DontDestroyOnLoad(holder);
        for (int i = 0; i < 25; i++)
        {
            GameObject newNpc = MonoBehaviour.Instantiate(NpcPrefab);

            newNpc.SetActive(false);
            newNpc.transform.position = Vector3.one * -1000;
            newNpc.transform.SetParent(holder.transform, false);
            newNpc.name = "NPC #" + i;
            npcPools.Enqueue(newNpc);
        }

        GameObject resourceHolder = new GameObject("#ResourceHolder");
        MonoBehaviour.DontDestroyOnLoad(resourceHolder);

        GoldPool = new Queue<GameObject>(resourcePoolSize);
        FoodPool = new Queue<GameObject>(resourcePoolSize);
        WaterPool = new Queue<GameObject>(resourcePoolSize);

        for (int i = 0; i < resourcePoolSize; i++)
        {
            GameObject newGold = MonoBehaviour.Instantiate(goldPrefab);

            newGold.SetActive(false);
            newGold.transform.position = Vector3.one * -1000;
            newGold.transform.SetParent(resourceHolder.transform, false);
            newGold.name = "Gold #" + i;
            GoldPool.Enqueue(newGold);

            GameObject newFood = MonoBehaviour.Instantiate(foodPrefab);

            newFood.SetActive(false);
            newFood.transform.position = Vector3.one * -1000;
            newFood.transform.SetParent(resourceHolder.transform, false);
            newFood.name = "Food #" + i;
            FoodPool.Enqueue(newFood);

            GameObject newWater = MonoBehaviour.Instantiate(waterPrefab);

            newWater.SetActive(false);
            newWater.transform.position = Vector3.one * -1000;
            newWater.transform.SetParent(resourceHolder.transform, false);
            newWater.name = "Water #" + i;
            WaterPool.Enqueue(newWater);
        }
    }

    public void ResetWaves()
    {
        currentWave = 0;
        
        ClearTrackingAndRepool();
    }


    public void ReleaseNewWave()
    {
        ++currentWave;
        List<NPCAttributes> pulledSolution = new List<NPCAttributes>();

        // pull solution from deck
        AddSolution(pulledSolution);
        // added randomness
        AddRandomness(pulledSolution);
        Shuffle(pulledSolution);
        SpawnWave(pulledSolution);
        pulledSolution.Clear();
    }

    public void Shuffle(List<NPCAttributes> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            NPCAttributes value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void AddSolution(List<NPCAttributes> list)
    {
        // Max Solution 5
        if (currentWave < 4)
        {
            GenerateSingleTypeSolution(list);
            return;
        }

        // Max Solution 7
        if(currentWave < 7)
        {
            GenerateDoubleTypeSolution(list);
            return;
        }

        // Max Solution 8
        if(currentWave < 12)
        {
            GenerateSingleTypeSolution(list);
            return;
        }

        GenerateDoubleTypeSolution(list);
        // Max Solution 10

    }
    public void AddRandomness(List<NPCAttributes> list)
    {
        GenerateRandomSingle(list, (NPCAttributes.ResourceType)Random.Range(0, 3));
        GenerateRandomSingle(list, (NPCAttributes.ResourceType)Random.Range(0, 3));
        // Max NPCs 5
        if (currentWave < 4)
        {
            return;
        }

        // Max NPCs 7
        if (currentWave < 7)
        {
            GenerateRandomDual(list, (NPCAttributes.ResourceType)Random.Range(0, 3), (NPCAttributes.ResourceType)Random.Range(0, 3));
            GenerateRandomSingle(list, (NPCAttributes.ResourceType)Random.Range(0, 3));
            return;
        }

        // Max NPCs 8
        if (currentWave < 12)
        {
            GenerateRandomDual(list, (NPCAttributes.ResourceType)Random.Range(0, 3),(NPCAttributes.ResourceType)Random.Range(0, 3));
            GenerateRandomDual(list, (NPCAttributes.ResourceType)Random.Range(0, 3), (NPCAttributes.ResourceType)Random.Range(0, 3));
            return;
        }

        // Max NPCs 10
        GenerateRandomDual(list, (NPCAttributes.ResourceType)Random.Range(0, 3), (NPCAttributes.ResourceType)Random.Range(0, 3));
        GenerateRandomDual(list, (NPCAttributes.ResourceType)Random.Range(0, 3), (NPCAttributes.ResourceType)Random.Range(0, 3));
        GenerateRandomSingle(list, (NPCAttributes.ResourceType)Random.Range(0, 3));
    }

    public void GenerateRandomSingle(List<NPCAttributes> list, NPCAttributes.ResourceType resourceType)
    {
        NPCAttributes npc = ScriptableObject.CreateInstance<NPCAttributes>();
        npc.resourceType1 = resourceType;
        npc.resourceAmount1 = Random.Range(1, 4);

        list.Add(npc);
    }

    public void GenerateRandomDual(List<NPCAttributes> list, NPCAttributes.ResourceType resourceType1, NPCAttributes.ResourceType resourceType2)
    {
        NPCAttributes npc = ScriptableObject.CreateInstance<NPCAttributes>();
        npc.resourceType1 = resourceType1;
        npc.resourceAmount1 = Random.Range(1, 4);

        if(resourceType1 == resourceType2)
        {

            bool increment = Random.Range(0.0f, 1.0f) > .5f;
            int resourceSelection = (int)resourceType2;
            if (increment)
            {
                if (--resourceSelection < 0)
                {
                    resourceSelection = 2;
                }
            }
            else
            {
                if (++resourceSelection > 2)
                {
                    resourceSelection = 0;
                }
            }
            resourceType2 = (NPCAttributes.ResourceType)resourceSelection;
        }

        npc.resourceType2 = resourceType2;
        npc.resourceAmount2 = Random.Range(1, 4);

        list.Add(npc);
    }

    public void GenerateSingleTypeSolution(List<NPCAttributes> list)
    {
        int targetFood = TownResourceBehaviour.Instance.TargetFoodValue;
        int targetGold = TownResourceBehaviour.Instance.TargetGoldValue;
        int targetWater = TownResourceBehaviour.Instance.TargetWaterValue;


        while(targetFood >= 1)
        {
            NPCAttributes npc = ScriptableObject.CreateInstance<NPCAttributes>();
            int val;

            if (targetFood < 3)
            {
                val = targetFood;
            }
            else
            {
                val = Random.Range(3, targetFood + 1);
            }
            npc.resourceType1 = NPCAttributes.ResourceType.Food;
            npc.resourceAmount1 = val;
            targetFood -= val;
            list.Add(npc);
        }

        while(targetWater >= 1)
        {
            NPCAttributes npc = ScriptableObject.CreateInstance<NPCAttributes>();
            int val;

            if (targetWater < 3)
            {
                val = targetWater;
            }
            else
            {
                val = Random.Range(3, targetWater + 1);
            }
            npc.resourceType1 = NPCAttributes.ResourceType.Water;
            npc.resourceAmount1 = val;
            targetWater -= val;
            list.Add(npc);
        }

        while(targetGold >= 1)
        {
            NPCAttributes npc = ScriptableObject.CreateInstance<NPCAttributes>();
            int val;

            if (targetGold < 3)
            {
                val = targetGold;
            }
            else
            {
                val = Random.Range(3, targetGold + 1);
            }
            npc.resourceType1 = NPCAttributes.ResourceType.Gold;
            npc.resourceAmount1 = val;
            targetGold -= val;
            list.Add(npc);
        }
    }
    public void GenerateDoubleTypeSolution(List<NPCAttributes> list)
    {
        int targetFood = TownResourceBehaviour.Instance.TargetFoodValue;
        int targetGold = TownResourceBehaviour.Instance.TargetGoldValue;
        int targetWater = TownResourceBehaviour.Instance.TargetWaterValue;


        while(targetFood > 2 && targetGold > 2 && targetWater > 2)
        {
            NPCAttributes npc = ScriptableObject.CreateInstance<NPCAttributes>();

            int selection = Random.Range(0, 3);
            int val;
            switch (selection)
            {
                case 0:
                    val = Random.Range(3, targetFood + 1);
                    npc.resourceType1 = NPCAttributes.ResourceType.Food;
                    npc.resourceAmount1 = val;
                    targetFood -= val;
                    break;
                case 1:
                    val = Random.Range(3, targetWater + 1);
                    npc.resourceType1 = NPCAttributes.ResourceType.Water;
                    npc.resourceAmount1 = val;
                    targetWater -= val;
                    break;
                default:
                case 2:
                    val = Random.Range(3, targetGold + 1);
                    npc.resourceType1 = NPCAttributes.ResourceType.Gold;
                    npc.resourceAmount1 = val;
                    targetGold -= val;
                    break;
            }

            bool increment = Random.Range(0.0f, 1.0f) > .5f;
            
            if(increment)
            {
                if(--selection < 0)
                {
                    selection = 2;
                }
            }
            else
            {
                if(++selection > 2)
                {
                    selection = 0;
                }
            }

            switch (selection)
            {
                case 0:
                    val = Random.Range(3, targetFood + 1);
                    npc.resourceType2 = NPCAttributes.ResourceType.Food;
                    npc.resourceAmount2 = val;
                    targetFood -= val;
                    break;
                case 1:
                    val = Random.Range(3, targetWater + 1);
                    npc.resourceType2 = NPCAttributes.ResourceType.Water;
                    npc.resourceAmount2 = val;
                    targetWater -= val;
                    break;
                default:
                case 2:
                    val = Random.Range(3, targetGold + 1);
                    npc.resourceType2 = NPCAttributes.ResourceType.Gold;
                    npc.resourceAmount2 = val;
                    targetGold -= val;
                    break;
            }
            list.Add(npc);
        }

        while(targetFood >= 1)
        {
            NPCAttributes npc = ScriptableObject.CreateInstance<NPCAttributes>();
            int val;

            if (targetFood < 3)
            {
                val = targetFood;
            }
            else
            {
                val = Random.Range(3, targetFood + 1);
            }
            npc.resourceType1 = NPCAttributes.ResourceType.Food;
            npc.resourceAmount1 = val;
            targetFood -= val;
            list.Add(npc);
        }

        while(targetWater >= 1)
        {
            NPCAttributes npc = ScriptableObject.CreateInstance<NPCAttributes>();
            int val;

            if (targetWater < 3)
            {
                val = targetWater;
            }
            else
            {
                val = Random.Range(3, targetWater + 1);
            }
            npc.resourceType1 = NPCAttributes.ResourceType.Water;
            npc.resourceAmount1 = val;
            targetWater -= val;
            list.Add(npc);
        }

        while(targetGold >= 1)
        {
            NPCAttributes npc = ScriptableObject.CreateInstance<NPCAttributes>();
            int val;

            if (targetGold < 3)
            {
                val = targetGold;
            }
            else
            {
                val = Random.Range(3, targetGold + 1);
            }
            npc.resourceType1 = NPCAttributes.ResourceType.Gold;
            npc.resourceAmount1 = val;
            targetGold -= val;
            list.Add(npc);
        }
    }

    public void SpawnWave(List<NPCAttributes> pulledSolution)
    {

        Lane[] lanes = GameObject.FindObjectsByType<Lane>(FindObjectsSortMode.None);
        float DelayNextRelease = 0.5f; // first release
        for (int i = 0; i < pulledSolution.Count; i++)
        {
            GameObject npc = npcPools.Dequeue();
            Lane lane = lanes[Random.Range(0, lanes.Length)];
            npc.GetComponent<NPCBehaviour>().SetAttribute(pulledSolution[i]);
            npc.GetComponent<NPCBehaviour>().SetTargetGrid(lane.grid[0]);
            npc.GetComponent<NPCBehaviour>().SetCurrentGrid(null);
            npc.transform.position = lane.grid[0].transform.position;

            Vector3 pos = npc.transform.position;
            pos.y += Random.Range(-.25f, .25f);
            npc.GetComponent<NPCBehaviour>().SetWaitToStartMovement(DelayNextRelease);
            DelayNextRelease += 1.75f; // Scale to difficulty
            npc.transform.position = pos;
            npc.SetActive(true);

            trackedNPCs.Add(npc.name, npc);

            if (npc.GetComponent<NPCBehaviour>().Attributes.resourceType1 != NPCAttributes.ResourceType.None) 
            {
                DequeueAndSetItem(npc.GetComponent<NPCBehaviour>().Attributes.resourceType1, npc, 0.1f, 0.1f);
            }

            if (npc.GetComponent<NPCBehaviour>().Attributes.resourceType2 != NPCAttributes.ResourceType.None) 
            {
                DequeueAndSetItem(npc.GetComponent<NPCBehaviour>().Attributes.resourceType2, npc, 0.2f, 0);
            }
        }
    }

    private void DequeueAndSetItem(NPCAttributes.ResourceType resourceType, GameObject npc, float xPosMod, float yPosMod) 
    {
        GameObject item = null;
        if (resourceType == NPCAttributes.ResourceType.Gold)
        {
            item = GoldPool.Dequeue();
        }
        else if (resourceType == NPCAttributes.ResourceType.Food)
        {
            item = FoodPool.Dequeue();
        }
        else if (resourceType == NPCAttributes.ResourceType.Water)
        {
            item = WaterPool.Dequeue();
        }

        item.transform.SetParent(npc.transform, false);
        item.transform.position = npc.transform.position;
        item.transform.position = new Vector2(item.transform.position.x + xPosMod, item.transform.position.y - yPosMod);
        item.SetActive(true);
        trackedItems.Add(item.name, item);

        if (npc.GetComponent<NPCBehaviour>().resources.Count >= 1) 
        {
            item.GetComponent<SpriteRenderer>().sortingOrder -= 1;
        }

        npc.GetComponent<NPCBehaviour>().resources.Add(item);

    }

    public void RemoveItemIfTracked(GameObject target)
    {
        if (!trackedItems.ContainsKey(target.name))
            return;

        trackedItems.Remove(target.name);

        if(target.name.Contains("Gold"))
        {
            GoldPool.Enqueue(target);
        }
        else if (target.name.Contains("Food"))
        {
            FoodPool.Enqueue(target);
        }
        else if (target.name.Contains("Water"))
        {
            WaterPool.Enqueue(target);
        }

    }

    public void RemoveIfTracked(GameObject target)
    {
        if(!trackedNPCs.ContainsKey(target.name)) 
            return;

        trackedNPCs.Remove(target.name);
        npcPools.Enqueue(target);
    }


    public void ClearTrackingAndRepool()
    {
        foreach(GameObject obj in trackedNPCs.Values)
        {
            obj.SetActive(false);
            npcPools.Enqueue(obj);
        }

        trackedNPCs.Clear();
    }

    public void CheckTrackedAllMatchTargets()
    {
        int foodCount = 0;
        int waterCount = 0;
        int goldCount = 0;

        foreach (GameObject obj in trackedNPCs.Values)
        {
            NPCAttributes att = obj.GetComponent<NPCBehaviour>().Attributes;
            switch (att.resourceType1)
            {
                case NPCAttributes.ResourceType.Food:
                    foodCount += att.resourceAmount1;
                    break;
                case NPCAttributes.ResourceType.Water:
                    waterCount += att.resourceAmount1;
                    break;
                case NPCAttributes.ResourceType.Gold:
                    goldCount += att.resourceAmount1;
                    break;
                default:
                case NPCAttributes.ResourceType.None:
                    break;
            }
            switch (att.resourceType2)
            {
                case NPCAttributes.ResourceType.Food:
                    foodCount += att.resourceAmount2;
                    break;
                case NPCAttributes.ResourceType.Water:
                    waterCount += att.resourceAmount2;
                    break;
                case NPCAttributes.ResourceType.Gold:
                    goldCount += att.resourceAmount2;
                    break;
                default:
                case NPCAttributes.ResourceType.None:
                    break;
            }
        }

        if (TownResourceBehaviour.Instance.CurrentWaterValue + waterCount != TownResourceBehaviour.Instance.TargetWaterValue)
        {
            return;
        }

        if (TownResourceBehaviour.Instance.CurrentGoldValue + goldCount != TownResourceBehaviour.Instance.TargetGoldValue)
        {
            return;
        }

        if (TownResourceBehaviour.Instance.CurretFoodValue + foodCount != TownResourceBehaviour.Instance.TargetFoodValue)
        {
            return;
        }

        foreach (GameObject obj in trackedNPCs.Values)
        {
            obj.GetComponent<NPCBehaviour>().PerfectRun();
        }
    }
}
