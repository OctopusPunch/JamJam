using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Waves
{
    public List<SubWave> subWaves = new List<SubWave>();


    // No idea if this is how we;ll handle events but have it here for now.
    [SerializeReference]
    public List<EventBehaviourBase> possibleEvents = new List<EventBehaviourBase>();

    public float eventChances = 10;
}
