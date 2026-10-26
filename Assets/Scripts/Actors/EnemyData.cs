using UnityEngine;

[CreateAssetMenu(menuName = "Actors/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public float staminaRecovery = 10f;
    public float desiredDistanceMin = 5f;
    public float desiredDistanceMax = 10f;
    public float movementTime = 2f;
    public float turnTime = 1f;
    public float attackTime = 5f;
    public float minTurnAngle = 30f;
}
