using System;
using System.Collections;
using ALTUSHKA.EasyNodeDialogSystem;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(DialogActivator))]
public class DialogTrigger : MonoBehaviour
{
    enum TriggerMode { OnLevelStart, OnPlayerEnter }

    [SerializeField] TriggerMode _mode = TriggerMode.OnPlayerEnter;

    [SerializeField, Min(0f)] float _startDelay = 1f;

    [SerializeField] string[] _playerTags = { "NEC", "MG" };

    DialogActivator _activator;
    bool _hasPlayed;

    void Awake()
    {
        _activator = GetComponent<DialogActivator>();
    }

    IEnumerator Start()
    {
        if (_mode != TriggerMode.OnLevelStart) yield break;

      
        while (PlayerInput.all.Count == 0) yield return null;
        yield return new WaitForSeconds(_startDelay);

        TryPlay();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_mode != TriggerMode.OnPlayerEnter) return;
        if (Array.IndexOf(_playerTags, other.tag) < 0) return;

        TryPlay();
    }

    void TryPlay()
    {
        if (_hasPlayed) return;
        if (DialogRunner.Instance == null || DialogRunner.Instance.IsTalking) return;

        _hasPlayed = _activator.TryStartDialog();
    }
}