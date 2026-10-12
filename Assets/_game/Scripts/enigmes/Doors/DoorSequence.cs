using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Playables;


public class DoorSequence : MonoBehaviour
{
    const int MainPlayerIndex = 0;

    enum Audience { AllPlayers, Necromancer, MagicalGirl }

    [SerializeField] Audience _audience;

    [SerializeField] List<DoorCondition> _conditions = new List<DoorCondition>();

    [SerializeField] PlayableDirector _director;
    [Tooltip("Désactivée dans la scène, Priority = 10, Output Channel = Channel01 + Channel02.")]
    [SerializeField] CinemachineCamera _doorCamera;

    [SerializeField] ScreenFader _fader;
    [SerializeField, Min(0f)] float _fadeDuration = 0.2f;

    [SerializeField] UnityEvent _onFinished = new UnityEvent();

    bool _hasStarted;
    PlayerInput _individualPlayer;
    OutputChannels _savedChannel;
    bool _individualSequence;
    bool _inputWasActive;
    bool _cameraWasActive;

    void Awake()
    {
        if (_director == null || _doorCamera == null
            || (_audience == Audience.AllPlayers && _fader == null))
        {
            enabled = false;
        }
    }

    void Update()
    {
        if (_hasStarted || !AreConditionsMet()) return;

        PlayerInput player = null;
        if (_audience != Audience.AllPlayers)
        {
            player = GetIndividualPlayer();
            if (player == null || player.gameObject.scene != gameObject.scene
                || player.camera == null || !player.camera.TryGetComponent<CinemachineBrain>(out _)) return;
        }

        _hasStarted = true;
        StartCoroutine(PlaySequence(player));
    }

    void OnDisable()
    {
        if (!_individualSequence) return;

        StopAllCoroutines();
        RestoreIndividualState();
    }

    PlayerInput GetIndividualPlayer()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null) return null;

        return _audience == Audience.Necromancer
            ? manager.necromancianInput : manager.magicalGirlInput;
    }

    bool AreConditionsMet()
    {
        if (_conditions.Count == 0) return false;

        foreach (DoorCondition condition in _conditions)
        {
            if (!condition.IsMet) return false;
        }
        return true;
    }

    IEnumerator PlaySequence(PlayerInput player)
    {
        if (player != null)
        {
            yield return PlayIndividualSequence(player);
            _onFinished.Invoke();
            yield break;
        }

        SetPlayersInputActive(false);

        // Switch the layout before the cinematic camera can reach either player view.
        yield return FadeTransition(() => SetSplitScreen(false));
        yield return BlendToDoorCamera();
        yield return PlayCinematic();
        yield return BlendBackToPlayers();
        yield return FadeTransition(() => SetSplitScreen(true));

        SetPlayersInputActive(true);
        _onFinished.Invoke();
    }

    IEnumerator PlayIndividualSequence(PlayerInput player)
    {
        CinemachineBrain brain = player.camera.GetComponent<CinemachineBrain>();
        float blendDuration = brain.DefaultBlend.BlendTime;
        _individualPlayer = player;
        _savedChannel = _doorCamera.OutputChannel;
        _inputWasActive = player.inputIsActive;
        _cameraWasActive = _doorCamera.gameObject.activeSelf;
        _individualSequence = true;

        try
        {
            // Channels follow the player's index, which can change with character selection.
            _doorCamera.OutputChannel = brain.ChannelMask;
            if (_inputWasActive) player.DeactivateInput();
            _doorCamera.gameObject.SetActive(true);
            yield return new WaitForSeconds(blendDuration);
            yield return PlayCinematic();
            _doorCamera.gameObject.SetActive(false);
            yield return new WaitForSeconds(blendDuration);
        }
        finally
        {
            RestoreIndividualState();
        }
    }

    void RestoreIndividualState()
    {
        if (!_individualSequence) return;
        _individualSequence = false;

        if (_doorCamera != null)
        {
            _doorCamera.gameObject.SetActive(_cameraWasActive);
            _doorCamera.OutputChannel = _savedChannel;
        }
        if (_individualPlayer != null && _inputWasActive) _individualPlayer.ActivateInput();
        _individualPlayer = null;
    }

    IEnumerator BlendToDoorCamera()
    {
        _doorCamera.gameObject.SetActive(true);
        yield return new WaitForSeconds(GetPlayerBlendDuration());
    }

    IEnumerator BlendBackToPlayers()
    {
        _doorCamera.gameObject.SetActive(false);
        yield return new WaitForSeconds(GetPlayerBlendDuration());
    }

    IEnumerator PlayCinematic()
    {
        _director.Play();

        yield return new WaitForSeconds((float)_director.duration);
    }

    IEnumerator FadeTransition(Action changeWhileBlack)
    {
        yield return _fader.FadeTo(1f, _fadeDuration);
        changeWhileBlack();
        yield return _fader.FadeTo(0f, _fadeDuration);
    }

    static void SetSplitScreen(bool split)
    {
        foreach (PlayerInput player in PlayerInput.all)
        {
            if (player.playerIndex != MainPlayerIndex && player.camera != null)
            {
                player.camera.enabled = split;
            }
        }

        if (PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.splitScreen = split;
        }
    }

    static void SetPlayersInputActive(bool active)
    {
        foreach (PlayerInput player in PlayerInput.all)
        {
            if (active) player.ActivateInput();
            else player.DeactivateInput();
        }
    }

    static float GetPlayerBlendDuration()
    {
        foreach (PlayerInput player in PlayerInput.all)
        {
            if (player.camera != null && player.camera.TryGetComponent(out CinemachineBrain brain))
            {
                return brain.DefaultBlend.BlendTime;
            }
        }
        return 0f;
    }
}