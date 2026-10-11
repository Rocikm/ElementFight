using UnityEngine;

[CreateAssetMenu(menuName = "Bending/Movement")]
public class MovementData : ScriptableObject
{
    public MovementType movementType;
    public float speed;
    public bool alignTransformDirection;
}


