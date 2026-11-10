using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AttackBinding[] attackBindings;
    [SerializeField] private EnemyAnimations animations;
    [SerializeField] private Fighter fighter;
    private AttackBinding currentBinding;
    private Transform target;
    private bool isAttacking = false;
    private bool hasLaunched = false;
    private float timer = 0;
    private int animationLayer;
    private int lastAttack = -1;

    void Awake()
    {
        animationLayer = animator.GetLayerIndex("Upper");
    }

    void Update()
    {
        if (isAttacking)
        {
            timer += Time.deltaTime;

            if (!hasLaunched && timer >= currentBinding.timeAfterSpawn)
            {
                Vector3 attackDirection = 
                    (target.position - currentBinding.origin.position).normalized;

                Projectile.SpawnProjectile(
                    currentBinding.prefab,
                    gameObject,
                    currentBinding.origin.position,
                    attackDirection
                );
                hasLaunched = true;
            }
            if (timer >= currentBinding.timeAfterSpawn)
            {
                isAttacking = false;
                // animator.SetLayerWeight(animationLayer, 1);
            }
        }
    }

    public void CancelAttack()
    {
        isAttacking = false;
    }

    public bool PlayRandomAttack(Transform target, out float animationLength)
    {
        animationLength = 0;

        if (isAttacking)
            return false;

        int index = Random.Range(0, attackBindings.Length);
        // int index = (lastAttack + 1) % attackBindings.Length;
        AttackBinding binding = attackBindings[index];

        if (!fighter.TryUseStamina(binding.prefab.GetComponent<Projectile>().elementData.staminaCost))
            return false;

        lastAttack = index;
        
        currentBinding = binding;
        this.target = target;

        timer = 0;
        animationLength = currentBinding.animation.length;

        transform.RotateAround(transform.position, Vector3.up, currentBinding.startRotation);

        animator.SetLayerWeight(animationLayer, 0);
        animator.CrossFadeInFixedTime(currentBinding.animation.name, 0.15f);

        isAttacking = true;
        hasLaunched = false;
        
        return true;
    }

    [System.Serializable]
    private class AttackBinding
    {
        public float timeAfterSpawn;
        public float startRotation;
        public AnimationClip animation;
        public Transform origin;
        public GameObject prefab;
    }
}

public enum ReactionType
{
    Hit,
    Knockout
}
