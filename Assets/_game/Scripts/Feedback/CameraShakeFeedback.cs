using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraShakeFeedback : MonoBehaviour
{
    [SerializeField] CinemachineImpulseSource _impulseSource;
    [SerializeField] CinemachineImpulseListener _impulseListener;
    [SerializeField] PlayerInput _playerInput;
    [SerializeField, Min(0f)] float _strength = 0.45f;
    [SerializeField] Attack2_NEC _attack2;
    [SerializeField] CinemachineBasicMultiChannelPerlin _chargeNoise;
    [SerializeField, Min(0f)] float _startChargeAmplitude = 0.1f;
    [SerializeField, Min(0f)] float _endChargeAmplitude = 0.4f;

    bool _isCharging;

    void Start()
    {
        UpdateChannel();
    }

    public void Shake()
    {
        if (!isActiveAndEnabled || _impulseSource == null) return;

        UpdateChannel();
        _impulseSource.GenerateImpulseWithForce(_strength);
    }

    public void StartCharge()
    {
        _isCharging = true;
    }

    public void StopCharge()
    {
        _isCharging = false;
        if (_chargeNoise != null) _chargeNoise.AmplitudeGain = 0f;
    }

    void Update()
    {
        if (_chargeNoise == null) return;

        _chargeNoise.AmplitudeGain = _isCharging && _attack2 != null && _attack2.IsCharging
            ? Mathf.Lerp(_startChargeAmplitude, _endChargeAmplitude, _attack2.ChargeRatio)
            : 0f;
    }

    void OnDisable()
    {
        StopCharge();
    }

    void UpdateChannel()
    {
        int playerIndex = _playerInput != null ? Mathf.Clamp(_playerInput.playerIndex, 0, 30) : 0;
        int channel = 1 << playerIndex;
        if (_impulseSource != null) _impulseSource.ImpulseDefinition.ImpulseChannel = channel;
        if (_impulseListener != null) _impulseListener.ChannelMask = channel;
    }
}
