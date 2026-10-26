using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Tooltip("Order: F, FR, R, BR, B, BL, L, FL")]
    [SerializeField] private AnimationClip[] movementClips = new AnimationClip[8];
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip turnRightClip;
    [SerializeField] private AnimationClip turnLeftClip;
    [SerializeField] private EnemyAnimations animations;

    private int animationLayer;

    void Awake()
    {
        animationLayer = animator.GetLayerIndex("Upper");
    }

    public void PlayMovement(Vector3 worldDirection)
    {
        if (worldDirection.sqrMagnitude < 0.001f)
        {
            if (idleClip != null)
                animator.CrossFadeInFixedTime(idleClip.name, 0.15f);
            return;
        }

        Vector3 localDirection =
            transform.InverseTransformDirection(worldDirection);

        // Angle relative to the enemy's forward direction.
        float angle =
            Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;

        if (angle < 0f)
            angle += 360f;

        // Divide the circle into 8 sectors of 45 degrees.
        int index = Mathf.RoundToInt(angle / 45f) % 8;

        AnimationClip clip = movementClips[index];

        if (clip != null)
            animator.CrossFadeInFixedTime(clip.name, 0.15f);
        
        animator.SetLayerWeight(animationLayer, 1);
    }

    public float GetAngleFromForward(Vector3 worldDirection)
    {
        Vector3 localDirection =
            transform.InverseTransformDirection(worldDirection);

        return Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
    }

    public void Turn(bool right)
    {
        if (right && turnRightClip != null)
            animator.CrossFadeInFixedTime(turnRightClip.name, 0.15f);
        else if (turnLeftClip != null)
            animator.CrossFadeInFixedTime(turnLeftClip.name, 0.15f);
        animator.SetLayerWeight(animationLayer, 1);
    }

    public void PlayHitReaction(ReactionType type, out float animationLength)
    {
        animationLength = 0;
        
        AnimationClip[] anims;

        switch (type)
        {
            case ReactionType.Hit:
                anims = animations.hitReactions;
                break;
            case ReactionType.Knockout:
                anims = animations.koReactions;
                break;
            default:
                anims = new AnimationClip[0];
                break;
        }
        
        if (anims.Length == 0)
            return;

        int index = Random.Range(0, anims.Length);
        AnimationClip animation = anims[index];

        animator.SetLayerWeight(animationLayer, 0);
        animator.CrossFadeInFixedTime(animation.name, 0.15f);
        animationLength = animation.length;
    }
}
