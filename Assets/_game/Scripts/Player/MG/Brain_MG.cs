using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Brain_MG : MonoBehaviour
{
    [SerializeField] InputActionReference _movementInput; 
    [SerializeField] InputActionReference _lookInput;
    [SerializeField] InputActionReference _attackInput;
    [SerializeField] InputActionReference _attack2Input;
    [SerializeField] InputActionReference _aimInput;


    [SerializeField] MoveSystem _moveSystem;
    [SerializeField] LookSystem _lookSystem;
    [SerializeField] Attack1_MG _attack1;
    [SerializeField] Attack2_MG _attack2;


    PlayerInput _playerInput; 
    InputAction _moveAction; 
    InputAction _lookAction;
    InputAction _attackAction;
    InputAction _attack2Action;
    InputAction _aimAction;

    public Action OnAttack1Started;
    public Action OnAttack1Stoped;

    public Action OnAttack2Started;
    public Action OnAttack2Stoped;


    public bool IsUsingCompetence2 => _attack2 != null && _attack2.IsAttacking;

    // Clocks
    [SerializeField, ReadOnly] float _competence1Timer = 0f;
    [SerializeField]           float _competence1Cooldown = 0.5f;

    [SerializeField, ReadOnly] float _competence2Timer = 0f;
    [SerializeField]           float _competence2Cooldown = 0.5f;


    void Start()
    {
        #region PlayerInput
        _playerInput = GetComponent<PlayerInput>();
        _playerInput.actions.Disable();                
        _playerInput.SwitchCurrentActionMap("Player");
        #endregion

        #region Move
        _moveAction = _playerInput.actions.FindAction(_movementInput.action.id);

        _moveAction.performed += UpdateMove;
        _moveAction.canceled += UpdateMove;
        #endregion

        #region Look
        _lookAction = _playerInput.actions.FindAction(_lookInput.action.id);

        _lookAction.performed += UpdateLook;
        _lookAction.canceled += UpdateLook;

        _aimAction = _playerInput.actions.FindAction(_aimInput.action.id);
        _aimAction.performed += UpdateAim;
        #endregion

        #region Attack
        _attackAction = _playerInput.actions.FindAction(_attackInput.action.id);

        _competence1Timer = _competence1Cooldown;

        _attackAction.canceled += CancelAttack1;
        #endregion

        #region Attack2
        if (_attack2Input != null && _attack2 != null)
        {
            _attack2Action = _playerInput.actions.FindAction(_attack2Input.action.id);

            _competence2Timer = _competence2Cooldown;

            _attack2Action.canceled += CancelAttack2;
            _attack2.Finished += OnAttack2Finished;
        }
        #endregion
    }

    void OnDestroy()
    {
        if (_attack2 != null) _attack2.Finished -= OnAttack2Finished;
        if (_moveAction == null) return;

        _moveAction.performed -= UpdateMove;
        _moveAction.canceled -= UpdateMove;

        _lookAction.performed -= UpdateLook;
        _lookAction.canceled -= UpdateLook;

        _attackAction.canceled -= CancelAttack1;
        _aimAction.performed -= UpdateAim;

        if (_attack2Action != null)
        {
            _attack2Action.canceled -= CancelAttack2;
        }
    }

    void Update()
    {
        _competence1Timer = Mathf.Max(0, _competence1Timer - Time.deltaTime);
        _competence2Timer = Mathf.Max(0, _competence2Timer - Time.deltaTime);

        if (IsUsingCompetence2)
        {
            if (_attack2Action != null && _attack2Action.WasPressedThisFrame())
                _attack2.StartAttack();
            return;
        }

        if (_attack2Action != null && _attack2Action.WasPressedThisFrame()
            && _competence2Timer == 0 && _attack2.CanAttack)
        {
            _attack1.CancelAttack();
            _attack2.StartAttack();
            OnAttack2Started?.Invoke();
            _competence2Timer = _competence2Cooldown;
            return;
        }

        if (_attackAction.IsPressed() && _competence1Timer == 0 && _attack1.CanAttack)
        {
            _attack1.StartAttack();
            OnAttack1Started?.Invoke();
            _competence1Timer = _competence1Cooldown;
        }
    }

    void OnDisable()
    {
        if (_attack1 != null) _attack1.CancelAttack();
        if (_attack2 != null) _attack2.StopAttack();
    }

    void CancelAttack1(InputAction.CallbackContext context)
    {
        _attack1.CancelAttack();
        OnAttack1Stoped?.Invoke();
    }

    void CancelAttack2(InputAction.CallbackContext context)
    {
        _attack2.CancelAttack();
    }

    void OnAttack2Finished()
    {
        OnAttack2Stoped?.Invoke();
    }

    void UpdateMove(InputAction.CallbackContext context)
    {
        _moveSystem.MoveDirection = context.ReadValue<Vector2>(); 
    }

    void UpdateLook(InputAction.CallbackContext context)
    {
        _lookSystem.LookDirection = context.ReadValue<Vector2>();
    }

    private void UpdateAim(InputAction.CallbackContext context)
    {
        _lookSystem.AimScreenPosition = context.ReadValue<Vector2>();
        _lookSystem.UseMouseAim = true;
    }
}
