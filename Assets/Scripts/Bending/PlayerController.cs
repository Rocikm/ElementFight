using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private ProjectileSpawner[] projectileSpawners;
    [SerializeField] private EnemyController enemy;
    [SerializeField] private Transform target;
    [SerializeField] private Transform enemySpawn;
    [SerializeField] private AimAssistSettings aimAssistSettings;
    [SerializeField] private InputActionProperty reviveAction;
    private float currentStamina;
    private float currentHealth;

    void Awake()
    {
        UpdateTarget();
        enemy.OnDefeated += HandleOnEnemyDefeated;
    }

    void Update()
    {
        if (reviveAction.action.ReadValue<float>() > 0.5)
            enemy.Revive(enemySpawn.position);
    }

    void OnValidate()
    {
        UpdateTarget();
    }

    private void UpdateTarget()
    {
        foreach (var spawner in projectileSpawners)
        {
            spawner.SetAimAssist(target, aimAssistSettings);
        }
    }

    private void HandleOnEnemyDefeated()
    {
        target = null;
        UpdateTarget();
    }
}
