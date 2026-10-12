using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageZone : MonoBehaviour
{
    [SerializeField, Min(0)] int _damagePerTick = 10;
    [SerializeField, Min(0.05f)] float _tickInterval = 0.5f;
    [Tooltip("Tags des colliders à blesser (ex : NEC, MG, ou les tags des alliés).")]
    [SerializeField] List<string> _damageableTags = new List<string> { "NEC", "MG" };

    readonly HashSet<LifeSystem> _inside = new HashSet<LifeSystem>();
    float _timer;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        LifeSystem life = GetTarget(other);
        if (life != null) _inside.Add(life);
    }

    void OnTriggerExit(Collider other)
    {
        LifeSystem life = GetTarget(other);
        if (life != null) _inside.Remove(life);
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = _tickInterval;

        _inside.RemoveWhere(life => life == null);
        foreach (LifeSystem life in _inside) life.TakeDamage(_damagePerTick);
    }

    LifeSystem GetTarget(Collider other)
    {
        if (!_damageableTags.Contains(other.tag)) return null;
        return other.transform.root.GetComponentInChildren<LifeSystem>();
    }
}