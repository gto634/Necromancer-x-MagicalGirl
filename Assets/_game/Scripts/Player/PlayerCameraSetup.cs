using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerCameraSetup : MonoBehaviour
{
    [SerializeField] CinemachineBrain _brain;     
    [SerializeField] CinemachineCamera _cinemachineCamera;
    [SerializeField] PlayerInput _playerInput;

    void Awake()
    {
        int playerIndex = _playerInput.playerIndex;
        OutputChannels myChannel = GetChannelForPlayer(playerIndex);

        _cinemachineCamera.OutputChannel = myChannel;
        _brain.ChannelMask = myChannel;
    }

    OutputChannels GetChannelForPlayer(int playerIndex)
    {
        OutputChannels[] channels =
        {
            OutputChannels.Channel01,
            OutputChannels.Channel02
        };
        return channels[playerIndex];
    }
}