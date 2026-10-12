using UnityEngine;

public class TeleportationPreview : MonoBehaviour
{
    [SerializeField] Attack2_NEC _attack2;
    [SerializeField] TeleportationSystem _teleportationSystem;
    [SerializeField] ParticleFeedback _pointFeedback;

    bool _isVisible;

    void LateUpdate()
    {
        if (_attack2 == null || _teleportationSystem == null || _pointFeedback == null
            || (!_attack2.IsCharging && !_attack2.IsTeleporting)
            || !_teleportationSystem.TryGetDestination(_attack2.TeleportationDistance, out Vector3 position))
        {
            Hide();
            return;
        }

        transform.SetPositionAndRotation(position, Quaternion.identity);
        if (_isVisible) return;

        _pointFeedback.Play();
        _isVisible = true;
    }

    void Hide()
    {
        if (!_isVisible) return;
        if (_pointFeedback != null) _pointFeedback.Stop();
        _isVisible = false;
    }

    void OnDisable()
    {
        Hide();
    }
}
