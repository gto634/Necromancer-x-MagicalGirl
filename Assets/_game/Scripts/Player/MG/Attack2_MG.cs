using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Attack2_MG : MonoBehaviour
{
    [FormerlySerializedAs("_shootSystem")]
    [SerializeField] ShootSystem _shootSystemStart;
    [SerializeField] ShootSystem _shootSystemFollow;
    [SerializeField] LookSystem _lookSystem;

    [SerializeField, Min(0f)] float _bufferDuration = 0.5f;
    [SerializeField, ReadOnly] float _bufferTimer;

    Brain_Proj2_MG _projectile;
    ParticleSystem _pendingTrail;
    bool _wasLookEnabled;
    bool _isAttacking;

    public bool IsAttacking => _isAttacking;
    public bool CanAttack => isActiveAndEnabled && !IsAttacking
        && _shootSystemStart != null && _shootSystemStart.isActiveAndEnabled && _shootSystemStart.CanShoot;

    public event Action Finished;

    void OnEnable()
    {
        if (_shootSystemStart != null) _shootSystemStart.OnShoot += OnStartShoot;
        if (_shootSystemFollow != null) _shootSystemFollow.OnShoot += OnFollowShoot;
    }

    void OnDisable()
    {
        if (_shootSystemStart != null) _shootSystemStart.OnShoot -= OnStartShoot;
        if (_shootSystemFollow != null) _shootSystemFollow.OnShoot -= OnFollowShoot;

        StopAttack();
    }

    void Update()
    {
        _bufferTimer = Mathf.Max(0f, _bufferTimer - Time.deltaTime);
    }

    public void StartAttack()
    {
        if (IsAttacking)
        {
            if (isActiveAndEnabled && _shootSystemFollow != null && _shootSystemFollow.isActiveAndEnabled)
                _bufferTimer = _bufferDuration;
            return;
        }

        if (!CanAttack) return;

        _isAttacking = true;
        _bufferTimer = 0f;
        _wasLookEnabled = _lookSystem != null && _lookSystem.enabled;
        _shootSystemStart.Shoot();
    }

    public void CancelAttack()
    {
        if (_shootSystemStart == null || !_shootSystemStart.HasPendingShoot) return;

        _shootSystemStart.CancelShoot();
        if (_projectile == null) FinishAttack();
    }

    public void StopAttack()
    {
        if (_shootSystemStart != null) _shootSystemStart.CancelShoot();
        if (_shootSystemFollow != null) _shootSystemFollow.CancelShoot();

        GameObject instance = _projectile != null ? _projectile.transform.root.gameObject : null;
        DetachProjectile();
        FinishAttack();
        if (instance != null) Destroy(instance);
    }

    void OnStartShoot()
    {
        OnShoot(_shootSystemStart);
    }

    void OnFollowShoot()
    {
        OnShoot(_shootSystemFollow);
    }

    void OnShoot(ShootSystem shootSystem)
    {
        if (!IsAttacking) return;

        _projectile = shootSystem.LastProjectile.GetComponentInChildren<Brain_Proj2_MG>();
        if (_projectile == null)
        {
            FinishAttack();
            return;
        }

        if (_pendingTrail != null)
        {
            ParticleTrailFeedback trail = _projectile.transform.root.GetComponentInChildren<ParticleTrailFeedback>();
            if (trail != null) trail.AttachParticles(_pendingTrail);
            else ParticleTrailFeedback.StopParticles(_pendingTrail);
            _pendingTrail = null;
        }

        if (_lookSystem != null) _lookSystem.enabled = false;

        _projectile.Rotated += Rotate;
        _projectile.Finished += EndAttack;
    }

    void Rotate(float angle)
    {
        if (_lookSystem != null) _lookSystem.Rotate(angle);
    }

    void EndAttack()
    {
        ParticleTrailFeedback trail = _projectile != null
            ? _projectile.transform.root.GetComponentInChildren<ParticleTrailFeedback>() : null;
        DetachProjectile();

        if (_bufferTimer > 0f && isActiveAndEnabled && _shootSystemFollow != null
            && _shootSystemFollow.isActiveAndEnabled && _shootSystemFollow.CanShoot)
        {
            // Keep the live emitter and its distance accumulator across combo projectiles.
            _pendingTrail = trail != null ? trail.DetachParticles() : null;
            _bufferTimer = 0f;
            _shootSystemFollow.Shoot();
            return;
        }

        FinishAttack();
    }

    void DetachProjectile()
    {
        if (_projectile != null)
        {
            _projectile.Rotated -= Rotate;
            _projectile.Finished -= EndAttack;
        }

        _projectile = null;
    }

    void FinishAttack()
    {
        ParticleTrailFeedback.StopParticles(_pendingTrail);
        _pendingTrail = null;
        if (!IsAttacking) return;

        _isAttacking = false;
        _bufferTimer = 0f;
        if (_lookSystem != null) _lookSystem.enabled = _wasLookEnabled;
        Finished?.Invoke();
    }
}
