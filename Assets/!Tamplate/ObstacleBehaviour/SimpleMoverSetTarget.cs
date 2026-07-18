using UnityEngine;

public class SimpleMoverSetTarget : MonoBehaviour
{
    [SerializeField] private SimpleMover mover;
    [SerializeField] private Transform singleTarget;
    [SerializeField] private Transform[] multipleTargets;

    public void SetTarget(Transform target)
    {
        if (mover != null)
        {
            mover.chaseTarget = target;
        }
    }

    public void SetFirstFromMultiple()
    {
        if (multipleTargets != null && multipleTargets.Length > 0)
        {
            SetTarget(multipleTargets[0]);
        }
    }
}
