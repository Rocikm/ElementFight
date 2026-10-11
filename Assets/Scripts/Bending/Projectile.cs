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
    private float timer;

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
            movement.LaunchCurved(Quaternion.AngleAxis(90, Vector3.up) * owner.transform.forward,
                direction * 8, 2, elementData);
        else
            movement.Launch(direction, elementData);

        state = ProjectileState.Launched;
        Launched?.Invoke(this);
    }

    private void Update()
    {
        switch(state)
        {
            case ProjectileState.Preparing:
                WaitForPrepared(Time.deltaTime);
                break;
            case ProjectileState.Impacted:
                WaitForDestroyed(Time.deltaTime);
                break;
        }
    }

    private void WaitForPrepared(float deltaTime)
    {
        timer -= deltaTime;

        if (timer <= 0)
        {
            Ready?.Invoke(this);
            state = ProjectileState.AwaitingAction;
        }
    }

    private void WaitForDestroyed(float deltaTime)
    {
        timer -= deltaTime;

        if (timer <= 0)
        {
            Destroy(gameObject);
        }
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
