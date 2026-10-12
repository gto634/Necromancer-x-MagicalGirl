using System.Collections.Generic;
using UnityEngine;

public class AnyCondition : DoorCondition
{
    [SerializeField] List<DoorCondition> _conditions = new List<DoorCondition>();

    public override bool IsMet
    {
        get
        {
            foreach (DoorCondition condition in _conditions)
            {
                if (condition.IsMet) return true;
            }
            return false;
        }
    }
}