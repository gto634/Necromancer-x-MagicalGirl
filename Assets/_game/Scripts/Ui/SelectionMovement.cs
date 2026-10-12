using UnityEngine;

public class SelectionMovement : MonoBehaviour
{
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] GameObject _leftArrow;
    [SerializeField] GameObject _rightArrow;
    [SerializeField] float _leftPosition = -500f;
    [SerializeField] float _middlePosition;
    [SerializeField] float _rightPosition = 500f;
    [SerializeField, Min(0.01f)] float _duration = 0.3f;

    int _targetPosition = int.MinValue;
    float _startPosition;
    float _destination;
    float _timer;
    bool _moving;

    void OnEnable()
    {
        _targetPosition = int.MinValue;
    }

    public void MoveTo(int position)
    {
        if (_rectTransform == null || position == _targetPosition) return;
        _targetPosition = position;

        if (_leftArrow != null) _leftArrow.SetActive(position > -1);
        if (_rightArrow != null) _rightArrow.SetActive(position < 1);

        float destination = position < 0 ? _leftPosition : position > 0 ? _rightPosition : _middlePosition;
        StartMove(destination);
    }

    void StartMove(float destination)
    {
        _startPosition = _rectTransform.anchoredPosition.x;
        _destination = destination;
        _timer = 0f;
        _moving = true;
    }

    void LateUpdate()
    {
        Advance(Time.unscaledDeltaTime);
    }

    void Advance(float deltaTime)
    {
        if (!_moving || _rectTransform == null) return;

        _timer += deltaTime;
        float progress = Mathf.Clamp01(_timer / Mathf.Max(0.01f, _duration));
        float x = Mathf.Lerp(_startPosition, _destination, Mathf.SmoothStep(0f, 1f, progress));
        _rectTransform.anchoredPosition = new Vector2(x, _rectTransform.anchoredPosition.y);

        if (progress < 1f) return;

        _moving = false;
    }

    void OnDisable()
    {
        _moving = false;
    }
}
