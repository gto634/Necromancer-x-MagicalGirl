using UnityEngine;

public class MovementTilt : MonoBehaviour
{
    [SerializeField] Transform _bone;
    [SerializeField] MoveSystem _moveSystem;

    [SerializeField, Range(0f, 45f)] float _tiltAngle = 12f;
    [SerializeField, Min(0f)] float _tiltSpeed = 10f;

    Vector3 _moveDirection;
    Quaternion _appliedTilt = Quaternion.identity;

    void Update()
    {
        RemoveTilt();
    }

    void LateUpdate()
    {
        if (_bone == null || _moveSystem == null) return;

        Vector3 direction = Vector3.zero;
        if (_moveSystem.isActiveAndEnabled && _moveSystem.Speed != 0f)
        {
            Vector2 move = _moveSystem.MoveDirection;
            direction = Vector3.ClampMagnitude(new Vector3(move.x, 0f, move.y), 1f);
        }

        float blend = 1f - Mathf.Exp(-_tiltSpeed * Time.deltaTime);
        _moveDirection = Vector3.Lerp(_moveDirection, direction, blend);

        Quaternion tilt = Quaternion.identity;
        if (_moveDirection.sqrMagnitude > 0.000001f)
        {
            Vector3 axis = Vector3.Cross(Vector3.up, _moveDirection);
            tilt = Quaternion.AngleAxis(_tiltAngle * _moveDirection.magnitude, axis);
        }

        Transform parent = _bone.parent;
        _appliedTilt = parent == null
            ? tilt
            : Quaternion.Inverse(parent.rotation) * tilt * parent.rotation;

        _bone.localRotation = _appliedTilt * _bone.localRotation;
    }

    void OnDisable()
    {
        RemoveTilt();
        _moveDirection = Vector3.zero;
    }

    void RemoveTilt()
    {
        if (_bone != null)
            _bone.localRotation = Quaternion.Inverse(_appliedTilt) * _bone.localRotation;

        _appliedTilt = Quaternion.identity;
    }
}
