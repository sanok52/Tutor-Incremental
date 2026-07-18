using UnityEngine;

public class HotTrigger : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if(other.TryGetComponent(out CleanHanger cleanHanger))
        {
            cleanHanger.Hot();
        }
    }
}
