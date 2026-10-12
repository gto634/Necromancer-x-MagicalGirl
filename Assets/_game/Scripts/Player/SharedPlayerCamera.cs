using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class SharedPlayerCamera : MonoBehaviour
{
    [SerializeField] PlayerInputManager _playerInputManager;
    [SerializeField] Camera _outputCamera;
    [SerializeField] CinemachineCamera _cinemachineCamera;
    [SerializeField] CinemachineTargetGroup _targetGroup;
    [SerializeField, Min(0.1f)] float _targetRadius = 2f;

    void OnEnable()
    {
        if (_playerInputManager == null || _outputCamera == null
            || _cinemachineCamera == null || _targetGroup == null)
        {
            Debug.LogError("SharedPlayerCamera : references de camera manquantes.", this);
            enabled = false;
            return;
        }

        _playerInputManager.splitScreen = false;
        _outputCamera.rect = new Rect(0f, 0f, 1f, 1f);
        foreach (PlayerInput player in PlayerInput.all)
            OnPlayerJoined(player);
    }

    // PlayerInputManager uses Send Messages on the same GameObject.
    public void OnPlayerJoined(PlayerInput player)
    {
        if (!isActiveAndEnabled || player == null || player.gameObject.scene != gameObject.scene)
            return;

        Transform root = player.transform.root;
        foreach (CinemachineBrain brain in root.GetComponentsInChildren<CinemachineBrain>(true))
            brain.enabled = false;
        foreach (CinemachineCamera camera in root.GetComponentsInChildren<CinemachineCamera>(true))
            camera.enabled = false;
        foreach (Camera camera in root.GetComponentsInChildren<Camera>(true))
            camera.enabled = false;
        foreach (AudioListener listener in root.GetComponentsInChildren<AudioListener>(true))
            listener.enabled = false;

        player.camera = _outputCamera;
        foreach (LookSystem look in root.GetComponentsInChildren<LookSystem>(true))
            look.SetCamera(_outputCamera);
        foreach (CommandMode command in root.GetComponentsInChildren<CommandMode>(true))
            command.SetCamera(_outputCamera);

        if (_targetGroup.FindMember(root) < 0)
        {
            _targetGroup.AddMember(root, 1f, _targetRadius);
            _cinemachineCamera.PreviousStateIsValid = false;
        }
    }

    public void OnPlayerLeft(PlayerInput player)
    {
        if (_targetGroup == null || player == null) return;
        _targetGroup.RemoveMember(player.transform.root);
        _cinemachineCamera.PreviousStateIsValid = false;
    }
}
