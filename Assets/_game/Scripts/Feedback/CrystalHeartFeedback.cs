using UnityEngine;

public class CrystalHeartFeedback : MonoBehaviour
{
    [SerializeField] Transform _crystalHeart;
    [SerializeField] Light _heartLight;
    [SerializeField, Min(0f)] float _reappearDelay = 0.5f;
    [SerializeField, Min(0f)] float _growDuration = 0.3f;
    [SerializeField, Range(0.001f, 1f)] float _startScale = 0.05f;

    Vector3 _initialScale;
    float _initialLightRange;
    float _timer;
    bool _wasLightEnabled;
    bool _isGrowing;
    bool _isHeld;
    bool _initialized;

    void Awake()
    {
        Initialize();
    }

    public void HideHeart()
    {
        if (_crystalHeart == null || !isActiveAndEnabled) return;
        Initialize();

        ApplySize(0f);
        _timer = -_reappearDelay;
        _isGrowing = true;
    }

    public void ShowHeart()
    {
        if (_crystalHeart == null || !isActiveAndEnabled) return;
        Initialize();

        _isHeld = false;
        _timer = 0f;
        _isGrowing = true;
        UpdateScale();
    }

    public void HoldHeart()
    {
        if (_crystalHeart == null || !isActiveAndEnabled) return;

        HideHeart();
        _isHeld = true;
    }

    public void ReleaseHeart()
    {
        if (!_isHeld) return;

        _isHeld = false;
        HideHeart();
    }

    void LateUpdate()
    {
        if (!_isGrowing || _crystalHeart == null) return;

        if (_isHeld)
        {
            ApplySize(0f);
            return;
        }

        _timer += Time.deltaTime;
        if (_timer < 0f)
        {
            ApplySize(0f);
            return;
        }

        UpdateScale();
    }

    void UpdateScale()
    {
        float progress = _growDuration > 0f ? Mathf.Clamp01(_timer / _growDuration) : 1f;
        ApplySize(Mathf.Lerp(_startScale, 1f, progress));

        if (progress >= 1f)
            _isGrowing = false;
    }

    void ApplySize(float size)
    {
        if (_crystalHeart != null)
            _crystalHeart.localScale = _initialScale * size;

        if (_heartLight != null)
        {
            _heartLight.range = _initialLightRange * size;
            _heartLight.enabled = _wasLightEnabled && size > 0f;
        }
    }

    void OnDisable()
    {
        _isHeld = false;
        _isGrowing = false;
        if (_initialized)
            ApplySize(1f);
    }

    void Initialize()
    {
        if (_initialized || _crystalHeart == null) return;

        _initialScale = _crystalHeart.localScale;
        if (_heartLight != null)
        {
            _initialLightRange = _heartLight.range;
            _wasLightEnabled = _heartLight.enabled;
        }
        _initialized = true;
    }
}
