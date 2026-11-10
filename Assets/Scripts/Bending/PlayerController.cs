using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private ProjectileSpawner[] projectileSpawners;
    [SerializeField] private EnemyController enemy;
    [SerializeField] private Transform target;
    [SerializeField] private Transform enemySpawn;
    [SerializeField] private AimAssistSettings aimAssistSettings;
    // [SerializeField] private InputActionProperty reviveAction;
    private FightManager fightManager;
    private Transform currentTarget;

    void Awake()
    {
        fightManager = GameManager.Instance.fightManager;
        currentTarget = target;
        UpdateTarget();
        fightManager.OnFightEnded += HandleOnFightEnded;
    }

    void Update()
    {
        // if (reviveAction.action.ReadValue<float>() > 0.5)
        // {
        //     enemy.OnFightStarted(enemySpawn.position);
        //     currentTarget = target;
        //     UpdateTarget();
        // }
    }

    void OnValidate()
    {
        UpdateTarget();
    }

    private void UpdateTarget()
    {
        foreach (var spawner in projectileSpawners)
        {
            spawner.SetAimAssist(currentTarget, aimAssistSettings);
        }
    }

    private void HandleOnFightEnded(Fighter winner, Fighter loser)
    {
        currentTarget = null;
        UpdateTarget();
    }
}
