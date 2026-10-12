using UnityEngine;
using UnityEngine.Rendering;

public class HealthBarFeedback : MonoBehaviour
{
    [SerializeField] LifeSystem _lifeSystem;
    [SerializeField] Transform _followPoint;
    [SerializeField] Canvas _canvas;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] RectTransform _fill;
    [SerializeField, Min(0)] float _height = 2.2f;
    [SerializeField, Min(0)] float _visibleDuration = 1f;
    [SerializeField, Min(0)] float _fadeDuration = 0.25f;

    float _elapsed;
    bool _isVisible;

    void Awake()
    {
        Hide();
    }

    void OnEnable()
    {
        Hide();
        if (_lifeSystem == null) return;

        SetHealth(_lifeSystem.Health);
        _lifeSystem.OnDamage += OnDamage;
        _lifeSystem.OnHeal += OnHeal;
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDisable()
    {
        if (_lifeSystem != null)
        {
            _lifeSystem.OnDamage -= OnDamage;
            _lifeSystem.OnHeal -= OnHeal;
        }

        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        Hide();
    }

    void Update()
    {
        UpdateVisibility(Time.deltaTime);
    }

    void LateUpdate()
    {
        if (!_isVisible || _followPoint == null || _canvas == null) return;
        _canvas.transform.position = _followPoint.position + Vector3.up * (_height * Mathf.Abs(_followPoint.lossyScale.y));
    }

    void OnDamage(int previousHealth, int newHealth)
    {
        if (newHealth >= previousHealth || _canvas == null || _canvasGroup == null) return;

        SetHealth(newHealth);
        _elapsed = 0;
        _isVisible = true;
        _canvasGroup.alpha = 1;
        _canvas.enabled = true;
    }

    void OnHeal(int previousHealth, int newHealth)
    {
        SetHealth(newHealth);
    }

    void SetHealth(int health)
    {
        if (_fill == null || _lifeSystem == null) return;
        float amount = _lifeSystem.MaxHealth > 0 ? (float)health / _lifeSystem.MaxHealth : 0;
        _fill.anchorMax = new Vector2(Mathf.Clamp01(amount), 1);
    }

    void UpdateVisibility(float deltaTime)
    {
        if (!_isVisible) return;

        _elapsed += deltaTime;
        if (_elapsed <= _visibleDuration) return;

        if (_fadeDuration <= 0 || _elapsed >= _visibleDuration + _fadeDuration)
        {
            Hide();
            return;
        }

        _canvasGroup.alpha = 1 - (_elapsed - _visibleDuration) / _fadeDuration;
    }

    void Hide()
    {
        _isVisible = false;
        _elapsed = 0;
        if (_canvasGroup != null) _canvasGroup.alpha = 0;
        if (_canvas != null) _canvas.enabled = false;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (!_isVisible || _canvas == null || camera == null) return;
        _canvas.transform.rotation = camera.transform.rotation;
    }
}
