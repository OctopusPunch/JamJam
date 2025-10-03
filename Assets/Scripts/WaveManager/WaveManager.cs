using UnityEngine;
using System.Collections.Generic;

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

}
