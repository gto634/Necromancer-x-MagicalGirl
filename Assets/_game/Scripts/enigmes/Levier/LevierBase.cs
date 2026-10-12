using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LevierBase : InteractInterface
{
    [SerializeField] Animator _animator;
    
    bool _isOn = false;
    public bool isOn {  get { return _isOn; } private set { _isOn = value; } }
    [SerializeField] UnityEvent _isOnEvent = new UnityEvent();
    [SerializeField] UnityEvent _isOffEvent = new UnityEvent();

    override public void OnInteract (InputAction.CallbackContext context) 
    {
        Debug.Log("E");
        Toggle();
        Debug.Log("_isOn = " + _isOn);

    }
    override public void OnPNJInteract()
    {
        Toggle();

    }

    void Toggle()
    {
        isOn = !isOn;

        if (_isOn)
        {
            _isOnEvent?.Invoke();
        }
        else
        {
            _isOffEvent?.Invoke();
        }

            _animator.SetBool("isOn", isOn);
        _animator.SetTrigger("wasClicked");

    }


    void Start()
    {
        
    }

    void Update()
    {

        
    }
   

}
