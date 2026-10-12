using System.Collections.Generic;
using UnityEngine;

public class EnemyKillCondition : DoorCondition
{
    [SerializeField] List<GameObject> _enemies = new List<GameObject>();

    [SerializeField, Min(0)] int _requiredKills = 0;

    readonly List<LifeSystem> _lives = new List<LifeSystem>();
    int _required;

    public int RequiredKills => _required;

    void Awake()
    {
        foreach (GameObject enemy in _enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            LifeSystem life = enemy.GetComponentInChildren<LifeSystem>();
            if (life == null)
            {
                continue;
            }
            _lives.Add(life);
        }

        _required = _requiredKills <= 0 ? _lives.Count : Mathf.Min(_requiredKills, _lives.Count);
    }

    public int DeadCount
    {
        get
        {
            int count = 0;
            foreach (LifeSystem life in _lives)
            {
                if (life == null || life.IsDead) count++;
            }
            return count;
        }
    }

    public override bool IsMet => _lives.Count > 0 && DeadCount >= _required;
}