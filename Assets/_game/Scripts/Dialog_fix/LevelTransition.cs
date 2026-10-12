using ALTUSHKA.EasyNodeDialogSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] string _nextScene;

    [SerializeField] bool _keepAllies = true;

    [SerializeField] GameObject _nextNecromancianPrefab;
    [SerializeField] GameObject _nextMagicalGirlPrefab;

    [SerializeField] string _afterDialog = "L1_Fin";

    bool _isLoading;

    void Start()
    {
        if (!string.IsNullOrEmpty(_afterDialog) && DialogRunner.Instance != null)
        {
            DialogRunner.Instance.DialogEnded += OnDialogEnded;
        }
    }

    void OnDestroy()
    {
        if (DialogRunner.Instance != null)
        {
            DialogRunner.Instance.DialogEnded -= OnDialogEnded;
        }
    }

    void OnDialogEnded(DialogSessionInfo info)
    {
        if (info.DialogName == _afterDialog)
        {
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        if (_isLoading) return;
        _isLoading = true;

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            if (_keepAllies) gameManager.SaveAllies();
            else gameManager.ClearSavedAllies();

            gameManager.SetNextPlayerPrefabs(_nextNecromancianPrefab, _nextMagicalGirlPrefab);
        }

        SceneLoadingScreen.LoadScene(_nextScene);
    }
}