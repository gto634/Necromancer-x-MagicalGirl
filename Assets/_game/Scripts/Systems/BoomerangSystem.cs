using System;
using UnityEngine;

public enum BoomerangState
{
    Idle,
    Thrown,
    Returning
}

public class BoomerangSystem : MonoBehaviour
{
    [SerializeField] Rigidbody _rb;

    [SerializeField, Min(0f)] float _throwSpeed = 12f;
    [SerializeField, Min(0f)] float _throwDuration = 0.5f;
    [SerializeField, Min(0f)] float _returnSpeed = 8f;
    [SerializeField, Min(0f)] float _returnAcceleration = 10f;
    [SerializeField, Min(0f)] float _maxReturnSpeed = 20f;
    [SerializeField, Min(0f)] float _catchDistance = 0.4f;
    [SerializeField] bool _orbit;
    [SerializeField, Min(0f)] float _turnSpeed = 180f;

    [SerializeField, ReadOnly] BoomerangState _state;
    [SerializeField, ReadOnly] float _throwTimer;
    [SerializeField, ReadOnly] float _currentSpeed;

    Transform _returnPoint;
    Vector3 _direction;

    public BoomerangState State => _state;
    public event Action ReturnStarted;
    public event Action Completed;

    public void Throw(Transform returnPoint)
    {
        _returnPoint = returnPoint;
        _direction = _rb.transform.forward;
        _throwTimer = _throwDuration;
        _currentSpeed = Mathf.Min(_returnSpeed, _maxReturnSpeed);
        _state = BoomerangState.Thrown;
    }

    void FixedUpdate()
    {
        if (_state == BoomerangState.Idle || _returnPoint == null) return;

        float deltaTime = Time.fixedDeltaTime;
        Vector3 position = _rb.position;

        if (_state == BoomerangState.Thrown)
        {
            float throwTime = Mathf.Min(_throwTimer, deltaTime);
            position += _direction * _throwSpeed * throwTime;
            _throwTimer = Mathf.Max(0f, _throwTimer - throwTime);
            deltaTime -= throwTime;

            if (_throwTimer == 0f)
            {
                _state = BoomerangState.Returning;
                ReturnStarted?.Invoke();
                if (!isActiveAndEnabled) return;
            }
        }

        if (_state == BoomerangState.Returning)
        {
            Vector3 target = _returnPoint.position;
            Vector3 offset = target - position;
            if (offset.sqrMagnitude <= _catchDistance * _catchDistance)
            {
                Complete();
                return;
            }

            _currentSpeed = Mathf.Min(_maxReturnSpeed, _currentSpeed + _returnAcceleration * deltaTime);
            Vector3 desiredDirection = offset.normalized;

            if (_orbit)
            {
                Quaternion rotation = Quaternion.RotateTowards(
                    Quaternion.LookRotation(_direction, Vector3.up),
                    Quaternion.LookRotation(desiredDirection, Vector3.up),
                    _turnSpeed * deltaTime);
                _direction = rotation * Vector3.forward;
            }
            else
            {
                _direction = desiredDirection;
            }

            Vector3 nextPosition = _orbit
                ? position + _direction * _currentSpeed * deltaTime
                : Vector3.MoveTowards(position, target, _currentSpeed * deltaTime);

            Vector3 movement = nextPosition - position;
            float progress = movement.sqrMagnitude > 0f
                ? Mathf.Clamp01(Vector3.Dot(offset, movement) / movement.sqrMagnitude)
                : 0f;
            Vector3 closestPosition = position + movement * progress;
            if ((target - closestPosition).sqrMagnitude <= _catchDistance * _catchDistance)
            {
                Complete();
                return;
            }

            position = nextPosition;
        }

        _rb.MovePosition(position);
        _rb.MoveRotation(Quaternion.LookRotation(_direction, Vector3.up));
    }

    void Complete()
    {
        _state = BoomerangState.Idle;
        Completed?.Invoke();
    }

    void OnDisable()
    {
        _state = BoomerangState.Idle;
        _returnPoint = null;
    }
}
