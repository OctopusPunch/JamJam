using UnityEngine;

[System.Serializable]
public abstract class EventBehaviourBase : ScriptableObject
{
    public abstract void Execute();
}
