using UnityEngine;

public class TriggerHandle : MonoBehaviour
{
    [SerializeField] private HandsReceiver Receiver;
    [SerializeField] private FPSHandsModel model;

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out FPSHandsItemObject itemObject))
            Receiver.TryInteract(itemObject.GetInstance(model), model);
    }
}