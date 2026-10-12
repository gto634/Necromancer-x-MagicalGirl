using System;
using UnityEngine;

public class Attack2_NEC : MonoBehaviour
{
    [SerializeField] AttackSystem _attackSystem;
    [SerializeField] TeleportationSystem _teleportationSystem;
    [SerializeField] MoveSystem _moveSystem;
    [SerializeField] LookSystem _lookSystem;

    [SerializeField, Min(0f)] float _minTeleportationDistance = 1f;
    [SerializeField, Min(0f)] float _chargeDuration = 1f;
    [SerializeField, Range(0f, 1f)] float _chargeMovementMultiplier = 0.5f;
    [SerializeField, ReadOnly] float _chargeTimer;
    [SerializeField, ReadOnly] float _teleportationDistance;

    [SerializeField, Min(0f)] float _teleportationDelay = 0.25f;
    [SerializeField, ReadOnly] float _teleportationTimer;

    bool _isCharging;
    bool _isAttacking;
    bool _isTeleporting;
    bool _wasMoveEnabled;
    bool _wasLookEnabled;
    bool _hasSavedMoveSpeed;
    float _moveSpeedBeforeCharge;
    bool _wasFullyCharged;

    public bool IsCharging => _isCharging;
    public bool IsTeleporting => _isTeleporting;
    public float TeleportationDistance => _teleportationDistance;
    public float ChargeRatio => _chargeDuration > 0f ? Mathf.Clamp01(_chargeTimer / _chargeDuration) : 1f;
    public bool IsAttacking => _isAttacking;
    public bool CanAttack => isActiveAndEnabled && !IsAttacking && _attackSystem.CanAttack;

    public event Action Finished;
    public event Action FullyCharged;

    public void StartAttack()
    {
        if (!CanAttack) return;

        _isAttacking = true;
        _isCharging = true;
        _chargeTimer = 0f;
        _wasFullyCharged = false;
        _moveSpeedBeforeCharge = _moveSystem.Speed;
        _hasSavedMoveSpeed = true;
        _moveSystem.Speed = _moveSpeedBeforeCharge * Mathf.Clamp01(_chargeMovementMultiplier);
        UpdateChargeDistance();
    }

    public void ReleaseAttack()
    {
        if (!isActiveAndEnabled || !IsCharging) return;

        _isCharging = false;
        RestoreMoveSpeed();
        _wasMoveEnabled = _moveSystem.enabled;
        _wasLookEnabled = _lookSystem.enabled;
        _moveSystem.enabled = false;
        _lookSystem.enabled = false;

        _isTeleporting = true;
        _teleportationTimer = _teleportationDelay;
    }

    void Update()
    {
        if (IsCharging)
        {
            _chargeTimer = Mathf.Min(_chargeDuration, _chargeTimer + Time.deltaTime);
            UpdateChargeDistance();
            return;
        }

        if (!_isTeleporting)
        {
            if (IsAttacking && !_attackSystem.IsAttacking) EndAttack();
            return;
        }

        _teleportationTimer = Mathf.Max(0f, _teleportationTimer - Time.deltaTime);
        if (_teleportationTimer > 0f) return;

        _teleportationSystem.Teleport(_teleportationDistance);
        _attackSystem.Attack(ignoreDelay: true);
        EndTeleportation();
    }

    void UpdateChargeDistance()
    {
        float maxDistance = Mathf.Max(0f, _teleportationSystem.Distance);
        float minDistance = Mathf.Clamp(_minTeleportationDistance, 0f, maxDistance);
        _teleportationDistance = Mathf.Lerp(minDistance, maxDistance, ChargeRatio);

        if (_wasFullyCharged || ChargeRatio < 1f) return;
        _wasFullyCharged = true;
        FullyCharged?.Invoke();
    }

    void EndTeleportation()
    {
        _isTeleporting = false;
        _teleportationTimer = 0f;

        if (_moveSystem != null) _moveSystem.enabled = _wasMoveEnabled;
        if (_lookSystem != null) _lookSystem.enabled = _wasLookEnabled;
    }

    public void StopAttack()
    {
        _isCharging = false;
        RestoreMoveSpeed();
        _chargeTimer = 0f;
        if (_isTeleporting) EndTeleportation();
        if (_attackSystem != null) _attackSystem.StopAttack();
        EndAttack();
    }

    void RestoreMoveSpeed()
    {
        if (!_hasSavedMoveSpeed) return;

        if (_moveSystem != null) _moveSystem.Speed = _moveSpeedBeforeCharge;
        _hasSavedMoveSpeed = false;
    }

    void EndAttack()
    {
        if (!IsAttacking) return;

        _isAttacking = false;
        Finished?.Invoke();
    }

    void OnDisable()
    {
        StopAttack();
    }
}
