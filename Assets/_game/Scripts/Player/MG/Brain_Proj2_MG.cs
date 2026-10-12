using System;
using UnityEngine;

public class Brain_Proj2_MG : MonoBehaviour
{
    [SerializeField] Projectile _projectile;
    [SerializeField] OrbitSystem _orbitSystem;
    [SerializeField] AttackSystem _attackSystem;

    Transform _rootTransform;
    ParticleTrailFeedback _trail;
    bool _hasFinished;

    public event Action<float> Rotated;
    public event Action Finished;

    void Awake()
    {
        _trail = _projectile.GetComponentInChildren<ParticleTrailFeedback>();
        if (_trail != null) _trail.DeferEmission();
    }

    void Start()
    {
        _rootTransform = _projectile.transform;
        if (_projectile.Owner == null || _orbitSystem.Duration <= 0f)
        {
            Destroy(_rootTransform.gameObject);
            return;
        }

        _orbitSystem.Rotated += OnRotate;
        _orbitSystem.Completed += OnCompleted;

        _orbitSystem.Orbit(_projectile.Owner);
        if (_trail != null) _trail.BeginEmission();

        foreach (HitboxSettings settings in _attackSystem.Hitboxes)
        {
            settings.Duration = _orbitSystem.Duration;
        }

        _attackSystem.Attack();
    }

    void Update()
    {
        if (_projectile.Owner == null)
        {
            Destroy(_rootTransform.gameObject);
            return;
        }
    }

    void OnRotate(float angle)
    {
        Rotated?.Invoke(angle);
    }

    void OnCompleted()
    {
        Finish();
        Destroy(_rootTransform.gameObject);
    }

    void OnDestroy()
    {
        if (_orbitSystem != null)
        {
            _orbitSystem.Rotated -= OnRotate;
            _orbitSystem.Completed -= OnCompleted;
        }

        Finish();
    }

    void Finish()
    {
        if (_hasFinished) return;

        _hasFinished = true;
        Finished?.Invoke();
    }
}
