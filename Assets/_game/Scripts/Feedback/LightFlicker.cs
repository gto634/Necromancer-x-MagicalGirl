using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] Light _light;
    [SerializeField, Min(0f)] float _minIntensity = 2f;
    [SerializeField, Min(0f)] float _maxIntensity = 3f;
    [SerializeField, Min(0f)] float _frequency = 5f;
    [SerializeField] float _phase;

    float _initialIntensity;

    void OnEnable()
    {
        if (_light != null) _initialIntensity = _light.intensity;
    }

    void Update()
    {
        if (_light == null) return;

        float variation = Mathf.PerlinNoise(Time.unscaledTime * _frequency, _phase);
        _light.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, variation);
    }

    void OnDisable()
    {
        if (_light != null) _light.intensity = _initialIntensity;
    }
}
