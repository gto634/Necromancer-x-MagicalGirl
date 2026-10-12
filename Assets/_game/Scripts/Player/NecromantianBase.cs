using UnityEngine;
using UnityEngine.InputSystem;

public class NecromantianBase : MonoBehaviour
{

    void Start()
    {
        GameManager.Instance.Necromancian = transform.root.gameObject;
        GameManager.Instance.necromancianInput = GetComponent<PlayerInput>();
    }
}
