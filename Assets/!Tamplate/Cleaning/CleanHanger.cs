using System;
using System.Collections.Generic;
using UnityEngine;

public class CleanHanger : MonoBehaviour
{
    [SerializeField] private List<HandsReceiver> handsReceivers = new List<HandsReceiver>();
    [SerializeField] private float hotWait = 10f;

    private List<FPSHandsItemObject> fPSHandsItemObjects = new List<FPSHandsItemObject>();
    private bool canHot = false;

    public event Action OnHot;
    [SerializeField] private GameObject radialGO;
    [SerializeField] private RadialProgress3D radialProgress;

    private void Start()
    {
        foreach (var item in handsReceivers)
        {
            item.OnInteraction += ReceiverInteraction;
        }
        radialGO.SetActive(false);
    }

    private void ReceiverInteraction(HandInteractionInfo info)
    {
        fPSHandsItemObjects.Add(info.item.SourceWorldObject);
        info.receiver.enabled = false;
        info.receiver.GetComponent<Collider>().enabled = false;

        if (fPSHandsItemObjects.Count == handsReceivers.Count)
        {
            canHot = true;       

        }
    }

    private void Update()
    {
        
    }

    private float timerHot;
    public void Hot()
    {
        if (!canHot)
            return;

        radialGO.SetActive(true);

        timerHot += Time.fixedDeltaTime;
        radialProgress.SetProgress(timerHot / hotWait);

        if (timerHot >= hotWait)
        {
            foreach (var item in fPSHandsItemObjects)
            {
                item.overridebleForContextFl.Find(x => x.id == "Washing").value = 2;
                item.OverTags.Add("ClothWashing2");
                item.GetComponentInChildren<Collider>().enabled = true;
                item.GetComponentInChildren<Collider>().isTrigger = true;
                //item.GetComponentInChildren<Rigidbody>().useGravity = false;
            }
            radialGO.SetActive(false);
            canHot = false;
            OnHot?.Invoke();
        }
    }
}
