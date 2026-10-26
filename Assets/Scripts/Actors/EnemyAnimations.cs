using UnityEngine;

[CreateAssetMenu(menuName = "Actors/Enemy Animations")]
public class EnemyAnimations : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Order: F, FR, R, BR, B, BL, L, FL")]
    public AnimationClip[] movementClips = new AnimationClip[8];
    public AnimationClip idleClip;
    public AnimationClip turnRightClip;
    public AnimationClip turnLeftClip;

    [Header("Combat")]
    public AnimationClip[] hitReactions;
    public AnimationClip[] koReactions;
}
