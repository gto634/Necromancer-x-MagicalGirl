using UnityEngine;
using UnityEngine.Events;

public class DeathFeedback : MonoBehaviour
{
    [SerializeField] GameObject _FXConvertToMG;
    [SerializeField] GameObject _FXConvertToNEC;

    [SerializeField] ConvertSystem _convertSystem;

    [SerializeField] UnityEvent _onDeathFeedback = new UnityEvent();

    void Start()
    {
        _convertSystem.OnConvert += OnDeathFeedback;

    }

    public void OnDeathFeedback(GameObject _Killer)
    {
        _onDeathFeedback.Invoke();

        if (_Killer.CompareTag("NEC")){
            Instantiate(_FXConvertToNEC,transform.root.position, Quaternion.identity) ;
        }
        else if(_Killer.CompareTag("MG"))
        {
            Instantiate(_FXConvertToMG, transform.root.position, Quaternion.identity);
        }
    }
}
