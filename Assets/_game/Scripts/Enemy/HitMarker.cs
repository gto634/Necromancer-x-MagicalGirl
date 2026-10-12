using UnityEngine;

public class HitMarker : MonoBehaviour
{
    [SerializeField] AttackSystem _attacksystem;
    public void Hit()
    {


        _attacksystem.Attack();

    }
}
