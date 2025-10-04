using UnityEngine;

[CreateAssetMenu(fileName = "NPCAttributes", menuName = "Scriptable Objects/NPCAttributes")]
public class NPCAttributes : ScriptableObject
{
    public enum ResourceType
    {
        Food,
        Water,
        Gold,
        None
    }


    public float movementSpeed = 1;

    public ResourceType resourceType1 = ResourceType.None;
    public int resourceAmount1 = 0;

    public ResourceType resourceType2 = ResourceType.None;
    public int resourceAmount2 = 0;


    public string GetResource1Info()
    {
        switch(resourceType1)
        {
            case ResourceType.Food:
                return "Food " + resourceAmount1;
                case ResourceType.Water:
                return "Water " + resourceAmount1;
            case ResourceType.Gold:
                return "Gold " + resourceAmount1;
            default:
            case ResourceType.None:
                return string.Empty;
        }
    }
    public string GetResource2Info()
    {
        switch(resourceType2)
        {
            case ResourceType.Food:
                return "<br>Food " + resourceAmount2;
                case ResourceType.Water:
                return "<br>Water " + resourceAmount2;
            case ResourceType.Gold:
                return "<br>Gold " + resourceAmount2;
            default:
            case ResourceType.None:
                return string.Empty;
        }
    }

    public string GetResourceInfo()
    {
        return GetResource1Info() + GetResource2Info();
    }
}