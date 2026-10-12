using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private GameObject _necromancian;
    private GameObject _magicalGirl;

    private PlayerInput _necromancianInput;
    private PlayerInput _magicalGirlInput;

    [SerializeField] GameObject _necromancianAllyPrefab;
    [SerializeField] GameObject _magicalGirlAllyPrefab;
    [SerializeField, Min(0f)] float _allySpawnRadius = 2f;

    readonly List<GameObject> _savedNecromancianAllies = new List<GameObject>();
    readonly List<GameObject> _savedMagicalGirlAllies = new List<GameObject>();

    public static GameManager Instance => _instance;

    public GameObject Necromancian
    {
        get => _necromancian;
        set => _necromancian = value;
    }

    public GameObject MagicalGirl
    {
        get => _magicalGirl;
        set => _magicalGirl = value;
    }

    public PlayerInput necromancianInput
    {
        get => _necromancianInput;
        set => _necromancianInput = value;
    }

    public PlayerInput magicalGirlInput
    {
        get => _magicalGirlInput;
        set => _magicalGirlInput = value;
    }

    public GameObject NextNecromancianPrefab { get; private set; }
    public GameObject NextMagicalGirlPrefab { get; private set; }

    private void Awake()
    {
        Application.targetFrameRate = 144;
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(gameObject);
    }

    #region Prefabs des joueurs

    public void SetNextPlayerPrefabs(GameObject necromancian, GameObject magicalGirl)
    {
        NextNecromancianPrefab = necromancian;
        NextMagicalGirlPrefab = magicalGirl;
    }

    public void ClearNextPlayerPrefabs()
    {
        NextNecromancianPrefab = null;
        NextMagicalGirlPrefab = null;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugConfigureAllyPrefabs(GameObject necromancian, GameObject magicalGirl)
    {
        _necromancianAllyPrefab = necromancian;
        _magicalGirlAllyPrefab = magicalGirl;
    }
#endif

    #endregion

    #region Sauvegarde des alliés

    public void SaveAllies()
    {
        SaveAlliesOf(_necromancian, _savedNecromancianAllies);
        SaveAlliesOf(_magicalGirl, _savedMagicalGirlAllies);
    }

    public void ClearSavedAllies()
    {
        _savedNecromancianAllies.Clear();
        _savedMagicalGirlAllies.Clear();
    }

    public void RestoreAllies(GameObject player, bool isNecromancian)
    {
        List<GameObject> savedAllies = isNecromancian ? _savedNecromancianAllies : _savedMagicalGirlAllies;
        if (savedAllies.Count == 0) return;

        PlayerCommander commander = player.GetComponentInChildren<PlayerCommander>();
        if (commander == null)
        {
            Debug.LogWarning($"GameManager : pas de PlayerCommander sur {player.name}, alliés non restaurés.", player);
            return;
        }

        for (int i = 0; i < savedAllies.Count; i++)
        {
            Vector3 position = FindAllySpawnPoint(player.transform.position, savedAllies[i], i, savedAllies.Count);
            GameObject ally = Instantiate(savedAllies[i], position, Quaternion.identity);
            commander.Convert(ally);
        }

        savedAllies.Clear();
    }

    void SaveAlliesOf(GameObject player, List<GameObject> savedAllies)
    {
        savedAllies.Clear();
        if (player == null) return;

        PlayerCommander commander = player.GetComponentInChildren<PlayerCommander>();
        if (commander == null) return;

        foreach (PNJAlly ally in commander.Allies)
        {
            if (ally == null) continue;

            LifeSystem life = ally.GetComponentInChildren<LifeSystem>();
            if (life != null && life.IsDead) continue;

            GameObject prefab = GetAllyPrefab(ally.gameObject);
            if (prefab != null) savedAllies.Add(prefab);
        }
    }

    GameObject GetAllyPrefab(GameObject ally)
    {
        if (ally.CompareTag("AllyNEC")) return _necromancianAllyPrefab;
        if (ally.CompareTag("AllyMG")) return _magicalGirlAllyPrefab;

        Debug.LogWarning($"GameManager : {ally.name} n'a ni le tag AllyNEC ni AllyMG, il ne sera pas gardé.", ally);
        return null;
    }

    // Répartit les alliés en cercle autour du joueur, sur le NavMesh de leur type d'agent
    Vector3 FindAllySpawnPoint(Vector3 center, GameObject allyPrefab, int index, int count)
    {
        float angle = index * Mathf.PI * 2f / count;
        Vector3 wanted = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _allySpawnRadius;

        NavMeshQueryFilter filter = new NavMeshQueryFilter { areaMask = NavMesh.AllAreas };
        if (allyPrefab.TryGetComponent(out NavMeshAgent agent))
        {
            filter.agentTypeID = agent.agentTypeID;
        }

        if (NavMesh.SamplePosition(wanted, out NavMeshHit hit, _allySpawnRadius + 2f, filter))
        {
            return hit.position;
        }
        return wanted;
    }

    #endregion
}