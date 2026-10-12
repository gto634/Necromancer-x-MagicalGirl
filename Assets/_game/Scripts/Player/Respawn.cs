using Unity.Cinemachine;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] LifeSystem _lifeSystem;
    [SerializeField] Rigidbody _rigidbody;        
    [SerializeField] CinemachineCamera _camera;   

    Vector3 _spawnPosition;
    Quaternion _spawnRotation;

    void Start()
    {
        _spawnPosition = transform.root.position;
        _spawnRotation = transform.root.rotation;
    }

    void Update()
    {
        if (_lifeSystem.IsDead)
            RespawnPlayer();
    }

    void RespawnPlayer()
    {
        transform.root.SetPositionAndRotation(_spawnPosition, _spawnRotation);

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _camera.PreviousStateIsValid = false;

        _lifeSystem.Revive();
    }
}