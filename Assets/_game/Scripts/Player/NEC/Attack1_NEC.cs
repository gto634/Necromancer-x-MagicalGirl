using UnityEngine;

public class Attack1_NEC : MonoBehaviour
{
    [SerializeField] ShootSystem _shootSystem;
    [SerializeField] Renderer[] _headRenderers;
    [SerializeField] Light[] _headLights = new Light[0];

    Brain_Proj1_NEC _projectile;
    bool[] _wasHeadEnabled;
    bool[] _wasHeadLightEnabled;

    public bool IsAttacking => _projectile != null || _shootSystem.HasPendingShoot;
    public bool CanAttack => isActiveAndEnabled && !IsAttacking
        && _shootSystem.isActiveAndEnabled && _shootSystem.CanShoot;

    void Awake()
    {
        _wasHeadEnabled = new bool[_headRenderers.Length];
        _wasHeadLightEnabled = new bool[_headLights.Length];
    }

    void OnEnable()
    {
        _shootSystem.OnShoot += OnShoot;
    }

    public void StartAttack()
    {
        if (!CanAttack) return;

        _shootSystem.Shoot();
    }

    public void CancelAttack()
    {
        if (_shootSystem != null) _shootSystem.CancelShoot();
    }

    void OnShoot()
    {
        _projectile = _shootSystem.LastProjectile.GetComponentInChildren<Brain_Proj1_NEC>();
        if (_projectile == null) return;

        _projectile.Initialize(_shootSystem.ShootPoint);
        _projectile.Finished += EndAttack;

        for (int i = 0; i < _headRenderers.Length; i++)
        {
            if (_headRenderers[i] == null) continue;
            _wasHeadEnabled[i] = _headRenderers[i].enabled;
            _headRenderers[i].enabled = false;
        }

        for (int i = 0; i < _headLights.Length; i++)
        {
            if (_headLights[i] == null) continue;
            _wasHeadLightEnabled[i] = _headLights[i].enabled;
            _headLights[i].enabled = false;
        }
    }

    void EndAttack()
    {
        if (_projectile != null) _projectile.Finished -= EndAttack;
        _projectile = null;

        for (int i = 0; i < _headRenderers.Length; i++)
        {
            if (_headRenderers[i] != null) _headRenderers[i].enabled = _wasHeadEnabled[i];
        }

        for (int i = 0; i < _headLights.Length; i++)
        {
            if (_headLights[i] != null) _headLights[i].enabled = _wasHeadLightEnabled[i];
        }
    }

    public void StopAttack()
    {
        CancelAttack();
        if (_projectile == null) return;

        GameObject instance = _projectile.transform.root.gameObject;
        EndAttack();
        Destroy(instance);
    }

    void OnDisable()
    {
        if (_shootSystem != null) _shootSystem.OnShoot -= OnShoot;
        StopAttack();
    }
}
