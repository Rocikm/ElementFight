using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ProjectileMovement))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileMovement movement;
    [SerializeField] private MonoBehaviour[] components;
    public Vector3 InitialPosition { get; private set; }
    public ElementData ElementData => elementData;

    private ElementData elementData;
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


    public void Initialize(ElementData elementData, GameObject owner, Vector3 initialPosition)
    {
        this.elementData = elementData;
        this.owner = owner;
        this.InitialPosition = initialPosition;
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
        
        direction = direction.normalized;

        if (elementData.movementType == MovementType.Curved)
        {
            Vector3 launchTarget = 
                target == null ? 
                transform.position + direction * 8 : 
                transform.position + direction * (target.position - transform.position).magnitude;

            Vector3 forward = direction.normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;

            Vector3 rotate = elementData.initialDirection;

            Vector3 adjustedDirection =
                forward * rotate.z +
                up * rotate.y +
                right * rotate.x;

            movement.LaunchCurved(adjustedDirection,
                launchTarget, (launchTarget - transform.position).magnitude * 0.4f, elementData);
        }
        else
            movement.Launch(direction, elementData);

        if (target != null)
            movement.SetAimAssist(target, true);

        state = ProjectileState.Launched;
        timer = expirationTime;
        Launched?.Invoke(this);
    }

    public static void SpawnProjectile(GameObject prefab, ElementData elementData, GameObject owner, Vector3 position, Vector3 direction, Transform target)
    {
        GameObject spawned = Instantiate(prefab, position, Quaternion.identity);
        Projectile projectile = spawned.GetComponent<Projectile>();
        projectile.Initialize(elementData, owner, position);
        projectile.SetAimAssist(target, new AimAssistSettings{alignmentThreshold = 0.4f, turnSpeed = 0.2f});
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

            GetComponent<Collider>().enabled = false;

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
