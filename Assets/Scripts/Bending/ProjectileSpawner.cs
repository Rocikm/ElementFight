using UnityEngine;
using UnityEngine.UIElements;

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private GestureInput gestureInput;
    [SerializeField] private GestureBinding[] bindings;

    void Start()
    {
        gestureInput.OnGesturePerformed += HandleOnGesturePerformed;
    }

    private void HandleOnGesturePerformed(object sender, GestureInput.GestureEventArgs args)
    {
        foreach (GestureBinding binding in bindings)
        {
            if (args.gesture == binding.gesture)
                SpawnProjectile(binding.prefab, args.startDirection, args.startPosition);
        }
    }

    private void SpawnProjectile(GameObject prefab, Vector3 direction, Vector3 position)
    {
            GameObject spawned = Instantiate(prefab, transform.position, Quaternion.identity);
            Projectile projectile = spawned.GetComponent<Projectile>();
            projectile.Initialize(gameObject, position);
            projectile.Launch(direction);
    }

    [System.Serializable]
    private class GestureBinding
    {
        public Gesture gesture;
        public GameObject prefab;
    }
}
