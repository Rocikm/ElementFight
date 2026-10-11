using UnityEngine;

public class ProjectileAudio : MonoBehaviour, IProjectileComponent
{
    private AudioManager audioManager;
    private ElementAudioData audioData;
    public void Initialize(Projectile projectile)
    {
        audioManager = GameManager.Instance.audioManager;
        audioData = projectile.elementData.audioData;

        projectile.Spawned += OnSpawned;
        projectile.Launched += OnLaunched;
        projectile.Impacted += OnImpacted;
    }

    private void OnSpawned(Projectile projectile)
    {
        PlayRandomAudio(audioData.spawnClips);
    }

    private void OnLaunched(Projectile projectile)
    {
        PlayRandomAudio(audioData.launchClips);
    }
    
    private void OnImpacted(Projectile projectile, Collider col)
    {
        PlayRandomAudio(audioData.impactClips);
    }

    private void PlayRandomAudio(AudioClip[] audioClips)
    {
        if (audioClips == null || audioClips.Length == 0)
            return;
        
        int index = Random.Range(0, audioClips.Length);
        audioManager.PlaySoundAtPosition(audioClips[index], transform.position);
    }
}
