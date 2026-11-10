using UnityEngine;
using System.Collections;

public class PlayerDamageVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Fighter fighter;
    [SerializeField] private GameObject[] targetObjects;

    [Header("Settings")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;

    private Material[] materials;
    private Color[] originalColors;

    private Coroutine flashCoroutine;

    private void Awake()
    {
        materials = new Material[targetObjects.Length];
        originalColors = new Color[targetObjects.Length];

        for (int i = 0; i < targetObjects.Length; i++)
        {
            materials[i] = targetObjects[i]
                .GetComponent<Renderer>().material;

            originalColors[i] = materials[i].GetColor("_BaseColor");
        }
    }

    private void OnEnable()
    {
        fighter.OnDamageTaken += HandleDamageTaken;
    }

    private void OnDisable()
    {
        fighter.OnDamageTaken -= HandleDamageTaken;
    }

    private void HandleDamageTaken(Fighter fighter, float damage)
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashDamage());
    }

    private IEnumerator FlashDamage()
    {
        foreach (Material mat in materials)
            mat.SetColor("_BaseColor", damageColor);

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < materials.Length; i++)
            materials[i].SetColor("_BaseColor", originalColors[i]);

        flashCoroutine = null;
    }
}
