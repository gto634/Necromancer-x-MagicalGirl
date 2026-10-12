using UnityEngine;
using UnityEngine.Events;

public class Feedback_NEC : MonoBehaviour
{
    [SerializeField] Brain_NEC _brain;
    [SerializeField] Animator _animator;
    [SerializeField] Attack2_NEC _attack2;
    [SerializeField] AttackSystem _attackSystem2;

    [SerializeField] UnityEvent _onAttack1Started = new UnityEvent();
    [SerializeField] UnityEvent _onAttack1Stoped = new UnityEvent();
    [SerializeField] UnityEvent _onAttack2Started = new UnityEvent();
    [SerializeField] UnityEvent _onAttack2Stoped = new UnityEvent();
    [SerializeField] UnityEvent _onAttack2HitboxActivated = new UnityEvent();
    [SerializeField] UnityEvent _onAttack2HitboxDesactivated = new UnityEvent();
    [SerializeField] UnityEvent _onAttack2ChargeStarted = new UnityEvent();
    [SerializeField] UnityEvent _onAttack2ChargeStoped = new UnityEvent();
    [SerializeField] UnityEvent _onAttack2FullyCharged = new UnityEvent();

    bool _isAttacking1;
    bool _isAttacking2;
    bool _hasActiveHitbox;
    bool _wasCharging2;

    void OnEnable()
    {
        _isAttacking2 = _attack2 != null && _attack2.IsAttacking;
        if (_attack2 != null) _attack2.FullyCharged += OnAttack2FullyCharged;
        if (_brain != null)
        {
            _brain.OnAttack1Started += OnAttack1Started;
            _brain.OnAttack1Stoped += OnAttack1Stoped;
            _brain.OnAttack2Started += OnAttack2Started;
            _brain.OnAttack2Stoped += OnAttack2Stoped;
        }

        if (_attackSystem2 != null)
        {
            _attackSystem2.Activated += UpdateHitboxFeedback;
            _attackSystem2.Desactivated += UpdateHitboxFeedback;
        }
        UpdateHitboxFeedback();
    }

    void OnDisable()
    {
        if (_attack2 != null) _attack2.FullyCharged -= OnAttack2FullyCharged;
        if (_brain != null)
        {
            _brain.OnAttack1Started -= OnAttack1Started;
            _brain.OnAttack1Stoped -= OnAttack1Stoped;
            _brain.OnAttack2Started -= OnAttack2Started;
            _brain.OnAttack2Stoped -= OnAttack2Stoped;
        }

        if (_attackSystem2 != null)
        {
            _attackSystem2.Activated -= UpdateHitboxFeedback;
            _attackSystem2.Desactivated -= UpdateHitboxFeedback;
        }

        if (_hasActiveHitbox) _onAttack2HitboxDesactivated.Invoke();
        if (_wasCharging2) _onAttack2ChargeStoped.Invoke();
        _hasActiveHitbox = false;
        _wasCharging2 = false;
        _isAttacking1 = false;
        _isAttacking2 = false;
    }

    void Update()
    {
        UpdateChargeFeedback();
        if (_animator == null) return;

        _animator.SetBool("isAttacking1", _isAttacking1);
        _animator.SetBool("isAttacking2", _isAttacking2);
        _animator.SetBool("isCharging2", _isAttacking2 && _attack2 != null
            && (_attack2.IsCharging || _attack2.IsTeleporting));
    }

    void UpdateChargeFeedback()
    {
        bool isCharging = _attack2 != null && _attack2.IsCharging;
        if (isCharging != _wasCharging2)
        {
            _wasCharging2 = isCharging;
            if (isCharging) _onAttack2ChargeStarted.Invoke();
            else _onAttack2ChargeStoped.Invoke();
        }

    }

    void OnAttack2FullyCharged() => _onAttack2FullyCharged.Invoke();

    public void StartAttack1()
    {
        _isAttacking1 = true;
    }

    public void StopAttack1()
    {
        _isAttacking1 = false;
    }

    public void StartAttack2()
    {
        _isAttacking1 = false;
        _isAttacking2 = true;
    }

    public void StopAttack2()
    {
        _isAttacking2 = false;
    }

    void UpdateHitboxFeedback()
    {
        bool hasActiveHitbox = false;
        if (_attackSystem2 != null && _attackSystem2.isActiveAndEnabled)
        {
            foreach (HitboxSettings settings in _attackSystem2.Hitboxes)
            {
                if (settings != null && settings.hitbox != null
                    && settings.hitbox.isActiveAndEnabled && settings.hitbox.Collider != null
                    && settings.hitbox.Collider.enabled)
                {
                    hasActiveHitbox = true;
                    break;
                }
            }
        }

        if (_hasActiveHitbox == hasActiveHitbox) return;
        _hasActiveHitbox = hasActiveHitbox;

        if (hasActiveHitbox) _onAttack2HitboxActivated.Invoke();
        else _onAttack2HitboxDesactivated.Invoke();
    }

    void OnAttack1Started() => _onAttack1Started.Invoke();
    void OnAttack1Stoped() => _onAttack1Stoped.Invoke();
    void OnAttack2Started() => _onAttack2Started.Invoke();
    void OnAttack2Stoped() => _onAttack2Stoped.Invoke();
}
