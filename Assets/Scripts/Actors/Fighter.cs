using UnityEngine;
using System;

public class Fighter : MonoBehaviour, IDamageable
{
    [SerializeField] private FighterData data;

    private float currentHealth;
    private float currentStamina;

    public float CurrentHealth => currentHealth;
    public float CurrentStamina => currentStamina;
    public float MaxHealth => data.maxHealth;
    public float MaxStamina => data.maxStamina;

    public bool IsDefeated => currentHealth <= 0;
    public bool CombatEnabled => combatEnabled;

    public event Action<Fighter> OnDefeated;
    public event Action<Fighter, float> OnDamageTaken;
    public event Action<Fighter> OnStarted;

    private bool combatEnabled;

    private void Awake()
    {
        currentHealth = data.maxHealth;
        currentStamina = data.maxStamina;
    }

    void Update()
    {
        if (!combatEnabled)
            return;

        currentStamina = Mathf.Min(data.maxStamina,
            currentStamina + data.staminaRecovery * Time.deltaTime);
    }

    public bool TryUseStamina(float amount)
    {
        if (currentStamina < amount)
            return false;

        currentStamina -= amount;
        currentStamina = Mathf.Max(0, currentStamina);

        return true;
    }

    public void TakeDamage(ElementData elementData)
    {
        if (!combatEnabled || IsDefeated)
            return;

        currentHealth -= Mathf.Abs(elementData.baseDamage);
        currentHealth = Mathf.Max(0, currentHealth);

        if (IsDefeated)
        {
            OnDefeated?.Invoke(this);
        }
        else
        {
            OnDamageTaken?.Invoke(this, elementData.baseDamage);
        }
    }

    public void SetCombatEnabled(bool enabled)
    {
        combatEnabled = enabled;
        OnStarted?.Invoke(this);
    }

    public void ResetStats()
    {
        currentHealth = data.maxHealth;
        currentStamina = data.maxStamina;
    }
}

