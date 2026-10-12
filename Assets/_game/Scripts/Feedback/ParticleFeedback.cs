using UnityEngine;

[DefaultExecutionOrder(200)]
public class ParticleFeedback : MonoBehaviour
{
    [SerializeField] GameObject _effectRoot;
    [SerializeField] Transform _followPoint;

    ParticleSystem[] _particles;

    void Awake()
    {
        Stop();
    }

    public void Play()
    {
        if (_effectRoot == null) return;

        if (_particles == null)
            _particles = _effectRoot.GetComponentsInChildren<ParticleSystem>(true);

        FollowPoint();
        _effectRoot.SetActive(true);
        foreach (ParticleSystem particles in _particles)
        {
            if (particles == null) continue;
            particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.Play(false);
        }
    }

    public void Stop()
    {
        if (_particles != null)
        {
            foreach (ParticleSystem particles in _particles)
            {
                if (particles != null)
                    particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        if (_effectRoot != null) _effectRoot.SetActive(false);
    }

    void LateUpdate()
    {
        FollowPoint();
    }

    void FollowPoint()
    {
        if (_followPoint != null)
            transform.SetPositionAndRotation(_followPoint.position, _followPoint.rotation);
    }

    void OnDisable()
    {
        Stop();
    }
}
