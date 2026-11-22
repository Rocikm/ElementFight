
using System;
using UnityEngine;

public class FighterInitializer : MonoBehaviour
{
    [Serializable]
    public class FighterTypeSettings
    {
        public FighterType type;
        [ColorUsage(true, true)] public Color overlayColor = Color.white;
        public GameObject attackPrefab;
    }

    [Header("References")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private EnemyAttack enemyAttack;

    [Header("Fighter Type Settings")]
    [SerializeField] private FighterTypeSettings[] typeSettings;

    private Material targetMaterial;

    private void Awake()
    {
        targetMaterial = targetRenderer.material;
    }

    public void SetFighterType(FighterType type)
    {
        foreach (FighterTypeSettings settings in typeSettings)
        {
            if (settings.type != type)
                continue;

            targetMaterial.SetColor(
                "_OverlayColor",
                settings.overlayColor
            );

            enemyAttack.SetAttackPrefab(settings.attackPrefab);

            return;
        }

        Debug.LogWarning(
            $"No settings found for fighter type: {type}",
            this
        );
    }
}
