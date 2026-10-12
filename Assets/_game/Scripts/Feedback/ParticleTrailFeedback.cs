using UnityEngine;

[DefaultExecutionOrder(100)]
public class ParticleTrailFeedback : MonoBehaviour
{
    [SerializeField] ParticleSystem _particlePrefab;

    ParticleSystem _particles;
    bool _hasStarted;
    bool _isDetached;
    bool _isReady = true;

    void OnEnable()
    {
        _isDetached = false;
    }

    void Start()
    {
        _hasStarted = true;
    }

    void LateUpdate()
    {
        if (!_hasStarted || !_isReady || _isDetached || _particlePrefab == null) return;

        // Orbit initialization and movement must happen before distance-based emission starts.
        if (_particles == null)
            _particles = Instantiate(_particlePrefab, transform.position, transform.rotation);

        _particles.transform.SetPositionAndRotation(transform.position, transform.rotation);
    }

    public void DeferEmission()
    {
        _isReady = false;
    }

    public void BeginEmission()
    {
        _isReady = true;
    }

    public ParticleSystem DetachParticles()
    {
        ParticleSystem particles = _particles;
        // Completion can run before this component's LateUpdate on the final orbit frame.
        if (particles != null)
            particles.transform.SetPositionAndRotation(transform.position, transform.rotation);
        _particles = null;
        _isDetached = true;
        return particles;
    }

    public void AttachParticles(ParticleSystem particles)
    {
        if (_particles == particles) return;

        StopParticles(DetachParticles());
        _particles = particles;
        _isDetached = false;
        if (_particles == null) return;

        ParticleSystem.MainModule main = _particles.main;
        main.stopAction = ParticleSystemStopAction.None;
    }

    public static void StopParticles(ParticleSystem particles)
    {
        if (particles == null) return;

        ParticleSystem.MainModule main = particles.main;
        main.stopAction = ParticleSystemStopAction.Destroy;
        particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void OnDisable()
    {
        StopParticles(DetachParticles());
    }
}
