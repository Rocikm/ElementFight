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
    private Rigidbody rb;
    private Transform trackingTarget;
    private bool aimAssistEnabled;
    private AimAssistSettings aimAssistSettings
        = new AimAssistSettings { alignmentThreshold = 0.9f, turnSpeed = 2f };

    private delegate void UpdateMovement(float deltaTime);
    
    // Movement strategy
    private UpdateMovement updateMovement;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        updateMovement?.Invoke(Time.deltaTime);
        ApplyAimAssist(Time.deltaTime);
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

        velocity = direction * speed;
        
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

    public void TogglePhysics(bool enable)
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on " + gameObject.name);
            return;
        }

        if (enable)
        {
            updateMovement = null;
            rb.isKinematic = false;
            rb.linearVelocity = velocity;
            rb.useGravity = true;
        }
        else
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void SetAimAssist(Transform target, bool enabled = true)
    {
        trackingTarget = target;
        aimAssistEnabled = enabled;
    }

    public void SetAimAssistSettings(AimAssistSettings settings)
    {
        aimAssistSettings = settings;
    }

    private void LinearMovement(float deltaTime)
    {
        transform.position += velocity * deltaTime;
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
            float accelerationAmount = 1 - Mathf.SmoothStep(0.5f, 0.95f, aligned);
            Vector3 acceleration = 220 * accelerationAmount * (anchor - transform.position).normalized;
            velocity += acceleration * deltaTime;

            if (aligned > 0.999f || aligned < -0.75f)
                isCurveEnabled = false;
        }

        if (alignDirection)
            transform.forward = velocity.normalized;

        transform.position += velocity * deltaTime;
    }

    private void ApplyAimAssist(float deltaTime)
    {
        if (!aimAssistEnabled || trackingTarget == null)
            return;

        if (velocity.sqrMagnitude < 0.001f)
            return;

        Vector3 toTarget = trackingTarget.position - transform.position;

        if (toTarget.sqrMagnitude < 0.001f)
            return;

        Vector3 currentDirection = velocity.normalized;
        Vector3 targetDirection = toTarget.normalized;

        // Only assist shots that are already mostly on target.
        float alignment = Vector3.Dot(currentDirection, targetDirection);

        if (alignment < aimAssistSettings.alignmentThreshold)
            return;

        // Gradually steer toward the target without changing speed.
        Vector3 assistedDirection = Vector3.RotateTowards(
            currentDirection, targetDirection, aimAssistSettings.turnSpeed * deltaTime, 0f);

        velocity = assistedDirection * velocity.magnitude;
    }
}

[System.Serializable]
public class AimAssistSettings
{
    public float alignmentThreshold;
    public float turnSpeed;
}
