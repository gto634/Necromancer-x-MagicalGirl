using System;
using System.Collections.Generic;
using UnityEngine;

public class LeverCondition : DoorCondition
{
    [SerializeField] List<LeverRequirement> _levers = new List<LeverRequirement>();

    public override bool IsMet
    {
        get
        {
            if (_levers.Count == 0) return false;

            foreach (LeverRequirement requirement in _levers)
            {
                if (requirement.lever == null) return false;

                bool wantedOn = !requirement.mustBeOff;
                if (requirement.lever.isOn != wantedOn) return false;
            }
            return true;
        }
    }
}