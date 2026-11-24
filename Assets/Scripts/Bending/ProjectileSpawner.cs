using UnityEngine;
using UnityEngine.UIElements;

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private GestureInput gestureInput;
    [SerializeField] private GestureBinding[] bindings;
    [SerializeField] private Fighter playerFighter;
    [SerializeField] private AudioClip attackFailedClip;
    private Transform target;
    private AimAssistSettings aimAssistSettings;
    private AudioManager audioManager;

    public void SetAimAssist(Transform target, AimAssistSettings settings)
    {
        this.target = target;
        aimAssistSettings = settings;
    }

    void Start()
    {
        gestureInput.OnGesturePerformed += HandleOnGesturePerformed;
        audioManager = GameManager.Instance.audioManager;
    }

    private void HandleOnGesturePerformed(object sender, GestureInput.GestureEventArgs args)
    {
        foreach (GestureBinding binding in bindings)
        {
            if (args.gesture == binding.gesture)
            {
                Vector3 direction = binding.useEndDirection ? args.endDirection : args.startDirection;
                SpawnProjectile(binding.elementData, binding.prefab, direction, args.startPosition);
            }
        }
    }

    private void SpawnProjectile(ElementData elementData, GameObject prefab, Vector3 direction, Vector3 position)
    {
        // Aim assist configuration

        
        // if (!playerFighter.TryUseStamina(prefab.GetComponent<Projectile>().elementData.staminaCost))
        //     return; 
        if (!playerFighter.TryUseStamina(playerFighter.Data.attackStaminaCost))
        {
            if (attackFailedClip != null)
                audioManager.PlaySoundAtPosition(attackFailedClip, transform.position);

            return;
        }

        GameObject spawned = Instantiate(prefab, transform.position, Quaternion.identity);
        Projectile projectile = spawned.GetComponent<Projectile>();
        projectile.Initialize(elementData, gameObject, position);

        if (target != null)
        {
            projectile.SetAimAssist(target, aimAssistSettings);
            
            Vector3 targetDirection = (target.position - transform.position).normalized;
            direction.Normalize();

            float angle = Vector3.Angle(direction, targetDirection);

            if (angle > 0.01f)
            {
                float correction = Mathf.Clamp01(aimAssistSettings.initialStrength);

                Vector3 adjustedDirection = Vector3.Slerp(
                    direction, targetDirection, correction).normalized;

                float adjustedAngle = Vector3.Angle(direction, adjustedDirection);

                if (adjustedAngle > aimAssistSettings.initialMaxAngle)
                {
                    adjustedDirection = Vector3.RotateTowards(direction, adjustedDirection, 
                        aimAssistSettings.initialMaxAngle * Mathf.Deg2Rad, 0f
                    ).normalized;
                }

                direction = adjustedDirection;
            }
        }

        projectile.Launch(direction);
    }

    [System.Serializable]
    private class GestureBinding
    {
        public Gesture gesture;
        public GameObject prefab;
        public ElementData elementData;
        public bool useEndDirection;
    }
}
