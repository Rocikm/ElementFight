using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform eyes;
    [SerializeField] private Transform torso;
    [SerializeField] private float additionalRotation;
    [SerializeField] private float groundHeight = 0f;
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform player;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private Fighter fighter;
    private Vector3 desiredDirection;
    private float timer = 0;
    private float attackTimer = 0;
    private State currentState;

    // public event Action OnDefeated;

    void OnEnable()
    {
        // fighter.OnStarted += OnFightStarted;
        fighter.OnDamageTaken += HandleOnDamageTaken;
        fighter.OnDefeated += HandleOnDefeated;
        currentState = State.free;
    }

    void OnDisable()
    {
        fighter.OnDamageTaken -= HandleOnDamageTaken;
        fighter.OnDefeated -= HandleOnDefeated;
    }


    void Update()
    {
        if (currentState == State.defeated || !fighter.CombatEnabled)
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

    public void OnFightStarted(Fighter fighter)
    {
        // transform.position = position;
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

    public void HandleOnDamageTaken(Fighter fighter, float amount)
    {
        if (currentState == State.defeated)
            return;

        attack.CancelAttack();

        float time;

        movement.PlayHitReaction(ReactionType.Hit, out time);
        currentState = State.staggered;
        timer = time;
    }

    public void HandleOnDefeated(Fighter fighter)
    {
        if (currentState == State.defeated)
            return;

        movement.PlayHitReaction(ReactionType.Knockout, out float time);
        currentState = State.defeated;
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
