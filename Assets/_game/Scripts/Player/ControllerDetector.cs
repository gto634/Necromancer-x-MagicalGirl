using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;  // PlayStation
using UnityEngine.InputSystem.XInput;       // Xbox

public class ControllerDetector : MonoBehaviour
{
    int _nombreManette = 0;
    InputDevice _Keyboard;
    List<InputDevice> _ManettesList = new List<InputDevice>();
    public event System.Action OnManettesChanged;
    public InputDevice Keyboard
    {
        get { return _Keyboard; }
    }
    public List<InputDevice> ManettesList
    {
        get { return _ManettesList; }
    }


    public int nombreManette
    {
        get {return _nombreManette;}
        private set { _nombreManette = value; }
    }

    void Awake()
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            DetectController(device , InputDeviceChange.Added);
        }

        // Détection quand un périphérique est branché ou retiré
        InputSystem.onDeviceChange += (device, change) =>
        {
            if (change == InputDeviceChange.Added)
                DetectController(device, change);
            if (change == InputDeviceChange.Removed)
            {
                DetectController(device, change);
            }

        };
    }

    void DetectController(InputDevice device, InputDeviceChange change)
    {
        if (device is XInputControllerWindows xbox)
        {
            Debug.Log($"Xbox Controller détecté : {xbox.displayName}");
            AddRemoveController(device, change);

        }
        else if (device is DualShockGamepad dualShock)
        {
            Debug.Log($"PlayStation Controller détecté : {dualShock.displayName}");
            AddRemoveController(device, change);

        }
        else if (device is Gamepad gamepad)
        {
            Debug.Log($"Autre Gamepad détecté : {gamepad.displayName} (Fabricant: {gamepad.displayName})");
            AddRemoveController(device, change);
        }
        else if (device is Keyboard keyboard)
        {
            Debug.Log($"Autre périphérique : {device.displayName} ({device.GetType().Name})");
            _Keyboard = device;
        }
        Debug.Log($"nombre manette : {nombreManette}");

    }

    void AddRemoveController(InputDevice _device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            nombreManette++;
            _ManettesList.Add(_device);
        }
        else
        {
            nombreManette--;
            _ManettesList.Remove(_device);

        }
        OnManettesChanged?.Invoke();
    }
}
