using UnityEngine;

[CreateAssetMenu(menuName = "Actors/Fighter")]
public class FighterData : ScriptableObject
{
    public float maxHealth;
    public float maxStamina;
    public float staminaRecovery;
}
