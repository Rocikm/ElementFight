using UnityEngine;

public class HandDamage : MonoBehaviour, IDamageable
{
    [SerializeField] private GestureInput gestureInput;
    [SerializeField] private Fighter player;
    [SerializeField] private PlayerData playerData;
    [SerializeField] ParticleSystem blockParticles;
    [SerializeField] AudioClip blockAudio;
    [SerializeField] AudioClip damageAudio;
    private AudioManager audioManager;
    private EffectManager effectManager;

    void Awake()
    {
        audioManager = GameManager.Instance.audioManager;
        effectManager = GameManager.Instance.effectManager;
    }
    public void TakeDamage(ElementData elementData)
    {
        if (gestureInput.velocity.magnitude > playerData.minBlockSpeed)
        {
            player.TryUseStamina(-player.Data.attackStaminaCost);
            audioManager.PlaySoundAtPosition(blockAudio, transform.position);
            effectManager.Play(blockParticles, transform.position, Quaternion.identity, new EffectOptions{});
            return;
        }

        player.TakeDamage(elementData);
        audioManager.PlaySoundAtPosition(damageAudio, transform.position);
    }
}
