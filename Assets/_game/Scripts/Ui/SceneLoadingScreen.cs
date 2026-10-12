using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingScreen : MonoBehaviour
{
    [SerializeField] GameObject _visuals;
    [SerializeField] UnityEngine.UI.RawImage _headImage;
    [SerializeField] UnityEngine.UI.RawImage _heartImage;
    [SerializeField] RectTransform _leftCurtain;
    [SerializeField] RectTransform _rightCurtain;
    [SerializeField] Camera _iconsCamera;
    [SerializeField] Transform _heart;
    [SerializeField] Transform _head;
    [SerializeField, Min(0f)] float _rotationSpeed = 110f;
    [SerializeField, Min(0f)] float _minimumDisplayTime = 0.8f;
    [SerializeField, Min(0f)] float _transitionDuration = 0.35f;

    static SceneLoadingScreen _instance;
    RenderTexture _renderTexture;
    bool _isLoading;
    float _previousTimeScale;

    public static bool IsLoading => _instance != null && _instance._isLoading;

    public static bool LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Loading screen: scene unavailable in the build: " + sceneName);
            return false;
        }

        if (_instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("UI/LoadingScreen");
            if (prefab == null || prefab.GetComponent<SceneLoadingScreen>() == null)
            {
                Debug.LogError("Loading screen: missing Resources/UI/LoadingScreen prefab.");
                return false;
            }
            Instantiate(prefab);
        }

        if (_instance._isLoading) return false;
        _instance.StartCoroutine(_instance.Load(sceneName));
        return true;
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        _renderTexture = new RenderTexture(512, 256, 24, RenderTextureFormat.ARGB32);
        _renderTexture.name = "Loading icons";
        _renderTexture.Create();
        _iconsCamera.targetTexture = _renderTexture;
        _headImage.texture = _renderTexture;
        _heartImage.texture = _renderTexture;
        _visuals.SetActive(false);
    }

    void Update()
    {
        if (!_isLoading) return;
        float angle = _rotationSpeed * Time.unscaledDeltaTime;
        _heart.Rotate(Vector3.back, angle, Space.World);
        _head.Rotate(Vector3.back, angle, Space.World);
    }

    IEnumerator Load(string sceneName)
    {
        _isLoading = true;
        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _visuals.SetActive(true);
        try
        {
            Canvas.ForceUpdateCanvases();
            SetCurtains(0f);
            yield return AnimateCurtains(true);
            float startedAt = Time.realtimeSinceStartup;
            // Present the closed screen before starting work on the next scene.
            yield return null;

            AsyncOperation operation = BeginLoad(sceneName);
            if (operation == null) yield break;
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f || Time.realtimeSinceStartup - startedAt < _minimumDisplayTime)
                yield return null;

            operation.allowSceneActivation = true;
            while (!operation.isDone) yield return null;

            // Let the new scene finish its first frame before revealing it.
            yield return null;
            yield return AnimateCurtains(false);
        }
        finally
        {
            Time.timeScale = _previousTimeScale;
            _visuals.SetActive(false);
            _isLoading = false;
        }
    }

    IEnumerator AnimateCurtains(bool closing)
    {
        float elapsed = 0f;
        while (elapsed < _transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / _transitionDuration));
            SetCurtains(closing ? progress : 1f - progress);
            yield return null;
        }
        SetCurtains(closing ? 1f : 0f);
    }

    void SetCurtains(float closedAmount)
    {
        float hiddenAmount = 1f - closedAmount;
        _leftCurtain.anchoredPosition = new Vector2(-_leftCurtain.rect.width * hiddenAmount, 0f);
        _rightCurtain.anchoredPosition = new Vector2(_rightCurtain.rect.width * hiddenAmount, 0f);
    }

    AsyncOperation BeginLoad(string sceneName)
    {
        try
        {
            return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            return null;
        }
    }

    void OnDestroy()
    {
        if (_instance != this) return;
        if (_isLoading) Time.timeScale = _previousTimeScale;
        _instance = null;
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }
}
