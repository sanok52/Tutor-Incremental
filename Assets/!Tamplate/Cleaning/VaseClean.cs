using System;
using UnityEngine;

public class VaseClean : MonoBehaviour
{
    [SerializeField] private HandsReceiver[] receivers;
    private int indexPlace;

    private bool isFull;
    public bool IsFull => isFull;

    public event Action OnFull; 

    void Start()
    {
        foreach (var receiver in receivers)
        {
            receiver.OnInteraction += NextPlace;
            receiver.gameObject.SetActive(false);
        }
        receivers[0].gameObject.SetActive(true);
    }

    private void NextPlace(HandInteractionInfo info)
    {
        receivers[indexPlace].enabled = false;
        receivers[indexPlace].GetComponent<Collider>().enabled = false;

        indexPlace++;
        if (indexPlace >= receivers.Length)
        {
            isFull = true;
            OnFull?.Invoke();
            return;
        }

        receivers[indexPlace].gameObject.SetActive(true);
    }
}
