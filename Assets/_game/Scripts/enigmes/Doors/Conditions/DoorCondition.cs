using UnityEngine;

public class DoorCondition : MonoBehaviour
{
    public virtual bool IsMet
    {
        get { return false; }
    }
}