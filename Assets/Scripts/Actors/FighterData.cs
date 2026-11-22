using UnityEngine;

[CreateAssetMenu(menuName = "Actors/Fighter")]
public class FighterData : ScriptableObject
{
    public float maxHealth = 100;
    public float maxStamina = 100;
    public float staminaRecovery = 20;
    public float attackStaminaCost = 25;
}
