using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    const float MaxStep = 1f / 30f;

    CanvasGroup _group;

    void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
    }

    public IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = _group.alpha;

        for (float t = 0f; t < duration; t += Mathf.Min(Time.unscaledDeltaTime, MaxStep))
        {
            _group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            yield return null;
        }
        _group.alpha = targetAlpha;
    }
}