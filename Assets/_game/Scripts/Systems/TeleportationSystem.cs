using UnityEngine;

public class TeleportationSystem : MonoBehaviour
{
    [SerializeField] Transform _rootTransform;
    [SerializeField] Transform _directionTransform;
    [SerializeField] Transform _raycastPoint;
    [SerializeField] Rigidbody _rb;
    [SerializeField] CapsuleCollider _bodyCollider;

    [SerializeField, Min(0f)] float _distance = 5f;
    [SerializeField, Min(0f)] float _wallMargin = 0.05f;
    [SerializeField] LayerMask _blockingLayers;

    public float Distance => _distance;

    void Reset()
    {
        _rootTransform = transform.root;
        _directionTransform = _rootTransform;
        _raycastPoint = _rootTransform;
        _rb = _rootTransform.GetComponent<Rigidbody>();
        _blockingLayers = LayerMask.GetMask("Environment", "Walls");
    }

    public void Teleport()
    {
        Teleport(_distance);
    }

    public void Teleport(float distance)
    {
        if (!TryGetDestination(distance, out Vector3 position)) return;

        bool moved = (position - _rootTransform.position).sqrMagnitude > 0f;
        if (_rb != null) _rb.position = position;
        _rootTransform.position = position;
        if (moved)
        {
            PlayerCommander commander = _rootTransform.GetComponentInChildren<PlayerCommander>();
            if (commander != null) commander.ResetFollowTrail();
        }
    }

    public bool TryGetDestination(float distance, out Vector3 position)
    {
        position = _rootTransform.position;
        distance = Mathf.Clamp(distance, 0f, Mathf.Max(0f, _distance));

        Vector3 direction = _directionTransform.forward;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return false;
        direction.Normalize();

        Physics.SyncTransforms();

        float radius = _bodyCollider != null ? _bodyCollider.radius : 0f;

        if (Physics.Raycast(_raycastPoint.position, direction, out RaycastHit hit,
            distance + radius + _wallMargin, _blockingLayers, QueryTriggerInteraction.Ignore))
        {
            distance = Mathf.Clamp(hit.distance - radius - _wallMargin, 0f, distance);
        }

        if (_bodyCollider != null)
        {
            Bounds bounds = _bodyCollider.bounds;
            radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
            Vector3 offset = Vector3.up * Mathf.Max(0f, bounds.extents.y - radius);
            Vector3 top = bounds.center + offset;
            Vector3 bottom = bounds.center - offset;

            // Leave a little clearance above the floor without narrowing the wall check.
            float groundClearance = Mathf.Max(_wallMargin, _bodyCollider.contactOffset);
            bottom += Vector3.up * Mathf.Min(groundClearance, offset.y * 2f);

            if (Physics.CheckCapsule(top, bottom, radius, _blockingLayers, QueryTriggerInteraction.Ignore)) return false;

            if (Physics.CapsuleCast(top, bottom, radius, direction, out hit,
                distance, _blockingLayers, QueryTriggerInteraction.Ignore))
            {
                distance = Mathf.Max(0f, hit.distance - _wallMargin);
            }
        }

        position = _rootTransform.position + direction * distance;
        return true;
    }
}
