using UnityEngine;
using UnityEngine.InputSystem;

public class MagicalGirlBase : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.MagicalGirl = transform.root.gameObject;
        GameManager.Instance.magicalGirlInput = GetComponent<PlayerInput>();
    }
}
