using UnityEngine;

public class Attack1_MG : MonoBehaviour
{
    [SerializeField] ShootSystem _shootSystem;

    public bool CanAttack => isActiveAndEnabled
        && _shootSystem.isActiveAndEnabled && _shootSystem.CanShoot;

    public void StartAttack()
    {
        if (!CanAttack) return;

        _shootSystem.Shoot();
    }

    public void CancelAttack()
    {
        if (_shootSystem != null) _shootSystem.CancelShoot();
    }

    void OnDisable()
    {
        CancelAttack();
    }
}
