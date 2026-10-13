using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

public class GestureInput : MonoBehaviour
{
    [SerializeField] private InputActionProperty activationAction;
    [SerializeField] private float minActivationDistance;
    [SerializeField] private Transform referenceFrame;
    [SerializeField] private bool autoActivation;
    [SerializeField] private float autoActivationDistance;

    private Vector3 startPosition;
    private Vector3 startDirection;
    private bool isTrackingGesture;
    private bool hasPerformedGesture;
    private Vector3 previousHandPosition;

    public event EventHandler<GestureEventArgs> OnGesturePerformed;
    public Vector3 velocity { get; private set; }

    void Start()
    {
        isTrackingGesture = false;
        hasPerformedGesture = false;
        previousHandPosition = transform.position;
    }

    void Update()
    {
        float activation = activationAction.action.ReadValue<float>();

        bool gesturePerformed;

        if (autoActivation)
            gesturePerformed = DetectAuto(activation);
        else
            gesturePerformed = DetectManual(activation);

        if (gesturePerformed == true)
        {
            Vector3 difference = referenceFrame.InverseTransformDirection(
                transform.position - startPosition);

            GestureEventArgs eventArgs = new GestureEventArgs(
                EvaluateGesture(difference), startPosition, startDirection,
                transform.position, transform.forward);

            OnGesturePerformed?.Invoke(this, eventArgs);            
        }

        UpdateHandVelocity();
    }

    private bool DetectAuto(float activation)
    {
        if (activation < 0.5)
        {
            isTrackingGesture = false;
            hasPerformedGesture = false;
            return false;
        }

        float distance = (startPosition - transform.position).magnitude;

        if (activation >= 0.5f && !isTrackingGesture)
        {
            startPosition = transform.position;
            startDirection = transform.forward;
            isTrackingGesture = true;
        }
        else if (activation >= 0.5 && !hasPerformedGesture && distance > autoActivationDistance)
        {
            isTrackingGesture = false;
            hasPerformedGesture = true;
            return true;
        }

        return false;
    }

    private bool DetectManual(float activation)
    {
        if (activation >= 0.5f && !isTrackingGesture)
        {
            startPosition = transform.position;
            startDirection = transform.forward;
            isTrackingGesture = true;
        }
        else if (activation < 0.5 && autoActivation)
        {
            isTrackingGesture = false;
        }
        else if (activation < 0.5f && isTrackingGesture)
        {
            isTrackingGesture = false;
            return true;
        }

        return false;
    }

    private Gesture EvaluateGesture(Vector3 direction)
    {
        if (direction.sqrMagnitude < minActivationDistance * minActivationDistance)
            return Gesture.Press;

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y) &&
            Mathf.Abs(direction.x) >= Mathf.Abs(direction.z))
        {
            return direction.x >= 0 ? Gesture.Right : Gesture.Left;
        }

        if (Mathf.Abs(direction.y) >= Mathf.Abs(direction.z))
        {
            return direction.y >= 0 ? Gesture.Up : Gesture.Down;
        }

        return direction.z >= 0 ? Gesture.Forward : Gesture.Back;
    }

    private void UpdateHandVelocity()
    {
        velocity = transform.position - previousHandPosition;
        previousHandPosition = transform.position;
    }

    public class GestureEventArgs : EventArgs
    {
        public Gesture gesture;
        public Vector3 startPosition;
        public Vector3 startDirection;
        public Vector3 endPosition;
        public Vector3 endDirection;

        public GestureEventArgs(
            Gesture gesture, Vector3 startPosition, Vector3 startDirection,
            Vector3 endPosition, Vector3 endDirection)
        {
            this.gesture = gesture;
            this.startPosition = startPosition;
            this.startDirection = startDirection;
            this.endPosition = endPosition;
            this.endDirection = endDirection;
        }
    }
}

public enum Gesture
{
    Forward,
    Back,
    Left,
    Right,
    Up,
    Down,
    Press
}
