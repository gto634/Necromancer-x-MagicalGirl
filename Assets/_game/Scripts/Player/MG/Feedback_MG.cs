using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Feedback_MG : MonoBehaviour
{
    [SerializeField] Brain_MG _brain;
    [SerializeField] Animator _animator;
    [SerializeField] ShootSystem _shootSystem1;
    [SerializeField] ShootSystem _shootSystem2;
    [SerializeField] ShootSystem _shootSystem2Follow;

    bool _isAttacking1 = false;
    bool _isAttackingRight = true;

    bool _isAttacking2 = false;
    bool _isAttack1Prepared;


    [SerializeField] UnityEvent _onAttack1Started = new UnityEvent();
    [SerializeField] UnityEvent _onAttack1Stoped = new UnityEvent();

    [FormerlySerializedAs("_onAttack1RightStarted")]
    [SerializeField] UnityEvent _onAttack1RightShoot = new UnityEvent();
    [FormerlySerializedAs("_onAttack1LeftStarted")]
    [SerializeField] UnityEvent _onAttack1LeftShoot = new UnityEvent();
    [SerializeField] UnityEvent _onAttack2Shoot = new UnityEvent();

    [SerializeField] UnityEvent _onAttack2Started = new UnityEvent();  
    [SerializeField] UnityEvent _onAttack2Stoped = new UnityEvent();



    void OnEnable()
    {
        if (_brain != null)
        {
            _brain.OnAttack1Started += OnAttack1Started;
            _brain.OnAttack1Stoped += OnAttack1Stoped;

            _brain.OnAttack2Started += OnAttack2Started;
            _brain.OnAttack2Stoped += OnAttack2Stoped;
        }

        if (_shootSystem1 != null) _shootSystem1.OnShoot += OnAttack1Shoot;
        if (_shootSystem2 != null) _shootSystem2.OnShoot += OnAttack2Shoot;
        if (_shootSystem2Follow != null) _shootSystem2Follow.OnShoot += OnAttack2Shoot;
    }

    void OnDisable()
    {
        if (_brain != null)
        {
            _brain.OnAttack1Started -= OnAttack1Started;
            _brain.OnAttack1Stoped -= OnAttack1Stoped;

            _brain.OnAttack2Started -= OnAttack2Started;
            _brain.OnAttack2Stoped -= OnAttack2Stoped;
        }

        if (_shootSystem1 != null) _shootSystem1.OnShoot -= OnAttack1Shoot;
        if (_shootSystem2 != null) _shootSystem2.OnShoot -= OnAttack2Shoot;
        if (_shootSystem2Follow != null) _shootSystem2Follow.OnShoot -= OnAttack2Shoot;
        _isAttack1Prepared = false;
    }

    void Update()
    {
        _animator.SetBool("isAttacking1", _isAttacking1);
        _animator.SetBool("isAttackingRight", _isAttackingRight);

        _animator.SetBool("isAttacking2", _isAttacking2);
    }

    public void StartAttack1()
    {
        _isAttacking1 = true;
        _isAttackingRight = !_isAttackingRight;
        _isAttack1Prepared = _shootSystem1 != null && _shootSystem1.HasPendingShoot;
    }
    public void StopAttack1()
    {
        _isAttacking1 = false;
        _isAttack1Prepared = false;
    }

    public void StartAttack2()
    {
        _isAttacking2 = true;
    }

    public void StopAttack2()
    {
        _isAttacking2 = false;
    }

    // Events

    void OnAttack1Shoot()
    {
        bool isAttackingRight = _isAttack1Prepared ? _isAttackingRight : !_isAttackingRight;
        _isAttack1Prepared = false;

        if (isAttackingRight)
            _onAttack1RightShoot.Invoke();
        else
            _onAttack1LeftShoot.Invoke();
    }

    void OnAttack2Shoot()
    {
        _onAttack2Shoot.Invoke();
    }

    void OnAttack1Started()
    {
        _onAttack1Started.Invoke();
    }

    void OnAttack1Stoped()
    {
        _onAttack1Stoped.Invoke();
    }

    void OnAttack2Started()
    {
        _onAttack2Started.Invoke();
    }

    void OnAttack2Stoped()
    {
        _onAttack2Stoped.Invoke();
    }
}
