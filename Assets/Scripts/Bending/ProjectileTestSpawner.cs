using NUnit.Framework.Internal;
using UnityEngine;

public class ProjectileTestSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] testPrefab;
    [SerializeField] private Transform spawnLocation;
    [SerializeField] private float spawnTime;

    private float timer;
    private int currentPrefab = 0;

    void Awake()
    {
        timer = spawnTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0)
        {
            GameObject spawned = Instantiate(testPrefab[currentPrefab], spawnLocation.position, Quaternion.identity);
            Projectile proj = spawned.GetComponent<Projectile>();
            proj.Initialize(gameObject, transform.position);
            proj.Launch(spawnLocation.forward);
            timer = spawnTime;
            currentPrefab = (currentPrefab + 1) % testPrefab.Length;
        }
    }
}
