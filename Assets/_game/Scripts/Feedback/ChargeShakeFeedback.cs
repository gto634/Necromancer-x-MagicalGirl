using UnityEngine;

[DefaultExecutionOrder(100)]
public class ChargeShakeFeedback : MonoBehaviour
{
    [SerializeField] Transform _visualRoot;
    [SerializeField] Attack2_NEC _attack2;
    [SerializeField, Min(0f)] float _startAmplitude = 0.01f;
    [SerializeField, Min(0f)] float _endAmplitude = 0.055f;
    [SerializeField, Min(0f)] float _frequency = 35f;

    Vector3 _appliedOffset;

    void LateUpdate()
    {
        RemoveShake();
        if (_visualRoot == null || _attack2 == null || !_attack2.IsCharging) return;

        float amplitude = Mathf.Lerp(_startAmplitude, _endAmplitude, _attack2.ChargeRatio);
        float phase = Time.time * _frequency;
        float seed = (GetInstanceID() & 0xFFFF) * 0.01f;
        _appliedOffset = new Vector3(
            Mathf.PerlinNoise(phase, seed) * 2f - 1f,
            (Mathf.PerlinNoise(phase, seed + 10f) * 2f - 1f) * 0.5f,
            Mathf.PerlinNoise(phase, seed + 20f) * 2f - 1f) * amplitude;

        _visualRoot.localPosition += _appliedOffset;
    }

    void RemoveShake()
    {
        if (_visualRoot != null) _visualRoot.localPosition -= _appliedOffset;
        _appliedOffset = Vector3.zero;
    }

    void OnDisable()
    {
        RemoveShake();
    }
}
