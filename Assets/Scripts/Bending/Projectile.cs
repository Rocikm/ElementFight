using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ProjectileMovement))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileMovement movement;
    [field:SerializeField] public ElementData elementData { get; private set; }
    [SerializeField] private MonoBehaviour[] components;
    public Vector3 initialPosition { get; private set; }
    private ProjectileState state;
    private GameObject owner;
    private Transform target;
    private float timer;
    private float expirationTime = 5f;

    public event Action<Projectile> Spawned;
    public event Action<Projectile> Ready;
    public event Action<Projectile> Launched;
    public event Action<Projectile> Returned;
    public event Action<Projectile, Collider> Impacted;


    public void Initialize(GameObject owner, Vector3 initialPosition)
    {
        this.owner = owner;
        this.initialPosition = initialPosition;
        transform.position += elementData.spawnOffset;

        IProjectileComponent[] pComponents = GetComponents<IProjectileComponent>();
        foreach (var component in pComponents)
        {
            component.Initialize(this);
        }

        // foreach (var component in components)
        // {
        //     ((IProjectileComponent)component).Initialize(this);
        // }

        Spawned?.Invoke(this);

        if (elementData.requiresPreparation)
            Prepare(initialPosition);
    }

    public void SetAimAssist(Transform target, AimAssistSettings settings)
    {
        this.target = target;
        movement.SetAimAssistSettings(settings);

        if (state == ProjectileState.Launched)
            movement.SetAimAssist(target, true);
    }

    public void Prepare(Vector3 targetPosition)
    {
        state = ProjectileState.Preparing;
        timer = elementData.preparationTime;

        movement.MoveToPosition(targetPosition, elementData);
    }

    public void Launch(Vector3 direction)
    {
        if (elementData.requiresPreparation && state != ProjectileState.AwaitingAction)
            return;

        if (elementData.movementType == MovementType.Curved)
        {
            Vector3 launchTarget = 
                target == null ? 
                direction * 8 : 
                target.position - owner.transform.position;

            movement.LaunchCurved(Quaternion.AngleAxis(90, Vector3.up) * owner.transform.forward,
                launchTarget, launchTarget.magnitude * 0.45f, elementData);
        }

        else
            movement.Launch(direction, elementData);

        if (target != null)
            movement.SetAimAssist(target, true);

        state = ProjectileState.Launched;
        timer = expirationTime;
        Launched?.Invoke(this);
    }

    public static void SpawnProjectile(GameObject prefab, GameObject owner, Vector3 position, Vector3 direction)
    {
        GameObject spawned = Instantiate(prefab, position, Quaternion.identity);
        Projectile projectile = spawned.GetComponent<Projectile>();
        projectile.Initialize(owner, position);
        projectile.Launch(direction);
    }

    private void Update()
    {
        switch (state)
        {
            case ProjectileState.Preparing:
                if (TickTimer())
                {
                    Ready?.Invoke(this);
                    state = ProjectileState.AwaitingAction;
                    timer = expirationTime;
                }
                break;
                
            case ProjectileState.AwaitingAction:
                if (TickTimer())
                {
                    Launch(Vector3.down);
                    movement.TogglePhysics(true);
                }
                break;

            case ProjectileState.Launched:
                if (TickTimer())
                {
                    movement.Stop();
                    state = ProjectileState.Impacted;
                    // movement.TogglePhysics(true);
                }
                break;

            case ProjectileState.Impacted:
                if (TickTimer())
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    private bool TickTimer()
    {
        timer -= Time.deltaTime;

        return timer <= 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state == ProjectileState.Launched &&
            (owner == null || other.transform.root != owner.transform.root))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(elementData);

            Impacted?.Invoke(this, other);
            movement.Stop();

            timer = elementData.cleanupTime;
            state = ProjectileState.Impacted;
        }
    }
}

public enum ProjectileState
{
    Preparing,
    AwaitingAction,
    Launched,
    Impacted
}
