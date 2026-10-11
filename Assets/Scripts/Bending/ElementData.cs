using UnityEngine;

[CreateAssetMenu(menuName = "Bending/Element")]
public class ElementData : ScriptableObject
{
    public ElementType elementType;
    public float baseDamage;
    public float speed = 5;
    public MovementType movementType;
    public bool alignForwardDirection;
    public bool requiresPreparation;
    public float preparationTime;
    public float cleanupTime;
    public Vector3 spawnOffset;
    public ElementAudioData audioData;
}

public enum ElementType
{
    Air,
    Water,
    Earth,
    Fire
}

public enum MovementType
{
    Straight,
    Curved
}
