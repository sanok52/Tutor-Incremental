using System;
using UnityEngine;

public class ReceiverBreackerFullable : MonoBehaviour
{
    [SerializeField] private HandsReceiver handsReceiver;
    [SerializeField] private FullableTriggerObject fullable;

    [Space]
    [SerializeField] private int substructMax;
    private int currentCount;

    private void Start()
    {
        handsReceiver.OnInteraction += Substruct;
    }

    private void Substruct(HandInteractionInfo info)
    {
        if (fullable.currentValue == 0 || currentCount >= substructMax)
            return;

        currentCount++;

        if (currentCount >= substructMax)
        {
            fullable.Break();
            currentCount = 0;
        }
    }
}
