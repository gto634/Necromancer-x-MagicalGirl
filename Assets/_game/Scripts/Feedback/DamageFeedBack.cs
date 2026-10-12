using UnityEngine;
using UnityEngine.Events;

public class DamageFeedBack : MonoBehaviour
{
    [SerializeField] LifeSystem _lifeSystem;
    [SerializeField] KnockbackSystem _knockbackSystem;

    [SerializeField] UnityEvent _onDamageFeedback = new UnityEvent();
    [SerializeField] UnityEvent _onKnockbackFeedback = new UnityEvent();

    bool isKnockbacked = false;
    bool isDamaged = false;

    void Start()
    {
        _lifeSystem.OnDamage += OnDamageFeedback;
        _knockbackSystem.OnKnockback += OnKnockbackFeedback;
    }

    private void Update()
    {
        if (isDamaged)
        { 
            _onDamageFeedback.Invoke();
        }
        else if (isKnockbacked)
        {
            _onKnockbackFeedback.Invoke();
        }

        isDamaged = false;
        isKnockbacked = false;
    }

    void OnDamageFeedback(int PreviousHealth, int NewHealth)
    {
        isDamaged = true;
    }
    void OnKnockbackFeedback(int Amount, Vector2 Direction)
    {
        isKnockbacked = true;
    }
}
