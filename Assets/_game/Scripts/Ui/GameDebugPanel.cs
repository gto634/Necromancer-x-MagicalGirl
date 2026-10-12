using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

// Automatically available in the Editor and Development Builds, from any scene.
public class GameDebugPanel : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] Key _toggleKey = Key.F3;
    [SerializeField] string _level1Scene = "Level1";
    [SerializeField] string _level2Scene = "Level2";
    [SerializeField] string _homeScene = "HomeScreen";
    [SerializeField] GameObject _coopNecPrefab;
    [SerializeField] GameObject _coopMgPrefab;
    [SerializeField] GameObject _pvpNecPrefab;
    [SerializeField] GameObject _pvpMgPrefab;
    [SerializeField] GameObject _coopNecAllyPrefab;
    [SerializeField] GameObject _coopMgAllyPrefab;
    [SerializeField] GameObject _pvpNecAllyPrefab;
    [SerializeField] GameObject _pvpMgAllyPrefab;

    static GameDebugPanel _instance;
    readonly List<PlayerInput> _blockedInputs = new List<PlayerInput>(2);
    readonly List<InputSystemUIInputModule> _blockedUi = new List<InputSystemUIInputModule>();
    readonly bool[] _playerInvincible = new bool[2];
    readonly bool[] _alliesInvincible = new bool[2];
    readonly string[] _playerNames = { "Necromancer", "Magical Girl" };
    readonly string[] _quickStartNames = { "NEC seul", "MG seule", "Duo" };
    bool _open;
    bool _keepAllies = true;
    bool _keepPlayers = true;
    float _previousTimeScale;
    CursorLockMode _previousCursorLock;
    bool _previousCursorVisible;
    float _nextRefresh;
    int _selectedPlayer;
    int _quickStart;
    string _healthText = "100";
    string _countText = "5";
    string _status = "F3 : ouvrir/fermer. Le jeu est en pause pendant l'utilisation du panneau.";
    GameObject _currentPlayer;
    LifeSystem _life;
    PlayerCommander _commander;
    Rect _window = new Rect(20f, 20f, 520f, 660f);
    Vector2 _scroll;
    GUIStyle _titleStyle;
    GUIStyle _statusStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { _instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        GameObject prefab = Resources.Load<GameObject>("Debug/GameDebugPanel");
        if (prefab != null) Instantiate(prefab);
        else Debug.LogWarning("Debug panel: Resources/Debug/GameDebugPanel prefab is missing.");
    }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Close();
    }
    void OnDestroy() { if (_instance == this) _instance = null; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _currentPlayer = null;
        _life = null;
        _commander = null;
        _nextRefresh = 0f;
    }

    void Update()
    {
        if (SceneLoadingScreen.IsLoading) { Close(); return; }
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[_toggleKey].wasPressedThisFrame)
        {
            if (_open) Close(); else Open();
        }
        else if (_open && keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Close();

        if (Time.unscaledTime >= _nextRefresh)
        {
            _nextRefresh = Time.unscaledTime + .25f;
            RefreshPlayer();
            ApplyInvincibility();
        }
    }

    void LateUpdate()
    {
        if (!_open || GameManager.Instance == null) return;
        BlockInput(GameManager.Instance.necromancianInput);
        BlockInput(GameManager.Instance.magicalGirlInput);
    }

    GameObject GetPlayer(int index)
    {
        GameManager manager = GameManager.Instance;
        return manager == null ? null : index == 0 ? manager.Necromancian : manager.MagicalGirl;
    }

    void RefreshPlayer()
    {
        GameObject player = GetPlayer(_selectedPlayer);
        if (player == null)
        {
            _currentPlayer = null;
            _life = null;
            _commander = null;
            return;
        }
        if (_currentPlayer == player) return;
        _currentPlayer = player;
        _life = player != null ? player.GetComponentInChildren<LifeSystem>(true) : null;
        _commander = player != null ? player.GetComponentInChildren<PlayerCommander>(true) : null;
        _healthText = _life != null ? _life.Health.ToString() : "100";
    }

    public void SelectPlayer(int index)
    {
        _selectedPlayer = Mathf.Clamp(index, 0, 1);
        _currentPlayer = null;
        _life = null;
        _commander = null;
        RefreshPlayer();
    }

    void ApplyInvincibility()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject player = GetPlayer(i);
            if (player == null) continue;
            LifeSystem life = player.GetComponentInChildren<LifeSystem>(true);
            if (life != null) life.DebugInvincible = _playerInvincible[i];
            PlayerCommander commander = player.GetComponentInChildren<PlayerCommander>(true);
            if (commander == null) continue;
            foreach (PNJAlly ally in commander.Allies)
            {
                if (ally == null) continue;
                LifeSystem allyLife = ally.GetComponentInChildren<LifeSystem>(true);
                if (allyLife != null) allyLife.DebugInvincible = _alliesInvincible[i];
            }
        }
    }

    public void Open()
    {
        if (_open || SceneLoadingScreen.IsLoading) return;
        RefreshPlayer();
        _previousTimeScale = Time.timeScale;
        _previousCursorLock = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        _open = true;
        foreach (InputSystemUIInputModule module in FindObjectsByType<InputSystemUIInputModule>(FindObjectsSortMode.None))
        {
            if (!module.enabled) continue;
            _blockedUi.Add(module);
            module.enabled = false;
        }
        if (GameManager.Instance == null) return;
        BlockInput(GameManager.Instance.necromancianInput);
        BlockInput(GameManager.Instance.magicalGirlInput);
    }

    void BlockInput(PlayerInput input)
    {
        if (input == null || !input.inputIsActive || _blockedInputs.Contains(input)) return;
        _blockedInputs.Add(input);
        input.DeactivateInput();
    }

    public void Close()
    {
        if (!_open) return;
        _open = false;
        Time.timeScale = _previousTimeScale;
        Cursor.lockState = _previousCursorLock;
        Cursor.visible = _previousCursorVisible;
        foreach (PlayerInput input in _blockedInputs)
        {
            if (input == null) continue;
            LifeSystem life = input.transform.root.GetComponentInChildren<LifeSystem>(true);
            if (life == null || !life.IsDead) input.ActivateInput();
        }
        _blockedInputs.Clear();
        foreach (InputSystemUIInputModule module in _blockedUi)
            if (module != null) module.enabled = true;
        _blockedUi.Clear();
    }

    public void SetPlayerHealth(int health)
    {
        RefreshPlayer();
        if (_life == null) return;
        _life.DebugSetHealth(health);
        _healthText = _life.Health.ToString();
        _status = _playerNames[_selectedPlayer] + " : " + _life.Health + "/" + _life.MaxHealth + " PV.";
    }

    public void SetPlayerInvincible(bool value)
    {
        _playerInvincible[_selectedPlayer] = value;
        ApplyInvincibility();
    }

    public void SetAlliesInvincible(bool value)
    {
        _alliesInvincible[_selectedPlayer] = value;
        ApplyInvincibility();
    }

    public void SetAlliesHealth(int health)
    {
        RefreshPlayer();
        if (_commander == null) return;
        // Setting zero PV can remove an ally synchronously through its death feedback.
        foreach (PNJAlly ally in _commander.Allies.ToArray())
        {
            if (ally == null) continue;
            LifeSystem life = ally.GetComponentInChildren<LifeSystem>(true);
            if (life != null) life.DebugSetHealth(health);
        }
        _status = "PV des sbires mis à jour.";
    }

    public int AddAllies(int count)
    {
        RefreshPlayer();
        if (_commander == null || _currentPlayer == null) return 0;
        bool pvp = _currentPlayer.GetComponentInChildren<PlayerDeathFeedback>(true) != null;
        GameObject prefab = _selectedPlayer == 0
            ? (pvp ? _pvpNecAllyPrefab : _coopNecAllyPrefab)
            : (pvp ? _pvpMgAllyPrefab : _coopMgAllyPrefab);
        if (prefab == null || !prefab.TryGetComponent(out NavMeshAgent template))
        {
            _status = "Prefab de sbire ou NavMeshAgent manquant.";
            return 0;
        }
        count = Mathf.Clamp(count, 1, 100);
        NavMeshQueryFilter filter = new NavMeshQueryFilter { agentTypeID = template.agentTypeID, areaMask = template.areaMask };
        int added = 0;
        for (int i = 0; i < count; i++)
        {
            float angle = (i + _commander.Allies.Count) * 2.399963f;
            Vector3 wanted = _currentPlayer.transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 2f;
            if (!NavMesh.SamplePosition(wanted, out NavMeshHit hit, 4f, filter)) continue;
            GameObject ally = Instantiate(prefab, hit.position, Quaternion.identity);
            _commander.Convert(ally);
            LifeSystem life = ally.GetComponentInChildren<LifeSystem>(true);
            if (life != null) life.DebugInvincible = _alliesInvincible[_selectedPlayer];
            added++;
        }
        _status = added + " sbire(s) ajouté(s)." + (added < count ? " Pas de NavMesh valide pour les autres." : "");
        return added;
    }

    public int RemoveAllies(int count)
    {
        RefreshPlayer();
        if (_commander == null) return 0;
        int removed = 0;
        PNJAlly[] allies = _commander.Allies.ToArray();
        for (int i = allies.Length - 1; i >= 0 && removed < count; i--)
        {
            PNJAlly ally = allies[i];
            if (ally == null) continue;
            ally.CancelOrder();
            _commander.RemoveAlly(ally);
            ally.gameObject.SetActive(false);
            Destroy(ally.gameObject);
            removed++;
        }
        _status = removed + " sbire(s) retiré(s).";
        return removed;
    }

    bool PreparePlayers(out InputDevice[] devices, out List<int> characters)
    {
        List<InputDevice> chosenDevices = new List<InputDevice>(2);
        characters = new List<int>(2);
        if (_keepPlayers && GameManager.Instance != null)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject player = GetPlayer(i);
                if (player == null) continue;
                PlayerInput input = player.GetComponentInChildren<PlayerInput>(true);
                if (input == null) continue;
                foreach (InputDevice device in input.devices)
                {
                    if (!device.added || (!(device is Keyboard) && !(device is Gamepad)) || chosenDevices.Contains(device)) continue;
                    chosenDevices.Add(device);
                    characters.Add(i == 0 ? -1 : 1);
                    break;
                }
            }
        }
        if (chosenDevices.Count == 0)
        {
            if (Keyboard.current != null) chosenDevices.Add(Keyboard.current);
            foreach (Gamepad gamepad in Gamepad.all)
            {
                if (chosenDevices.Count >= (_quickStart == 2 ? 2 : 1)) break;
                chosenDevices.Add(gamepad);
            }
            if (chosenDevices.Count < (_quickStart == 2 ? 2 : 1))
            {
                devices = null;
                _status = "Duo : clavier + manette ou deux manettes nécessaires. Le mode solo accepte le clavier ou une manette.";
                return false;
            }
            characters.Add(_quickStart == 1 ? 1 : -1);
            if (_quickStart == 2) characters.Add(1);
        }
        devices = chosenDevices.ToArray();
        return true;
    }

    public bool LoadLevel(string sceneName)
    {
        if (SceneLoadingScreen.IsLoading || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            _status = "Scène indisponible ou chargement en cours : " + sceneName;
            return false;
        }
        bool home = sceneName == _homeScene;
        InputDevice[] devices = null;
        List<int> characters = null;
        if (!home && !PreparePlayers(out devices, out characters)) return false;
        Close(); // Restore time before the loading screen captures its previous time scale.
        GameManager manager = GameManager.Instance;
        bool pvp = sceneName == _level2Scene;
        // Level2 normally receives this persistent manager from Level1.
        if (!home && manager == null) manager = new GameObject("GameManager (Debug)").AddComponent<GameManager>();
        if (manager != null)
        {
            if (!home) manager.DebugConfigureAllyPrefabs(pvp ? _pvpNecAllyPrefab : _coopNecAllyPrefab,
                pvp ? _pvpMgAllyPrefab : _coopMgAllyPrefab);
            if (!home && _keepAllies) manager.SaveAllies(); else manager.ClearSavedAllies();
            manager.SetNextPlayerPrefabs(home ? null : pvp ? _pvpNecPrefab : _coopNecPrefab,
                home ? null : pvp ? _pvpMgPrefab : _coopMgPrefab);
        }
        ConnectPlayer.SetPlayers(devices, characters);
        bool loaded = SceneLoadingScreen.LoadScene(sceneName);
        if (!loaded) { _status = "Le chargement a échoué."; Open(); }
        return loaded;
    }

    void OnGUI()
    {
        if (SceneLoadingScreen.IsLoading) return;
        Matrix4x4 previousMatrix = GUI.matrix;
        int previousDepth = GUI.depth;
        float scale = Mathf.Clamp(Mathf.Min(Screen.width / 1280f, Screen.height / 720f), .5f, 1.5f);
        GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
        GUI.depth = -1000;
        if (!_open)
        {
            if (GUI.Button(new Rect(Screen.width / scale - 155f, 10f, 145f, 30f), "Debug [" + _toggleKey + "]")) Open();
        }
        else
        {
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 16 };
                _statusStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            }
            _window.width = Mathf.Min(520f, Screen.width / scale - 20f);
            _window.height = Mathf.Min(660f, Screen.height / scale - 20f);
            _window.x = Mathf.Clamp(_window.x, 0f, Screen.width / scale - _window.width);
            _window.y = Mathf.Clamp(_window.y, 0f, Screen.height / scale - _window.height);
            _window = GUI.Window(GetInstanceID(), _window, DrawWindow, "Debug — jeu en pause");
        }
        GUI.matrix = previousMatrix;
        GUI.depth = previousDepth;
    }

    void DrawWindow(int id)
    {
        _scroll = GUILayout.BeginScrollView(_scroll);
        GUILayout.Label("Scène : " + SceneManager.GetActiveScene().name, _titleStyle);
        _keepPlayers = GUILayout.Toggle(_keepPlayers, "Conserver les joueurs et leurs périphériques");
        GUILayout.Label("Si aucun joueur, ou pour une nouvelle sélection :");
        _quickStart = GUILayout.Toolbar(_quickStart, _quickStartNames);
        _keepAllies = GUILayout.Toggle(_keepAllies, "Conserver les sbires au changement de niveau");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Level1", GUILayout.Height(30f))) LoadLevel(_level1Scene);
        if (GUILayout.Button("Level2 direct", GUILayout.Height(30f))) LoadLevel(_level2Scene);
        if (GUILayout.Button("Recharger", GUILayout.Height(30f))) LoadLevel(SceneManager.GetActiveScene().name);
        if (GUILayout.Button("Accueil", GUILayout.Height(30f))) LoadLevel(_homeScene);
        GUILayout.EndHorizontal();
        GUILayout.Space(12f);
        int selected = GUILayout.Toolbar(_selectedPlayer, _playerNames);
        if (selected != _selectedPlayer) SelectPlayer(selected);
        bool previousEnabled = GUI.enabled;
        GUI.enabled = _life != null;
        GUILayout.Label(_life == null ? "Ce personnage n'est pas présent dans la scène." : "PV : " + _life.Health + " / " + _life.MaxHealth, _titleStyle);
        bool invincible = GUILayout.Toggle(_playerInvincible[_selectedPlayer], "Joueur invincible (dégâts ignorés)");
        if (invincible != _playerInvincible[_selectedPlayer]) SetPlayerInvincible(invincible);
        GUILayout.BeginHorizontal();
        GUILayout.Label("PV exacts", GUILayout.Width(70f));
        _healthText = GUILayout.TextField(_healthText, 7, GUILayout.Width(75f));
        if (GUILayout.Button("Appliquer") && int.TryParse(_healthText, out int health)) SetPlayerHealth(health);
        if (GUILayout.Button("PV max")) SetPlayerHealth(_life.MaxHealth);
        if (GUILayout.Button("-10 PV")) _life.TakeDamage(10);
        GUILayout.EndHorizontal();
        GUILayout.Label("Les PV exacts peuvent être forcés même en invincibilité. 0 PV déclenche la mort normale.", _statusStyle);
        GUILayout.Space(12f);
        GUI.enabled = _commander != null;
        GUILayout.Label("Sbires : " + (_commander != null ? _commander.Allies.Count : 0), _titleStyle);
        bool alliesInvincible = GUILayout.Toggle(_alliesInvincible[_selectedPlayer], "Sbires invincibles (actuels et nouveaux)");
        if (alliesInvincible != _alliesInvincible[_selectedPlayer]) SetAlliesInvincible(alliesInvincible);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Quantité", GUILayout.Width(70f));
        _countText = GUILayout.TextField(_countText, 3, GUILayout.Width(75f));
        if (GUILayout.Button("Ajouter") && int.TryParse(_countText, out int add)) AddAllies(add);
        if (GUILayout.Button("Enlever") && int.TryParse(_countText, out int remove)) RemoveAllies(Mathf.Clamp(remove, 1, 100));
        if (GUILayout.Button("Tout enlever")) RemoveAllies(int.MaxValue);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Soigner les sbires")) SetAlliesHealth(int.MaxValue);
        if (GUILayout.Button("Sbires à 1 PV")) SetAlliesHealth(1);
        GUILayout.EndHorizontal();
        GUI.enabled = previousEnabled;
        GUILayout.Space(12f);
        GUILayout.Label(_status, _statusStyle);
        GUILayout.Label("Après une mort en PvP, recharger le niveau pour réinitialiser le personnage.", _statusStyle);
        if (GUILayout.Button("Fermer [F3 / Échap]", GUILayout.Height(30f))) Close();
        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0f, 0f, _window.width, 22f));
    }
#endif
}
