using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALTUSHKA.EasyNodeDialogSystem
{
    /// <summary>Default Input: Space / Enter / click call DialogRunner.Advance.</summary>
    public sealed class DialogLegacyInput : MonoBehaviour
    {
        #region Developer API

            /// <summary>Same as Space / Enter / click during dialog.</summary>
            public void RequestAdvance() => RequestAdvanceImpl();

        #endregion Developer API


        [SerializeField] InputActionReference _dialogInteract;
        private DialogRunner _runner;

        private void Start()
        {
            _runner = DialogRunner.Instance;
             _dialogInteract.action.performed += interact;

            if (_runner == null) throw new InvalidOperationException("DialogLegacyInput: DialogRunner.Instance is missing - place DialogRunner in the scene.");
        }

        private void interact(InputAction.CallbackContext context)
        {
            _runner.Advance();
        }

        private void RequestAdvanceImpl()
        {
            if (!_runner.IsTalking) return;

            _runner.Advance();
        }

        private void Update()
        {
            if (_runner == null || !_runner.IsTalking) return;
   

        }
    }
}