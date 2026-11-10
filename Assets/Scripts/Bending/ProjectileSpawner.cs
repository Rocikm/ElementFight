using UnityEngine;
using UnityEngine.UIElements;

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private GestureInput gestureInput;
    [SerializeField] private GestureBinding[] bindings;
    [SerializeField] private Fighter playerFighter;
    private Transform target;
    private AimAssistSettings aimAssistSettings;

    public void SetAimAssist(Transform target, AimAssistSettings settings)
    {
        this.target = target;
        aimAssistSettings = settings;
    }

    void Start()
    {
        gestureInput.OnGesturePerformed += HandleOnGesturePerformed;
    }

    private void HandleOnGesturePerformed(object sender, GestureInput.GestureEventArgs args)
    {
        foreach (GestureBinding binding in bindings)
        {
            if (args.gesture == binding.gesture)
            {
                Vector3 direction = binding.useEndDirection ? args.endDirection : args.startDirection;
                SpawnProjectile(binding.prefab, direction, args.startPosition);
            }
        }
    }

    private void SpawnProjectile(GameObject prefab, Vector3 direction, Vector3 position)
    {
        if (!playerFighter.TryUseStamina(prefab.GetComponent<Projectile>().elementData.staminaCost))
            return;

        GameObject spawned = Instantiate(prefab, transform.position, Quaternion.identity);
        Projectile projectile = spawned.GetComponent<Projectile>();
        projectile.Initialize(gameObject, position);

        if (target != null)
            projectile.SetAimAssist(target, aimAssistSettings);

        projectile.Launch(direction);
    }

    [System.Serializable]
    private class GestureBinding
    {
        public Gesture gesture;
        public GameObject prefab;
        public bool useEndDirection;
    }
}
