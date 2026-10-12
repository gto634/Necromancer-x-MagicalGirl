using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootSystem : MonoBehaviour
{
    [SerializeField] Transform _shootPoint;
    [SerializeField] GameObject _prefab;
    [SerializeField] Transform _owner;

    [SerializeField, ReadOnly] float _delayTimer;
    [SerializeField, Min(0f)]  float _delay;

    bool _hasShoot = false;
    public bool CanShoot => _delayTimer == 0f && !_hasShoot;
    public bool HasPendingShoot => _hasShoot;
    public GameObject LastProjectile { get; private set; }
    public Transform ShootPoint => _shootPoint;

    public Action OnShoot;

    void Reset()
    {
        _owner = transform.root;
        _delay = 0.5f;
        _delayTimer = _delay;
    }

    void Awake()
    {
        if (_owner == null)
        {
            _owner = transform.root;
        }
    }

    void Update()
    {
        if (_delay == 0f) return;

        _delayTimer = Mathf.Max(0f, _delayTimer - Time.deltaTime);

        if (_delayTimer == 0f && _hasShoot)
        {
            CreateProjectile();
            _hasShoot = false;
        }
    }

    public void Shoot()
    {
        if (_delay == 0f)
        {
            CreateProjectile();
            return;
        }
        if (_delayTimer > 0f) return;

        _hasShoot = true;
        _delayTimer = _delay;
    }

    void CreateProjectile()
    {
        GameObject instance = Instantiate(_prefab, _shootPoint.position, _shootPoint.rotation);
        LastProjectile = instance;

        if (instance.TryGetComponent<Projectile>(out Projectile projectile))
        {
            projectile.Initialize(_owner);
        }

        OnShoot?.Invoke();
    }

    public void CancelShoot(InputAction.CallbackContext context)
    {
        CancelShoot();
    }

    public void CancelShoot()
    {
        _hasShoot = false;
        _delayTimer = 0f;
    }
}
