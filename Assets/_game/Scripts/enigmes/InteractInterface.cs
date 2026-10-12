using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class InteractInterface : MonoBehaviour
{
    [SerializeField] InputActionReference _Interact;
    [SerializeField] int _numberOfPNJRequiredToInteract;
    [SerializeField] List<Transform> _interactionPointList = new List<Transform>();
    [SerializeField] Renderer[] _rendererToHighlight;
    int[] _rendererCalques;
    int _highlightCalque;

    Dictionary<PNJAlly, Transform> _AllyReservationPoint = new Dictionary<PNJAlly, Transform>();
    List<PNJAlly> _AllyArrived = new List<PNJAlly>();

    Dictionary<PlayerInput, InputAction> _PlayerActionDict = new Dictionary<PlayerInput, InputAction>();

    readonly public static List<InteractInterface> _InteractInterfaceList = new List<InteractInterface>();
    NavMeshPath _path ;


    public InputActionReference Interact { get { return _Interact; } }

    virtual public void OnInteract(InputAction.CallbackContext context)
    {

    }

    virtual public void OnPNJInteract()
    {

    }
    private void Awake()
    {
        _path = new NavMeshPath();
        _highlightCalque = LayerMask.NameToLayer("CommandHighlight");
        if (_highlightCalque == -1)
            Debug.LogError("Le calque 'CommandHighlight' n'existe pas ", this);
        _rendererCalques = new int[_rendererToHighlight.Length];
        for (int i = 0; i < _rendererToHighlight.Length; i++)
        {
            _rendererCalques[i] = _rendererToHighlight[i].gameObject.layer;
        }

    }
    private void OnEnable()
    {
        _InteractInterfaceList.Add(this);   
    }

    private void OnDisable()
    {
       foreach (var action in _PlayerActionDict.Values)
        {
            action.performed -= OnInteract;
        }
        _PlayerActionDict.Clear();
        _InteractInterfaceList.Remove(this);

    }
    public bool TryReserve(PNJAlly pnj, out Transform point)
    {
        if (_AllyReservationPoint.ContainsKey(pnj))
        {
            point = _AllyReservationPoint[pnj];
            return true;
        }

        if (HasFreeSlot())
        {
            foreach (Transform interactionPoint in _interactionPointList)
            {
                if (!_AllyReservationPoint.ContainsValue(interactionPoint))
                {
                    point = interactionPoint;
                    _AllyReservationPoint.Add(pnj, point);
                    return true;
                }
            }
        }
        point = null;
        return false;
    }
    public void PNJArrived(PNJAlly pnj)
    {
        if(_AllyReservationPoint.ContainsKey(pnj) && !_AllyArrived.Contains(pnj))
        {
            _AllyArrived.Add(pnj);
        }
        else
        {
            return;
        }

        if (_AllyArrived.Count >= _numberOfPNJRequiredToInteract)
        {
            OnPNJInteract();
            List<PNJAlly>  templist = new List<PNJAlly>(_AllyArrived);
            foreach (PNJAlly ally in templist)
            {
                ally.InteractionFinished();
                CancelReservation(ally);
            }
        }

    }

    public void CancelReservation(PNJAlly pnj)
    {
        if (_AllyReservationPoint.ContainsKey(pnj))
        {
            _AllyReservationPoint.Remove(pnj);
        }
        if(_AllyArrived.Contains(pnj))
        {
            _AllyArrived.Remove(pnj);
        }

    }
    public bool HasFreeSlot()
    {
        if(_AllyReservationPoint.Count < _numberOfPNJRequiredToInteract)
        {             return true;
        }
        else { 
            return false;
        }
    }
    public bool IsReachableBy(NavMeshAgent agent)
    {

        foreach (Transform interactionPoint in _interactionPointList)
        {
            if (!_AllyReservationPoint.ContainsValue(interactionPoint))
            {
                if (agent.CalculatePath(interactionPoint.position, _path))
                {
                    if (_path.status == NavMeshPathStatus.PathComplete)
                    {
                        return true;
                    }
                }
            }
                
        }
        return false;
    }
    public void SetHighlighted(bool on)
    {
        if (_highlightCalque == -1) return;

        for (int i = 0; i < _rendererToHighlight.Length; i++)
        {
            if (on)
            {
                _rendererToHighlight[i].gameObject.layer = _highlightCalque;
            }
            else
            {
                _rendererToHighlight[i].gameObject.layer = _rendererCalques[i];
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerInteractor"))
        {
            Debug.Log("enter");

            Debug.Log(other.transform.root.gameObject.name);
            if (other.transform.root.gameObject == GameManager.Instance.Necromancian)
            {
                if (!_PlayerActionDict.ContainsKey(GameManager.Instance.necromancianInput))
                {
                    _PlayerActionDict[GameManager.Instance.necromancianInput] = GameManager.Instance.necromancianInput.actions.FindAction(Interact.action.id);
                    _PlayerActionDict[GameManager.Instance.necromancianInput].performed += OnInteract;

                }

            }
            else if (other.transform.root.gameObject == GameManager.Instance.MagicalGirl)
            {
                if (!_PlayerActionDict.ContainsKey(GameManager.Instance.magicalGirlInput))
                {
                    _PlayerActionDict[GameManager.Instance.magicalGirlInput] = GameManager.Instance.magicalGirlInput.actions.FindAction(Interact.action.id);
                    _PlayerActionDict[GameManager.Instance.magicalGirlInput].performed += OnInteract;
                }

            }
        }
     
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerInteractor"))
        {
            Debug.Log("exit");
            Debug.Log(other.transform.root.gameObject.name);
            if (other.transform.root.gameObject == GameManager.Instance.Necromancian)
            {
                if (_PlayerActionDict.ContainsKey(GameManager.Instance.necromancianInput))
                {
                    _PlayerActionDict[GameManager.Instance.necromancianInput].performed -= OnInteract;
                    _PlayerActionDict.Remove(GameManager.Instance.necromancianInput);
                }
            }
            else if (other.transform.root.gameObject == GameManager.Instance.MagicalGirl)
            {
                if (_PlayerActionDict.ContainsKey(GameManager.Instance.magicalGirlInput))
                {
                    _PlayerActionDict[GameManager.Instance.magicalGirlInput].performed -= OnInteract;
                    _PlayerActionDict.Remove(GameManager.Instance.magicalGirlInput);

                }
            }
        }
    }
}
