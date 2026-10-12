using System;
using UnityEngine;

public class Brain_Proj1_NEC : MonoBehaviour
{
    [SerializeField] Projectile _projectile;
    [SerializeField] BoomerangSystem _boomerangSystem;
    [SerializeField] AttackSystem _attackSystem;
    [SerializeField, Min(0f)] float _lifeDuration = 12f;

    Transform _returnPoint;
    Transform _rootTransform;

    public event Action Finished;

    public void Initialize(Transform returnPoint)
    {
        _returnPoint = returnPoint;
    }

    void Start()
    {
        _rootTransform = _projectile.transform;
        if (_projectile.Owner == null || _returnPoint == null)
        {
            Destroy(_rootTransform.gameObject);
            return;
        }

        _boomerangSystem.ReturnStarted += OnReturnStarted;
        _boomerangSystem.Completed += OnCompleted;

        foreach (HitboxSettings settings in _attackSystem.Hitboxes)
        {
            settings.Duration = _lifeDuration;
            settings.hitbox.IgnoredRoot = _projectile.Owner;
        }

        _attackSystem.Attack();
        _boomerangSystem.Throw(_returnPoint);
    }

    void Update()
    {
        _lifeDuration = Mathf.Max(0f, _lifeDuration - Time.deltaTime);
        if (_projectile.Owner == null || _returnPoint == null || _lifeDuration == 0f)
        {
            Destroy(_rootTransform.gameObject);
        }
    }

    void OnReturnStarted()
    {
        foreach (HitboxSettings settings in _attackSystem.Hitboxes)
        {
            settings.hitbox.ResetHits();
        }
    }

    void OnCompleted()
    {
        Destroy(_rootTransform.gameObject);
    }

    void OnDestroy()
    {
        if (_boomerangSystem != null)
        {
            _boomerangSystem.ReturnStarted -= OnReturnStarted;
            _boomerangSystem.Completed -= OnCompleted;
        }

        Finished?.Invoke();
    }
}
