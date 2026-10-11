using UnityEngine;

public class AirVisual : MonoBehaviour, IProjectileComponent
{
    [SerializeField] ParticleSystem trailParticles;
    [SerializeField] ParticleSystem impactParticles;
    private EffectManager effectManager;
    private Projectile projectile;

    public void Initialize(Projectile projectile)
    {
        this.projectile = projectile;
        projectile.Launched += OnLaunched;
        projectile.Impacted += OnImpacted;
        effectManager = GameManager.Instance.effectManager;
    }

    private void OnLaunched(Projectile projectile)
    {
        // Start trail / particles
    }

    private void OnImpacted(Projectile projectile, Collider other)
    {
        if (impactParticles != null)
            effectManager.Play(impactParticles, transform.position, Quaternion.Euler(transform.forward), new EffectOptions{});
    }
}
