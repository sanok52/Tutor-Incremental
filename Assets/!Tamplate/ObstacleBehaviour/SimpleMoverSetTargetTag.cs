using UnityEngine;

public class SimpleMoverSetTargetTag : MonoBehaviour
{
    [SerializeField] private SimpleMover mover;
    [SerializeField] private string tagTarget;

    void Start()
    {
        GameObject found = GameObject.FindWithTag(tagTarget);
        if (found != null && mover != null)
        {
            mover.chaseTarget = found.transform;
        }
    }
}