using UnityEngine;

public class RockInteraction : MonoBehaviour, IProjectileComponent
{
    private Projectile projectile;
    private bool waitingForHit;

    public void Initialize(Projectile projectile)
    {
        this.projectile = projectile;
        projectile.Ready += OnProjectileReady;
        projectile.Launched += OnProjectileLaunched;
    }

    private void OnDisable()
    {
        if (projectile == null)
            return;

        projectile.Ready -= OnProjectileReady;
        projectile.Launched -= OnProjectileLaunched;
    }

    private void OnProjectileReady(Projectile projectile)
    {
        waitingForHit = true;
    }

    private void OnProjectileLaunched(Projectile projectile)
    {
        waitingForHit = false;
        
        // var rb = GetComponent<Rigidbody>();
        // if (rb != null)
        // {
        //     rb.useGravity = true;
        //     rb.isKinematic = false;
        // }
    }


    private void OnTriggerEnter(Collider other)
    {        
        if (!waitingForHit)
            return;

        GestureInput handInput = other.GetComponent<GestureInput>();
        
        if (handInput == null)
            return;

        Vector3 velocity = handInput.velocity;

        if (velocity.sqrMagnitude > 0.0001f)
        {
            projectile.Launch(velocity.normalized);
        }
    }
}