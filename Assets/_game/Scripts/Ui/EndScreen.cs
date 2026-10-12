using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class EndScreen : MonoBehaviour
{
    public static EndScreen Instance { get; private set; }

    [SerializeField] GameObject _panel;
    [SerializeField] TMP_Text _messageText;

    [SerializeField, Min(0f)] float _returnToMenuDelay = 5f;
    [SerializeField] string _menuScene = "HomeScreen";

    bool _isShown;

    void Awake()
    {
        Instance = this;
        _panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(string message)
    {
        if (_isShown) return;
        _isShown = true;

        _messageText.text = message;
        _panel.SetActive(true);

        foreach (PlayerInput player in PlayerInput.all)
        {
            player.DeactivateInput();
        }

        if (_returnToMenuDelay > 0f)
        {
            StartCoroutine(ReturnToMenu());
        }
    }

    IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(_returnToMenuDelay);

        if (GameManager.Instance != null) GameManager.Instance.ClearSavedAllies();

        SceneLoadingScreen.LoadScene(_menuScene);
    }
}