using UnityEngine;

[CreateAssetMenu(fileName = "NPCAttributes", menuName = "Scriptable Objects/NPCAttributes")]
public class NPCAttributes : ScriptableObject
{
    public float movementSpeed = 1;
    public int foodResource = 0;
    public int waterResource = 0;
}