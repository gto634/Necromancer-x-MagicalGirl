using System;
using UnityEngine;

public class OrbitSystem : MonoBehaviour
{
    [SerializeField] Transform _rootTransform;
    [SerializeField, Min(0f)] float _startRadius = 2f;
    [SerializeField, Min(0f)] float _endRadius = 5f;
    [SerializeField, Min(0f)] float _startScale = 1f;
    [SerializeField, Min(0f)] float _endScale = 1f;
    [SerializeField, Min(0f)] float _targetAngle = 360f;
    [SerializeField, Min(0f)] float _speed = 180f;
    [SerializeField, Min(0f)] float _acceleration = 180f;
    [SerializeField] float _height = 1f;

    [SerializeField, ReadOnly] Transform _center;
    [SerializeField, ReadOnly] float _angle;
    [SerializeField, ReadOnly] float _rotationAngle;
    [SerializeField, ReadOnly] float _currentSpeed;

    Vector3 _initialScale;

    public float TargetAngle => _targetAngle;

    public float Duration
    {
        get
        {
            if (_acceleration > 0f)
            {
                float doubleAngle = 2f * _targetAngle;
                return doubleAngle / (_speed + Mathf.Sqrt(_speed * _speed + doubleAngle * _acceleration));
            }

            return _speed > 0f ? _targetAngle / _speed : 0f;
        }
    }

    public bool IsOrbiting { get; private set; }

    public event Action<float> Rotated;
    public event Action Completed;

    void Reset()
    {
        _rootTransform = transform.root;
    }

    void Awake()
    {
        if (_rootTransform == null)
        {
            _rootTransform = transform.root;
        }

        _initialScale = _rootTransform.localScale;
    }

    public void Orbit(Transform center)
    {
        _center = center;
        _rotationAngle = 0f;
        _currentSpeed = _speed;
        IsOrbiting = _center != null && Duration > 0f;
        if (!IsOrbiting) return;

        Vector3 direction = _rootTransform.position - _center.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = _center.forward;
        }

        _angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
        UpdatePosition();
    }

    void LateUpdate()
    {
        if (!IsOrbiting || _center == null) return;

        float deltaTime = Time.deltaTime;
        float rotation = _currentSpeed * deltaTime + 0.5f * _acceleration * deltaTime * deltaTime;
        _currentSpeed += _acceleration * deltaTime;
        rotation = Mathf.Min(rotation, _targetAngle - _rotationAngle);
        _rotationAngle += rotation;
        _angle = Mathf.Repeat(_angle + rotation, 360f);
        UpdatePosition();

        Rotated?.Invoke(rotation);

        if (_rotationAngle >= _targetAngle)
        {
            IsOrbiting = false;
            Completed?.Invoke();
        }
    }

    void UpdatePosition()
    {
        float progress = _targetAngle > 0f ? Mathf.Clamp01(_rotationAngle / _targetAngle) : 1f;
        float currentRadius = Mathf.Lerp(_startRadius, _endRadius, progress);
        float currentScale = Mathf.Lerp(_startScale, _endScale, progress);

        Vector3 offset = Quaternion.AngleAxis(_angle, Vector3.up) * (Vector3.forward * currentRadius);

        _rootTransform.position = _center.position + offset + Vector3.up * _height;
        _rootTransform.forward = Quaternion.AngleAxis(_angle, Vector3.up) * Vector3.forward;
        _rootTransform.localScale = _initialScale * currentScale;
    }
}
