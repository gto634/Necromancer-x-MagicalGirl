using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConnectPlayer : MonoBehaviour
{
    [SerializeField] PlayerInputManager _playerInputManager;

    [SerializeField] GameObject _necromancerPrefab;
    [SerializeField] GameObject _magicalGirlPrefab;

    [SerializeField] Transform _SpawnNEC;
    [SerializeField] Transform _SpawnMG;

    static InputDevice[] s_devices;
    static List<int> s_characters;

    public static void SetPlayers(InputDevice[] devices, List<int> characters)
    {
        s_devices = devices;
        s_characters = characters;
    }

    void Start()
    {
        if (s_devices == null)
        {
            if (_playerInputManager != null && _playerInputManager.joiningEnabled
                && _playerInputManager.joinBehavior != PlayerJoinBehavior.JoinPlayersManually)
            {
                return;
            }

            return;
        }

        JoinPlayers();
    }

    void JoinPlayers()
    {
        GameManager gameManager = GameManager.Instance;

        for (int i = 0; i < s_devices.Length; i++)
        {
            bool isNecromancer = s_characters[i] == -1;

            _playerInputManager.playerPrefab = GetPlayerPrefab(isNecromancer, gameManager);

            PlayerInput player = s_devices[i] is Keyboard
                ? _playerInputManager.JoinPlayer(i, -1, "Keyboard&Mouse", s_devices[i], Mouse.current)
                : _playerInputManager.JoinPlayer(i, -1, "Gamepad", s_devices[i]);

            if (player == null)
            {
                continue;
            }

            Transform root = player.transform.root;
            root.position = isNecromancer ? _SpawnNEC.position : _SpawnMG.position;
            PlayerCommander commander = root.GetComponentInChildren<PlayerCommander>();
            if (commander != null) commander.ResetFollowTrail();

            if (gameManager != null)
            {
                gameManager.RestoreAllies(root.gameObject, isNecromancer);
            }
        }

        if (gameManager != null)
        {
            gameManager.ClearNextPlayerPrefabs();
        }
    }

    GameObject GetPlayerPrefab(bool isNecromancer, GameManager gameManager)
    {
        GameObject nextPrefab = null;
        if (gameManager != null)
        {
            nextPrefab = isNecromancer ? gameManager.NextNecromancianPrefab : gameManager.NextMagicalGirlPrefab;
        }

        if (nextPrefab != null) return nextPrefab;
        return isNecromancer ? _necromancerPrefab : _magicalGirlPrefab;
    }
}
