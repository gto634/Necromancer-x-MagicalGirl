using System;
using UnityEngine;
using UnityEngine.AI;

public class ConvertSystem : MonoBehaviour
{

    [SerializeField] GameObject _ModelNEC_ally;
    [SerializeField] GameObject _ModelMG_ally;
    [SerializeField] LifeSystem _lifeSystem;

    PNJAlly _ally;
    bool _hasConverted;

    [SerializeField] DeathFeedback _Feedback;

    public Action<GameObject> OnConvert;

    void Awake()
    {
        if (_lifeSystem == null) _lifeSystem = transform.root.GetComponentInChildren<LifeSystem>();
        _ally = transform.root.GetComponent<PNJAlly>();
    }

    void OnEnable()
    {
        if (_lifeSystem != null) _lifeSystem.OnDeath += OnDeath;
    }

    void OnDisable()
    {
        if (_lifeSystem != null) _lifeSystem.OnDeath -= OnDeath;
    }

    void OnDeath()
    {
        if (_ally == null) _ally = transform.root.GetComponent<PNJAlly>();
        if (_ally == null || _ally.Owner == null || _ally.PlayerCommander == null
            || !_ally.PlayerCommander.IsDuel) return;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;
        GameObject opponent = _ally.Owner == gameManager.Necromancian
            ? gameManager.MagicalGirl : _ally.Owner == gameManager.MagicalGirl ? gameManager.Necromancian : null;
        ConvertTo(opponent);
    }

    public void onDeathConvert(GameObject Killer)
    {
        if (_hasConverted || _lifeSystem == null || !_lifeSystem.IsDead) return;
        if (_ally == null) _ally = transform.root.GetComponent<PNJAlly>();
        if (_ally != null && _ally.Owner != null) return;
        if (Killer == null) return;

        Projectile projectile = Killer.GetComponent<Projectile>();
        if (projectile != null && projectile.Owner != null) Killer = projectile.Owner.root.gameObject;
        PNJAlly killerAlly = Killer.GetComponent<PNJAlly>();
        if (killerAlly != null && killerAlly.Owner != null) Killer = killerAlly.Owner;
        ConvertTo(Killer);
    }

    void ConvertTo(GameObject master)
    {
        if (_hasConverted || master == null) return;
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;

        GameObject prefab = master == gameManager.Necromancian ? _ModelNEC_ally
            : master == gameManager.MagicalGirl ? _ModelMG_ally : null;
        PlayerCommander commander = master.GetComponentInChildren<PlayerCommander>();
        if (prefab == null || commander == null) return;

        _hasConverted = true;
        Transform actualTransform = transform.root;
        Vector3 position = actualTransform.position;
        NavMeshAgent agent = prefab.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 3f, filter)) position = hit.position;
        }

        GameObject newModel = Instantiate(prefab, position, actualTransform.rotation);
        commander.Convert(newModel);
        if (_ally != null)
        {
            _ally.CancelOrder();
            if (_ally.PlayerCommander != null) _ally.PlayerCommander.RemoveAlly(_ally);
        }

        OnConvert?.Invoke(master);
        Destroy(actualTransform.gameObject);
    }
}
