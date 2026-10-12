using System.Collections.Generic;
using ALTUSHKA.EasyNodeDialogSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogInputBridge : MonoBehaviour
{
    const string DialogueMap = "Dialogue";
    const string GameplayMap = "Player";
    const string AdvanceAction = "Dialogue/Advance";

    readonly List<InputAction> _advanceActions = new List<InputAction>();
    bool _isCapturing;

    void OnEnable()
    {
        DialogRunner.GameplayFreezeRequested += OnFreezeRequested;
    }

    void OnDisable()
    {
        DialogRunner.GameplayFreezeRequested -= OnFreezeRequested;
        ReleasePlayers();
    }

    void OnFreezeRequested(bool freeze, bool showCursor)
    {
        if (freeze) CapturePlayers();
        else ReleasePlayers();
    }

    void CapturePlayers()
    {
        if (_isCapturing) return;
        _isCapturing = true;

        foreach (PlayerInput player in PlayerInput.all)
        {
            player.SwitchCurrentActionMap(DialogueMap);

            InputAction advance = player.actions.FindAction(AdvanceAction);
            if (advance == null)
            {
                continue;
            }

            advance.performed += OnAdvance;
            _advanceActions.Add(advance);
        }
    }

    void ReleasePlayers()
    {
        if (!_isCapturing) return;
        _isCapturing = false;

        foreach (InputAction advance in _advanceActions)
        {
            advance.performed -= OnAdvance;
        }
        _advanceActions.Clear();

        foreach (PlayerInput player in PlayerInput.all)
        {
            player.SwitchCurrentActionMap(GameplayMap);
        }
    }

    void OnAdvance(InputAction.CallbackContext context)
    {
        if (DialogRunner.Instance != null)
        {
            DialogRunner.Instance.Advance();
        }
    }
}