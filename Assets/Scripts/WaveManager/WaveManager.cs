using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class WaveManager
{
    [SerializeField]
    public List<Waves> Waves = new List<Waves>();

    public int currentWave = 0;
    public int currentSubWave = 0;

    public Dictionary<string, GameObject> trackedNPCs = new Dictionary<string, GameObject>(250);

    public GameObject NpcPrefab;

    public Queue<GameObject> npcPools = new Queue<GameObject>(250);

    public bool beginNewWave;

    [Header("Wave Scaling Settings")]
    [SerializeField] int baseSubwaves = 2;
    [SerializeField] int subWaveIncrease = 4;
    [SerializeField] int minNpcsPerSubwave = 3;      
    [SerializeField] int maxNpcsPerSubwave = 10;     
    [SerializeField] int baseDemand = 0;
    [SerializeField] int perWaveIncrement = 1;       
    [SerializeField] int NPCCarryLimit = 2;
    [SerializeField] float baseMarginOfError = 0.2f;
    [SerializeField] float maxMarginOfError = 1.0f;
    [SerializeField] float marginOfErrorGrowthRate = 1.15f;


    public float GetCurrentSubWaveSpawnTime()
    {
        if(Waves.Count <= currentWave)
        {
            return 0;
        }
        if(Waves[currentWave].subWaves.Count <= currentSubWave)
        {
            return 0;
        }
        return Waves[currentWave].subWaves[currentSubWave].spawnSubWaveInSeconds;
    }

    public void Initialise()
    {
        GameObject holder = new GameObject("#NPC Holder");
        MonoBehaviour.DontDestroyOnLoad(holder);
        for (int i = 0; i < 250; i++)
        {
            GameObject newNpc = MonoBehaviour.Instantiate(NpcPrefab);

            newNpc.SetActive(false);
            newNpc.transform.position = Vector3.one * -1000;
            newNpc.transform.SetParent(holder.transform, false);
            newNpc.name = "NPC #" + i;
            npcPools.Enqueue(newNpc);
        }

        Waves.Clear();
        Waves = GenerateWaves(10);
    }

    public void ResetWaves()
    {
        currentWave = 0;
        currentSubWave = 0;

        
        ClearTrackingAndRepool();
    }


    public void ReleaseNewSubwave()
    {
        if(currentSubWave >= Waves[currentWave].subWaves.Count) 
        {
            if(trackedNPCs.Count > 0)
            {
                return;
            }
            ++currentWave;
            Debug.Log("Increased wave to: " + currentWave);
            currentSubWave = 0;
            beginNewWave = true;
            return;
        }

        Lane[] lanes = GameObject.FindObjectsByType<Lane>(FindObjectsSortMode.None);

        SubWave subWave = Waves[currentWave].subWaves[currentSubWave];


        float DelayNextRelease = 0.5f;
        for(int i = 0; i < subWave.waveData.Count; i++)
        {
            GameObject npc = npcPools.Dequeue();
            npc.GetComponent<NPCBehaviour>().SetAttribute(subWave.waveData[i]);
            npc.GetComponent<NPCBehaviour>().SetTargetGrid(lanes[Random.Range(0, lanes.Length)].grid[0]);
            npc.transform.position = lanes[Random.Range(0, lanes.Length)].grid[0].transform.position;

            Vector3 pos = npc.transform.position;
            pos.y += Random.Range(-.35f, .35f);
            npc.GetComponent<NPCBehaviour>().SetWaitToStartMovement(DelayNextRelease);
            DelayNextRelease += .75f;
            npc.transform.position = pos;
            npc.SetActive(true);

            trackedNPCs.Add(npc.name, npc);
        }
        ++currentSubWave;
    }


    public void CheckIsLastSubwave()
    {

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

    private List<Waves> GenerateWaves(int amountOfWaves)
    {
        List<Waves> generatedWaves = new List<Waves>();

        for (int i = 0; i < amountOfWaves; i++)
        {
            Waves newWave = new Waves();

            int subWaveCount = baseSubwaves + (i / subWaveIncrease);
            newWave.subWaves = GenerateSubwaves(subWaveCount, 3, 6);

            float marginOfError = Mathf.Min(maxMarginOfError, baseMarginOfError * Mathf.Pow(marginOfErrorGrowthRate, i));

            Demand monsterDemand = new Demand
            {
                Food = (baseDemand + perWaveIncrement * i) + Random.Range(0, 3),
                Water = (baseDemand + perWaveIncrement * i) + Random.Range(0, 3),
                Money = (baseDemand + perWaveIncrement * i) + Random.Range(0, 3)
            };

            Demand cityDemand = new Demand
            {
                Food = (baseDemand + perWaveIncrement * i) + Random.Range(0, 3),
                Water = (baseDemand + perWaveIncrement * i) + Random.Range(0, 3),
                Money = (baseDemand + perWaveIncrement * i) + Random.Range(0, 3)
            };

            List<NPCAttributes> npcs = GenerateNPCAttributes(monsterDemand, cityDemand, NPCCarryLimit, marginOfError);

            int npcIndex = 0;
            for (int j = 0; j < newWave.subWaves.Count; j++)
            {
                int npcsInThisSubwave = Random.Range(minNpcsPerSubwave, maxNpcsPerSubwave + 1);

                for (int k = 0; k < npcsInThisSubwave; k++)
                {
                    NPCAttributes npcToAdd;
                    if (npcIndex < npcs.Count)
                    {
                        npcToAdd = npcs[npcIndex++];
                    }
                    else
                    {
                        var src = npcs[Random.Range(0, Mathf.Max(1, npcs.Count))];
                        npcToAdd = ScriptableObject.CreateInstance<NPCAttributes>();
                        npcToAdd.resourceType1 = src.resourceType1;
                        npcToAdd.resourceAmount1 = src.resourceAmount1;
                        npcToAdd.resourceType2 = src.resourceType2;
                        npcToAdd.resourceAmount2 = src.resourceAmount2;
                    }
                    newWave.subWaves[j].waveData.Add(npcToAdd);
                }
            }

            newWave.monsterDemand = monsterDemand;
            newWave.townDemand = cityDemand;

            generatedWaves.Add(newWave);
        }

        return generatedWaves;
    }

    private List<SubWave> GenerateSubwaves(int numberOfSubWaves, int minSpawn, int maxSpawn)
    {
        List<SubWave> subWaves = new List<SubWave>();
        for (int i = 0; i < numberOfSubWaves; i++)
        {
            SubWave newSubWave = new SubWave
            {
                spawnSubWaveInSeconds = Random.Range(minSpawn, maxSpawn + 1)
            };
            subWaves.Add(newSubWave);
        }
        return subWaves;
    }

    public List<NPCAttributes> GenerateNPCAttributes(Demand monsterDemand, Demand townDemand, int npcCarryLimit, float marginOfError)
    {
        var neededResources = monsterDemand.ToDict();
        foreach (var pair in townDemand.ToDict()) neededResources[pair.Key] += pair.Value;

        foreach (var key in neededResources.Keys.ToList())
        {
            neededResources[key] = Mathf.CeilToInt(neededResources[key] * (1 + marginOfError));
        }

        var npcs = new List<NPCAttributes>();
        var rand = new System.Random();

        while (neededResources.Values.Any(v => v > 0))
        {
            var biggest = neededResources
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Take(2)
                .ToList();

            if (biggest.Count == 0) break;

            int remainingCarry = npcCarryLimit;
            var npcResources = new List<(NPCAttributes.ResourceType, int)>();

            foreach (var kv in biggest)
            {
                if (remainingCarry <= 0) break;
                int maxCanGive = Mathf.Min(kv.Value, remainingCarry);
                int toGive = rand.Next(1, maxCanGive + 1);
                npcResources.Add((kv.Key, toGive));
                neededResources[kv.Key] -= toGive;
                remainingCarry -= toGive;
            }

            NPCAttributes newNpcAttr = ScriptableObject.CreateInstance<NPCAttributes>();

            if (npcResources.Count >= 1)
            {
                newNpcAttr.resourceType1 = npcResources[0].Item1;
                newNpcAttr.resourceAmount1 = npcResources[0].Item2;
            }
            else
            {
                newNpcAttr.resourceType1 = NPCAttributes.ResourceType.None;
                newNpcAttr.resourceAmount1 = 0;
            }

            if (npcResources.Count == 2)
            {
                newNpcAttr.resourceType2 = npcResources[1].Item1;
                newNpcAttr.resourceAmount2 = npcResources[1].Item2;
            }
            else
            {
                newNpcAttr.resourceType2 = NPCAttributes.ResourceType.None;
                newNpcAttr.resourceAmount2 = 0;
            }

            npcs.Add(newNpcAttr);
        }

        return npcs;
    }
}

[System.Serializable]
public class Demand
{
    public int Money;
    public int Food;
    public int Water;

    public Dictionary<NPCAttributes.ResourceType, int> ToDict()
    {
        return new Dictionary<NPCAttributes.ResourceType, int>
        {
            { NPCAttributes.ResourceType.Gold, Money },
            { NPCAttributes.ResourceType.Food,  Food  },
            { NPCAttributes.ResourceType.Water, Water }
        };
    }
}