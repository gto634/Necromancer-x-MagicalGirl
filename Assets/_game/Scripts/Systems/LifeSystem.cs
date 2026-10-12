using System;
using UnityEngine;

public class LifeSystem : MonoBehaviour
{
    [SerializeField, Min(0)] int _maxHealth = 100;
    [SerializeField, ReadOnly] int _health = 100;

    public int Health
    {
        get => _health;
        private set
        {
            if (value > _maxHealth)
            {
                value = _maxHealth;
            }
            if (value < 0)
            {
                value = 0;
            }
            _health = value;

            if (_health == 0)
            {
                OnDeath?.Invoke();
            }
        }
    }



    public int MaxHealth => _maxHealth;
    public bool IsDead => _health == 0;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public bool DebugInvincible { get; set; }

    public void DebugSetHealth(int health)
    {
        int newHealth = Mathf.Clamp(health, 0, _maxHealth);
        if (newHealth == Health) return;
        if (newHealth > Health) OnHeal?.Invoke(Health, newHealth);
        else OnDamage?.Invoke(Health, newHealth);
        Health = newHealth;
    }
#endif

    public Action<int, int> OnHeal;
    public Action<int, int> OnDamage;
    public Action OnDeath;

    private void Reset()
    {
        _maxHealth = 100;
        _health = _maxHealth;
    }

    private void Start()
    {
        _health = _maxHealth;
    }

    public void Heal(int amount) {
        if (IsDead) return;

        int newHealth = Mathf.Min(Health + amount, _maxHealth);

        OnHeal?.Invoke(Health, newHealth);

        Health = newHealth;
    }

    public void TakeDamage(int amount) {
        if (IsDead) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (DebugInvincible) return;
#endif

        Debug.Log($"{gameObject.name} took {amount} damage.");

        int newHealth = Mathf.Max(Health - amount, 0);

        OnDamage?.Invoke(Health, newHealth);

        Health = newHealth;

        Debug.Log($"{gameObject.name} health is now {Health}.");
    }
    public void Revive()
    {
        OnHeal?.Invoke(_health, _maxHealth);
        Health = _maxHealth;
    }
}
