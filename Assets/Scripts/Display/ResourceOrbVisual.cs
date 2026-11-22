
using UnityEngine;

public class ResourceOrbVisual : MonoBehaviour
{
    public enum ResourceType
    {
        Stamina,
        Health
    }

    [Header("References")]
    [SerializeField] private Fighter fighter;
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private Renderer[] orbs;

    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.cyan;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0f);

    [Header("Settings")]
    [SerializeField] private bool interpolateColor = false;

    private Material[] materials;

    private void Awake()
    {
        materials = new Material[orbs.Length];

        for (int i = 0; i < orbs.Length; i++)
            materials[i] = orbs[i].material;
    }

    private void Update()
    {
        float currentResource;
        float maxResource;

        switch (resourceType)
        {
            case ResourceType.Stamina:
                currentResource = fighter.CurrentStamina;
                maxResource = fighter.MaxStamina;
                break;

            case ResourceType.Health:
                currentResource = fighter.CurrentHealth;
                maxResource = fighter.MaxHealth;
                break;

            default:
                return;
        }

        UpdateOrbs(currentResource, maxResource);
    }

    private void UpdateOrbs(float current, float max)
    {
        if (materials.Length == 0 || max <= 0f)
            return;

        float resourcePerOrb = max / materials.Length;

        for (int i = 0; i < materials.Length; i++)
        {
            float threshold = resourcePerOrb * (i + 1);

            float colorPercent = interpolateColor
                ? Mathf.Clamp01((current - threshold + resourcePerOrb) / resourcePerOrb)
                : (current >= threshold ? 1f : 0f);

            Color targetColor = Color.Lerp(
                inactiveColor,
                activeColor,
                colorPercent
            );

            materials[i].SetColor("_BaseColor", targetColor);
        }
    }
}
