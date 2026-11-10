
using UnityEngine;

public class StaminaVisual : MonoBehaviour
{
    [SerializeField] private Fighter fighter;
    [SerializeField] private Renderer[] stamina;
    [SerializeField] private Renderer[] health;

    [SerializeField] private Color lowStaminaColor = Color.gray;
    [SerializeField] private Color fullStaminaColor = Color.cyan;
    [SerializeField] private Color lowHealthColor = Color.gray;
    [SerializeField] private Color fullHealthColor = Color.red;

    private Material[] staminaMaterials;
    private Material[] healthMaterials;

    private void Awake()
    {
        staminaMaterials = new Material[stamina.Length];

        for (int i = 0; i < stamina.Length; i++)
            staminaMaterials[i] = stamina[i].material;

        healthMaterials = new Material[health.Length];

        for (int i = 0; i < health.Length; i++)
            healthMaterials[i] = health[i].material;
    }

    private void Update()
    {
        float staminaPercent = fighter.CurrentStamina / fighter.MaxStamina;

        Color staminaColor = Color.Lerp(
            lowStaminaColor, fullStaminaColor, staminaPercent);

        foreach (Material mat in staminaMaterials)
        {
            mat.SetColor("_BaseColor", staminaColor);
        }

        float healthPercent = fighter.CurrentHealth / fighter.MaxHealth;

        Color healthColor = Color.Lerp(
            lowHealthColor, fullHealthColor, healthPercent);

        foreach (Material mat in healthMaterials)
        {
            mat.SetColor("_BaseColor", healthColor);
        }
    }
}
