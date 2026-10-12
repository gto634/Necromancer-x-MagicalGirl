using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class CommandMode : MonoBehaviour
{

    [SerializeField] PlayerInput _playerInput;
    [SerializeField] PlayerCommander _playerCommander ;
    [SerializeField] Camera _camera;

    [SerializeField] InputActionReference _CommandModeEnterRef;
    [SerializeField] InputActionReference _CommandModeConfirmRef;
    [SerializeField] InputActionReference _CommandModeExitRef;
    [SerializeField] InputActionReference _CommandModePointRef;
    [SerializeField] InputActionReference _CommandModeStickRef;

    [SerializeField] float _stickSpeed = 0.8f;
    [SerializeField] float _magnetRadius = 0.1f;

    [SerializeField] RectTransform _cursorUI;
    [SerializeField] RectTransform _markerUI;
    [SerializeField] float _markerHeight;
    InputAction _CommandModeEnter;
    InputAction _CommandModeConfirm;
    InputAction _CommandModeExit;
    InputAction _CommandModePoint; 
    InputAction _CommandModeStick;
    List<InteractInterface> interactInterfaces = new List<InteractInterface>();

    InteractInterface _selected = null;
    bool _active = false;
    Vector2 _cursor = Vector2.zero;

    public void SetCamera(Camera camera)
    {
        _camera = camera;
    }

    void Start()
    {
        _CommandModeEnter= _playerInput.actions.FindAction(_CommandModeEnterRef.action.id);
        _CommandModeConfirm =_playerInput.actions.FindAction(_CommandModeConfirmRef.action.id);
        _CommandModeExit= _playerInput.actions.FindAction(_CommandModeExitRef.action.id);
        _CommandModePoint = _playerInput.actions.FindAction(_CommandModePointRef.action.id);
        _CommandModeStick = _playerInput.actions.FindAction(_CommandModeStickRef.action.id);

        _CommandModeEnter.performed += OnCommandModeEnter;
        _CommandModeConfirm.performed += OnCommandModeConfirm;
        _CommandModeExit .performed += OnCommandModeExit;

    }
    private void OnDestroy()
    {
        if (_active)
        {
            _active = false;
            UpdateCameraEffects();
        }
        if (_CommandModeEnter == null) return;
        if (_CommandModeConfirm == null) return;
        if (_CommandModeExit == null) return;
        _CommandModeEnter.performed -= OnCommandModeEnter;
        _CommandModeConfirm.performed -= OnCommandModeConfirm;
        _CommandModeExit.performed -= OnCommandModeExit;
    }

    private void OnCommandModeExit(InputAction.CallbackContext context)
    {
        ExitMode();
    }

    private void OnCommandModeConfirm(InputAction.CallbackContext context)
    {
        if(_selected != null)
        {
            _playerCommander.SendAllyTo(_selected);
            ExitMode();
        }
    }

    private void OnCommandModeEnter(InputAction.CallbackContext context)
    {
        _cursorUI.gameObject.SetActive(true);
        _selected = null;
        _active = true;
        _cursor = new Vector2{ x = 0.5f, y = 0.5f };
        _playerInput.SwitchCurrentActionMap("Command");
        interactInterfaces.Clear();
        foreach (var interact in InteractInterface._InteractInterfaceList)
        {
            Vector3 PointtoCamera = _camera.WorldToViewportPoint(interact.transform.position);
            if (PointtoCamera.x > 0&& PointtoCamera.x <1
                && PointtoCamera.y > 0 && PointtoCamera.y < 1
                && PointtoCamera.z > 0&& interact.HasFreeSlot())
            {
                foreach(PNJAlly ally in _playerCommander.Allies)
                {
                    if (!interactInterfaces.Contains(interact) && ally.CurrentTarget == null&& interact.IsReachableBy(ally.NavAgent) )
                    {
                        interactInterfaces.Add(interact);
                        Debug.Log(interact.name + " is reachable by " + ally.name);
                    }
                }
            }
        }
        foreach (InteractInterface interact in interactInterfaces)
        {
            interact.SetHighlighted(true);
        }

        UpdateCameraEffects();
    }
    void Update()
    {
        if(!_active) return;
     
        if (_playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            _cursor = _camera.ScreenToViewportPoint(_CommandModePoint.ReadValue<Vector2>()); 
        } else if (_playerInput.currentControlScheme == "Gamepad")
        {
            _cursor += _CommandModeStick.ReadValue<Vector2>() * _stickSpeed * Time.deltaTime;

            _cursor.x= Mathf.Clamp01(_cursor.x);
            _cursor.y= Mathf.Clamp01(_cursor.y);

        }

        InteractInterface meilleur = null;
        float meilleurdistance = _magnetRadius;
        foreach (InteractInterface candidat in interactInterfaces)
        {

            Vector2 viewpos = _camera.WorldToViewportPoint(candidat.transform.position);
            float deltaX = (viewpos.x - _cursor.x) * _camera.aspect;
            float deltaXY = viewpos.y - _cursor.y;

            float distance = Mathf.Sqrt(deltaX * deltaX + deltaXY * deltaXY);

            if (distance< meilleurdistance)
            {
                meilleurdistance = distance;
                meilleur = candidat;
            }
        }
        _selected = meilleur;

        if (_selected != null)
        {
            // Le curseur s'aimante sur l'objet sélectionné
            _cursorUI.position = _camera.WorldToScreenPoint(_selected.transform.position);
            _markerUI.position = _camera.WorldToScreenPoint(_selected.transform.position + Vector3.up * _markerHeight);
            _markerUI.gameObject.SetActive(true);
        }
        else
        {
            _cursorUI.position = _camera.ViewportToScreenPoint(_cursor);
            _markerUI.gameObject.SetActive(false);
        }

    }
    void ExitMode()
    {
        _cursorUI.gameObject.SetActive(false);
        _markerUI.gameObject.SetActive(false);

        _active = false;
        _selected = null;

        _playerInput.SwitchCurrentActionMap("Player");
        foreach (InteractInterface interact in interactInterfaces)
        {
            interact.SetHighlighted(false);
        }
        interactInterfaces.Clear();

        UpdateCameraEffects();


    }
    void UpdateCameraEffects()
    {
        if (_camera == null) return;

        bool active = _active;
        foreach (PlayerInput player in PlayerInput.all)
        {
            CommandMode other = player.transform.root.GetComponentInChildren<CommandMode>();
            if (other != null && other._camera == _camera && other._active)
            {
                active = true;
                break;
            }
        }

        // A shared camera stays in command mode until both players leave it.
        UniversalAdditionalCameraData cameraData = _camera.GetUniversalAdditionalCameraData();
        int commandLayer = 1 << LayerMask.NameToLayer("CommandGray");
        if (active) cameraData.volumeLayerMask |= commandLayer;
        else cameraData.volumeLayerMask &= ~commandLayer;
        cameraData.SetRenderer(active ? 1 : -1);
    }
}
