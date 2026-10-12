using UnityEngine;
public class EventCondition : DoorCondition
{
    [SerializeField, ReadOnly] bool _isMet;

    public override bool IsMet => _isMet;

    public void Validate() { _isMet = true; }
    public void Invalidate() { _isMet = false; }
}