using System;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;

public class ProjectileMovement : MonoBehaviour
{
    private float speed;
    private float targetTime;
    private Vector3 direction;
    private Vector3 targetPosition;
    private Vector3 initialPosition;
    private Vector3 velocity;
    private float radius;
    private float timer;
    private bool isCurveEnabled;
    private bool alignDirection;

    private delegate void UpdateMovement(float deltaTime);
    
    // Movement strategy
    private UpdateMovement updateMovement;

    private void Update()
    {
        updateMovement?.Invoke(Time.deltaTime);
    }

    public void Stop()
    {
        updateMovement = null;
    }

    public void Launch(Vector3 direction, ElementData data)
    {
        this.direction = direction;
        speed = data.speed;
        
        if (data.alignForwardDirection)
            transform.forward = direction;
        
        updateMovement = LinearMovement;
    }

    public void MoveToPosition(Vector3 target, ElementData data)
    {
        initialPosition = transform.position;
        targetPosition = target;
        targetTime = data.preparationTime;

        if (targetTime <= 0)
        {
            transform.position = target;
            updateMovement = null;
            return;
        }

        timer = 0;

        updateMovement = TargetMovement;
    }

    public void LaunchCurved(Vector3 initialDirection, Vector3 target, float radius, ElementData data)
    {
        initialPosition = transform.position;
        velocity = initialDirection * data.speed;
        targetPosition = target;
        this.radius = radius;
        isCurveEnabled = true;
        alignDirection = data.alignForwardDirection;

        updateMovement = CurvedMovement;
    }

    private void LinearMovement(float deltaTime)
    {
        transform.position += direction * speed * deltaTime;
    }

    private void TargetMovement(float deltaTime)
    {
        timer += deltaTime;

        float t = Mathf.Clamp01(timer / targetTime);

        transform.position = Vector3.Lerp(
            initialPosition, targetPosition, t);

        if (timer > targetTime)
            updateMovement = null;
    }

    private void CurvedMovement(float deltaTime)
    {
        if (isCurveEnabled)
        {
            Vector3 anchor = initialPosition + (targetPosition - initialPosition).normalized * radius;
            Vector3 targetDirection = (targetPosition - transform.position).normalized;
            float aligned = Vector3.Dot(velocity.normalized, targetDirection);
            float accelerationAmount = 1 - Mathf.SmoothStep(0.5f, 0.85f, aligned);
            Vector3 acceleration = 120 * accelerationAmount * (anchor - transform.position).normalized;
            velocity += acceleration * deltaTime;

            if (aligned > 0.999f)
                isCurveEnabled = false;
        }

        if (alignDirection)
            transform.forward = velocity.normalized;

        transform.position += velocity * deltaTime;
    }
}
