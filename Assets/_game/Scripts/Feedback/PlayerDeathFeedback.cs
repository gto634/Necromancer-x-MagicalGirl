using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerDeathFeedback : MonoBehaviour
{
    [SerializeField] LifeSystem _lifeSystem;
    [SerializeField] PlayerInput _playerInput;

    [SerializeField] MaterialFlash[] _flashes;
    [SerializeField, Min(0f)] float _blinkDuration = 1.2f;
    [SerializeField, Min(0.02f)] float _blinkInterval = 0.15f;

    [SerializeField] GameObject _explosionVfx;
    [SerializeField] Transform _explosionPoint;
    [SerializeField, Min(0f)] float _explosionLifetime = 3f;
    [SerializeField] GameObject _visualRoot;

    [SerializeField] string _endMessage = "La Magical Girl a gagné !";
    [SerializeField, Min(0f)] float _endScreenDelay = 1f;

    [SerializeField] UnityEvent _onDeathFeedback = new UnityEvent();

    bool _isPlaying;

    void Start()
    {
        _lifeSystem.OnDeath += OnDeath;
    }

    void OnDestroy()
    {
        if (_lifeSystem != null)
        {
            _lifeSystem.OnDeath -= OnDeath;
        }
    }

    void OnDeath()
    {
        if (_isPlaying) return;
        _isPlaying = true;

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        if (_playerInput != null) _playerInput.DeactivateInput();
        _onDeathFeedback.Invoke();

        yield return Blink();
        Explode();

        yield return new WaitForSeconds(_endScreenDelay);

        if (EndScreen.Instance != null)
        {
            EndScreen.Instance.Show(_endMessage);
        }
        else
        {
        }
    }

    IEnumerator Blink()
    {
        if (_flashes == null || _flashes.Length == 0) yield break;

        int index = 0;
        for (float t = 0f; t < _blinkDuration; t += _blinkInterval)
        {
            MaterialFlash flash = _flashes[index % _flashes.Length];
            if (flash != null) flash.Flash();

            index++;
            yield return new WaitForSeconds(_blinkInterval);
        }
    }

    void Explode()
    {
        Vector3 position = _explosionPoint != null ? _explosionPoint.position : transform.root.position;

        if (_explosionVfx != null)
        {
            GameObject vfx = Instantiate(_explosionVfx, position, Quaternion.identity);
            Destroy(vfx, _explosionLifetime);
        }

        if (_visualRoot != null)
        {
            _visualRoot.SetActive(false);
        }
    }
}