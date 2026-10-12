using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCommander : MonoBehaviour
{

    [SerializeField] StateGraphAsset _allyStateGraph;
    [SerializeField] Transform _followPoint;
    [SerializeField, Min(0f)] float _followDistance = 2f;
    [SerializeField] bool _isDuel;
    List<PNJAlly> _allies = new List<PNJAlly>();
    readonly List<Vector3> _followPositions = new List<Vector3>(64);
    Transform _playerTransform;
    const float FollowSampleDistance = 0.05f;

    public bool IsDuel => _isDuel;

    public Transform FollowPoint
    {
        get
        {
            UpdateFollowPoint();
            return _followPoint;
        }
    }

    void OnEnable()
    {
        ResetFollowTrail();
    }

    void LateUpdate()
    {
        UpdateFollowPoint();
    }

    public void ResetFollowTrail()
    {
        _playerTransform = transform.root;
        _followPositions.Clear();
        _followPositions.Add(_playerTransform.position);
        if (_followPoint != null)
            _followPoint.position = _playerTransform.position;
    }

    void UpdateFollowPoint()
    {
        if (_followPoint == null) return;
        if (_playerTransform == null || _followPositions.Count == 0)
            ResetFollowTrail();

        Vector3 position = _playerTransform.position;
        if (_followDistance <= 0f)
        {
            _followPositions.Clear();
            _followPositions.Add(position);
            _followPoint.position = position;
            return;
        }

        Vector3 lastSample = _followPositions[_followPositions.Count - 1];
        if ((position - lastSample).sqrMagnitude >= FollowSampleDistance * FollowSampleDistance)
            _followPositions.Add(position);

        float remainingDistance = _followDistance;
        Vector3 newerPosition = position;
        for (int i = _followPositions.Count - 1; i >= 0; i--)
        {
            Vector3 olderPosition = _followPositions[i];
            float segmentLength = Vector3.Distance(newerPosition, olderPosition);
            if (segmentLength >= remainingDistance)
            {
                _followPoint.position = Vector3.Lerp(newerPosition, olderPosition, remainingDistance / segmentLength);
                if (i > 0) _followPositions.RemoveRange(0, i);
                return;
            }
            remainingDistance -= segmentLength;
            newerPosition = olderPosition;
        }
        _followPoint.position = _followPositions[0];
    }

    public List<PNJAlly> Allies
    {
        get { return _allies; }

        private set { _allies = value; }
    }

    public void SendAllyTo(InteractInterface target) { 
    
        foreach (PNJAlly ally in Allies)
        {
            if (!ally.CurrentTarget)
            { 
                bool ok = ally.OrderInteract(target); 
                Debug.Log("OrderInteract -> " + ok);    
            }
        }
    }

    public PNJAlly Convert(GameObject enemy)
    {

        PNJAlly ally = enemy.GetComponent<PNJAlly>();
       
        if (!ally)
        {
            ally = enemy.AddComponent<PNJAlly>();
        
        }else if ( ally.PlayerCommander != null && ally.PlayerCommander != this)
        {
            ally.CancelOrder();
            ally.PlayerCommander.RemoveAlly(ally);
        }
         

        ally.SetOwner(transform.root.gameObject);
        ally.SetCommander(this);

        if (!_allies.Contains(ally))
        {
            _allies.Add(ally);
        }
        Variables.Object(enemy).Set("HasInteractTarget", false);
        enemy.GetComponent<StateMachine>().nest.SwitchToMacro(_allyStateGraph);
        return ally;
    }

    public void RemoveAlly(PNJAlly ally)
    {
        if (_allies.Contains(ally))
        {
            _allies.Remove(ally);
        }
    }

}
