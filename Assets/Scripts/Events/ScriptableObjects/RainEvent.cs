using UnityEngine;

[CreateAssetMenu(fileName = "RainEvent", menuName = "Scriptable Objects/RainEvent")]
public class RainEvent : EventBehaviourBase
{
    public override void Execute()
    {
        Debug.Log("Set is Raining");
    }
}
