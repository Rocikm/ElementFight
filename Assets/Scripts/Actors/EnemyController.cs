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
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallDetectionDistance = 2f;
    [SerializeField] private float backwardDetectionDistance = 1f;
    [SerializeField] private float wallDetectionRadius = 0.4f;
    [SerializeField] private float wallAvoidanceWeight = 2f;

    private readonly Vector3[] avoidanceDirections =
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right,
        (Vector3.forward + Vector3.left).normalized,
        (Vector3.forward + Vector3.right).normalized,
        (Vector3.back + Vector3.left).normalized,
        (Vector3.back + Vector3.right).normalized
    };
    private Vector3 desiredDirection;
    private float timer = 0;
    private float attackTimer = 0;
    private State currentState;
    private bool alreadyMoved = false;

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
            ResetHeight();

            currentState = State.free;

            if (!alreadyMoved && ExecuteMovement())
                return;

            if (currentState == State.free && IsReadyToAttack())
            {
                ExecuteAttack();
            }
            
            alreadyMoved = false;
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

        Vector3 toPlayer = player.position - transform.position - Vector3.up * 5;
        Vector3 toPlayerH = new Vector3(toPlayer.x, 0, toPlayer.z);

        // torso.forward = Quaternion.Euler(0, additionalRotation, 0) * toPlayerH.normalized;
        eyes.forward = toPlayer.normalized;
    }

    public void HandleOnDamageTaken(Fighter fighter, float amount)
    {
        if (currentState == State.defeated || currentState == State.attacking)
            return;

        attack.CancelAttack();
        ResetHeight();

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

    private bool ExecuteMovement()
    {
        alreadyMoved = true;

        Vector3 toPlayer = player.position - transform.position;
        float toPlayerDistance = toPlayer.magnitude;

        float angle = movement.GetAngleFromForward(toPlayer.normalized);
        if (angle > enemyData.minTurnAngle)
        {
            movement.Turn(true);
            timer = enemyData.turnTime;
            currentState = State.turning;
            alreadyMoved = false;
            return true;
        }
        else if (angle < -enemyData.minTurnAngle)
        {
            movement.Turn(false);
            timer = enemyData.turnTime;
            currentState = State.turning;
            alreadyMoved = false;
            return true;
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
        else
        {
            alreadyMoved = false;
            return false;
        }

        // Add wall avoidance
        if (desiredDirection != Vector3.zero)
        {
            Vector3 wallAvoidance = GetWallAvoidanceDirection();

            desiredDirection += wallAvoidance * wallAvoidanceWeight;
        }

        desiredDirection.y = 0;
        desiredDirection = desiredDirection.normalized;
        
        movement.PlayMovement(desiredDirection);
        timer = enemyData.movementTime;
        return true;
    }

    private Vector3 GetWallAvoidanceDirection()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        toPlayer.Normalize();

        Vector3 left = Vector3.Cross(Vector3.up, toPlayer).normalized;
        Vector3 right = -left;

        Vector3 avoidance = Vector3.zero;

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        foreach (Vector3 direction in avoidanceDirections)
        {
            if (Physics.SphereCast(
                    origin,
                    wallDetectionRadius,
                    direction,
                    out RaycastHit hit,
                    wallDetectionDistance,
                    wallLayer))
            {
                float strength = 1f - hit.distance / wallDetectionDistance;

                avoidance += -direction * strength;
            }
        }

        avoidance.y = 0f;

        return avoidance.normalized;
    }

    // private Vector3 GetWallAvoidanceDirection()
    // {
    //     Vector3 toPlayer = player.position - transform.position;
    //     toPlayer.y = 0f;
    //     toPlayer.Normalize();

    //     Vector3 left = Vector3.Cross(Vector3.up, toPlayer).normalized;
    //     Vector3 right = -left;
    //     Vector3 backward = -toPlayer;

    //     Vector3 origin = transform.position + Vector3.up * 0.5f;

    //     bool leftBlocked = Physics.SphereCast(
    //         origin, wallDetectionRadius, left,
    //         out _, wallDetectionDistance, wallLayer);

    //     bool rightBlocked = Physics.SphereCast(
    //         origin, wallDetectionRadius, right,
    //         out _, wallDetectionDistance, wallLayer);

    //     // Check whether retreating is possible
    //     bool backwardBlocked = Physics.SphereCast(
    //         origin, wallDetectionRadius, backward,
    //         out RaycastHit backwardHit, wallDetectionDistance, wallLayer);

    //     Vector3 direction = Vector3.zero;

    //     if (leftBlocked)
    //         direction = right;

    //     if (rightBlocked)
    //         direction = left;

    //     if (backwardBlocked)
    //         direction += toPlayer;

    //     return direction;
    // }

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

        if (toPlayerDistance > enemyData.desiredDistanceMax)
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
