using UnityEngine;
using UnityEngine.AI;
using Unity.VisualScripting;
public class PNJAlly : MonoBehaviour
{
    [SerializeField] NavMeshAgent _navAgent;

    GameObject _Owner;
    PlayerCommander _PlayerCommander;
    GameObject _combatTarget;
    Collider[] _combatColliders = new Collider[32];
    float _nextCombatRefreshTime;
    const float CombatSearchRadius = 10.5f;
    const float CombatRefreshInterval = 0.2f;

    public GameObject Owner
    { get { return _Owner; }  private set { _Owner = value; } }

    public PlayerCommander PlayerCommander
    { get { return _PlayerCommander; } private set { _PlayerCommander = value; } }

    public Transform FollowPoint
    { get { return PlayerCommander != null ? PlayerCommander.FollowPoint : null; } }

    public GameObject CombatTarget
    {
        get
        {
            if (!CanAttackTarget(_combatTarget)) _combatTarget = null;
            if (Time.time >= _nextCombatRefreshTime) RefreshCombatTarget();
            return _combatTarget;
        }
    }

    public NavMeshAgent NavAgent
        { get { return _navAgent; } private set { _navAgent = value; } }

    InteractInterface _CurrentTarget;
    public InteractInterface CurrentTarget
    { get { return _CurrentTarget; } private set { _CurrentTarget = value; } }

    Transform _TargetPoint;
    public Transform TargetPoint
    { get { return _TargetPoint; } private set { _TargetPoint = value; } }

    private void Awake()
    {
        if (_navAgent == null)
        {
            _navAgent = GetComponent<NavMeshAgent>();
        }

    }
    public void SetOwner(GameObject owner)
    {
        Owner = owner;
        ClearCombatTarget();
    }
    public void SetCommander(PlayerCommander commander)
    {
        _PlayerCommander = commander;
        ClearCombatTarget();
    }

    public bool CanAttackTarget(GameObject target)
    {
        if (target == null) return false;

        GameObject actor = target.transform.root.gameObject;
        if (actor == transform.root.gameObject || !actor.activeInHierarchy) return false;
        if (Owner != null && actor == Owner.transform.root.gameObject) return false;

        PNJAlly targetAlly = actor.GetComponentInChildren<PNJAlly>();
        if (Owner != null && targetAlly != null && targetAlly.Owner == Owner) return false;

        LifeSystem life = actor.GetComponentInChildren<LifeSystem>();
        if (life == null || !life.isActiveAndEnabled || life.IsDead) return false;

        return (PlayerCommander != null && PlayerCommander.IsDuel) || actor.CompareTag("Enemy");
    }

    void RefreshCombatTarget()
    {
        _nextCombatRefreshTime = Time.time + CombatRefreshInterval;
        int count;
        do
        {
            count = Physics.OverlapSphereNonAlloc(transform.position, CombatSearchRadius,
                _combatColliders, Physics.AllLayers, QueryTriggerInteraction.Collide);
            if (count < _combatColliders.Length) break;
            System.Array.Resize(ref _combatColliders, _combatColliders.Length * 2);
        }
        while (true);

        _combatTarget = null;
        float closestDistance = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            Collider candidate = _combatColliders[i];
            if (candidate == null) continue;
            GameObject actor = candidate.transform.root.gameObject;
            if (!CanAttackTarget(actor)) continue;

            Vector3 offset = actor.transform.position - transform.position;
            offset.y = 0f;
            float distance = offset.sqrMagnitude;
            if (distance >= closestDistance) continue;
            closestDistance = distance;
            _combatTarget = actor;
        }
    }

    void ClearCombatTarget()
    {
        _combatTarget = null;
        _nextCombatRefreshTime = 0f;
    }

    private void OnDisable()
    {
        ClearCombatTarget();
        if (CurrentTarget != null)
        {
            CurrentTarget.CancelReservation(this);
        }
        CurrentTarget = null;
        TargetPoint = null;
        if(Owner != null && PlayerCommander != null)
        {
            PlayerCommander.RemoveAlly(this);
        }

    }

    public void InteractionFinished() 
    {
        CancelOrder();
    }

    public bool OrderInteract(InteractInterface target)
    {
        if(target == null) return false;
        if(CurrentTarget == target) return true;

        if(target.TryReserve(this, out Transform point))
        {
            TargetPoint = point;    

            if( CurrentTarget != null)
            {
                CurrentTarget.CancelReservation(this);
            }
            CurrentTarget = target;
            Variables.Object(gameObject).Set("HasInteractTarget", true);
            return true;

        }
        return false;
    }

    public void CancelOrder()
    {
        if (CurrentTarget != null)
        {
            CurrentTarget.CancelReservation(this);
        }
        Variables.Object(gameObject).Set("HasInteractTarget", false); 
        CurrentTarget = null;
        TargetPoint = null;
    }

}
