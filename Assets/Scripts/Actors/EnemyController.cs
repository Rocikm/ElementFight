using System;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private Transform eyes;
    [SerializeField] private Transform torso;
    [SerializeField] private float additionalRotation;
    [SerializeField] private float groundHeight = 0f;
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform player;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyAttack attack;
    private float currentHealth;
    private float currentStamina;
    private Vector3 desiredDirection;
    private float timer = 0;
    private float attackTimer = 0;
    private State currentState;

    public event Action OnDefeated;

    void Start()
    {
        currentHealth = enemyData.maxHealth;
        currentStamina = enemyData.maxStamina;
    }
    

    void Update()
    {
        if (currentState == State.defeated)
            return;

        timer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;

        if (timer <= 0)
        {
            currentState = State.free;

            ExecuteMovement();

            if (currentState == State.free && IsReadyToAttack())
                ExecuteAttack();
        }
    }

    public void Revive(Vector3 position)
    {
        currentHealth = enemyData.maxHealth;
        transform.position = position;
        currentState = State.free;
    }

    void LateUpdate()
    {
        if (!(currentState == State.free || currentState == State.walking || currentState == State.turning))
            return;

        Vector3 toPlayer = player.position - transform.position;
        Vector3 toPlayerH = new Vector3(toPlayer.x, 0, toPlayer.z);

        torso.forward = Quaternion.Euler(0, additionalRotation, 0) * toPlayerH.normalized;
        eyes.forward = toPlayer.normalized;

    }

    public void TakeDamage(ElementData elementData)
    {
        if (currentState == State.defeated)
            return;

        currentHealth = Mathf.Max(0, currentHealth - elementData.baseDamage);
        
        attack.CancelAttack();

        float time;
        
        if (currentHealth <= 0.001f)
        {
            movement.PlayHitReaction(ReactionType.Knockout, out time);
            currentState = State.defeated;
            OnDefeated?.Invoke();
            return;
        }

        movement.PlayHitReaction(ReactionType.Hit, out time);
        currentState = State.staggered;
        timer = time;
    }

    private void ResetHeight()
    {
        transform.position = 
            new Vector3(transform.position.x, groundHeight, transform.position.z);
    }

    private void ExecuteMovement()
    {
        ResetHeight();

        Vector3 toPlayer = player.position - transform.position;
        float toPlayerDistance = toPlayer.magnitude;

        float angle = movement.GetAngleFromForward(toPlayer.normalized);
        if (angle > enemyData.minTurnAngle)
        {
            movement.Turn(true);
            timer = enemyData.turnTime;
            currentState = State.turning;
            return;
        }
        else if (angle < -enemyData.minTurnAngle)
        {
            movement.Turn(false);
            timer = enemyData.turnTime;
            currentState = State.turning;
            return;
        }
        
        desiredDirection = Vector3.zero;
        
        if (toPlayerDistance < enemyData.desiredDistanceMin)
        {
            desiredDirection = -toPlayer;
            currentState = State.walking;
        }
        else if (toPlayerDistance > enemyData.desiredDistanceMax)
        {
            desiredDirection = toPlayer;
            currentState = State.walking;
        }

        desiredDirection.y = 0;
        desiredDirection = desiredDirection.normalized;
        
        movement.PlayMovement(desiredDirection);
        timer = enemyData.movementTime;
    }

    private void ExecuteAttack()
    {
        ResetHeight();

        if (attack.PlayRandomAttack(player, out float time))
        {
            currentState = State.attacking;
            timer = time;
            // transform.rotation = Quaternion.Euler(toPlayer);
        }
        else
            timer = 0;

        attackTimer = enemyData.attackTime;
    }

    private bool IsReadyToAttack()
    {
        if (attackTimer > 0)
            return false;
        
        Vector3 toPlayer = player.position - transform.position;
        float toPlayerDistance = toPlayer.magnitude;

        if (toPlayerDistance < enemyData.desiredDistanceMin ||
            toPlayerDistance > enemyData.desiredDistanceMax)
            return false;
        
        return true;
    }

    private enum State
    {
        free,
        walking,
        turning,
        attacking,
        blocking,
        staggered,
        defeated
    }
}
