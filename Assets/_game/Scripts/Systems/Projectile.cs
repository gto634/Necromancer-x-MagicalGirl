using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField, ReadOnly] Transform _owner;

    public Transform Owner => _owner;

    public void Initialize(Transform owner)
    {
        _owner = owner;
    }
}
