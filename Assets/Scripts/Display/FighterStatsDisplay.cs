
using UnityEngine;
using UnityEngine.UI;

public class FighterStatsDisplay : MonoBehaviour
{
    [Header("Fighter")]
    [SerializeField] private Fighter fighter;

    [Header("UI Bars")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image staminaFill;

    [Header("Display Settings")]
    [SerializeField] private bool smoothBars = true;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Billboard (Optional)")]
    [SerializeField] private bool faceCamera = false;
    [SerializeField] private Transform cameraTransform;

    private float targetHealth;
    private float targetStamina;

    private void Start()
    {
        if (fighter == null)
        {
            Debug.LogWarning("Stats Display: No Fighter assigned!", this);
            enabled = false;
            return;
        }

        UpdateTargets();

        if (faceCamera && cameraTransform == null)
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        UpdateTargets();
        UpdateBars();
    }

    private void LateUpdate()
    {
        if (!faceCamera || cameraTransform == null)
            return;

        Vector3 direction = cameraTransform.position - transform.position;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private void UpdateTargets()
    {
        targetHealth = fighter.MaxHealth > 0
            ? fighter.CurrentHealth / fighter.MaxHealth
            : 0f;

        targetStamina = fighter.MaxStamina > 0
            ? fighter.CurrentStamina / fighter.MaxStamina
            : 0f;

        targetHealth = Mathf.Clamp01(targetHealth);
        targetStamina = Mathf.Clamp01(targetStamina);
    }

    private void UpdateBars()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = smoothBars
                ? Mathf.Lerp(healthFill.fillAmount, targetHealth,
                    smoothSpeed * Time.deltaTime)
                : targetHealth;
        }

        if (staminaFill != null)
        {
            staminaFill.fillAmount = smoothBars
                ? Mathf.Lerp(staminaFill.fillAmount, targetStamina,
                    smoothSpeed * Time.deltaTime)
                : targetStamina;
        }
    }
}
