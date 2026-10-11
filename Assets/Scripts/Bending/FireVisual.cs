using UnityEngine;

public class FireVisual : MonoBehaviour
{
    [SerializeField] Projectile projectile;
    [SerializeField] ParticleSystem trailParticles;
    [SerializeField] ParticleSystem impactParticles;
    private EffectManager effectManager;

    private void OnEnable()
    {
        projectile.Launched += OnLaunched;
        projectile.Impacted += OnImpacted;
        effectManager = GameManager.Instance.effectManager;
    }

    private void OnDisable()
    {
        projectile.Launched -= OnLaunched;
        projectile.Impacted -= OnImpacted;
    }

    private void OnLaunched(Projectile projectile)
    {
        // Start trail / particles
    }

    private void OnImpacted(Projectile projectile, Collider other)
    {
        if (impactParticles != null)
            effectManager.Play(impactParticles, transform.position, Quaternion.identity, new EffectOptions{});
    }
}
