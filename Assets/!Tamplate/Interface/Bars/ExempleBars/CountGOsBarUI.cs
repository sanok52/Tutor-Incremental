using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using DG.Tweening;

public class CountGOsBarUI : BarUI
{
    [Space]
    [SerializeField] private GameObject prefab;
    [SerializeField] private float offset = 0.5f;
    private List<GameObject> listObs = new List<GameObject>();

    [Space]
    [SerializeField] private bool useMaxValue;
    [SerializeField] private GameObject prefabMaxValue;
    private List<GameObject> listMaxValueObs = new List<GameObject>();

    [Space]
    [SerializeField] private string addUIAnim;
    [SerializeField] private string removeUIAnim;

    public override void ShowMaxValueInInterface(float maxValue)
    {
        if (useMaxValue == false)
        {
            foreach (var ob in listMaxValueObs)
            {
                ob.SetActive(false);
            }
            return;
        }

        int count = (int)(maxValue - CurrentValue);

        float dealy = 0.5f;
        if (count < listMaxValueObs.FindAll((go) => go.activeInHierarchy).Count)        
            dealy = Time.deltaTime;        

        DOVirtual.DelayedCall(dealy, () =>
        {


            for (int i = 0; i < listMaxValueObs.Count; i++)
            {
                listMaxValueObs[i].SetActive(i < count);
                listMaxValueObs[i].transform.localPosition += ((listObs.Count > 0 && listObs.Count < (int)CurrentValue) ?
                    listObs[(int)CurrentValue].transform.localPosition : Vector3.zero) + (Vector3.right * offset * i);
            }
            count -= listMaxValueObs.Count;
            while (count > 0)
            {
                CreateMaxOb();
                count--;
            }
        });
    }
   

    public override void ShowValueInInterface(float currentValue)
    {
        int count = (int)currentValue;
        for (int i = 0; i < listObs.Count; i++)
        {
            bool active = i < count;

            if (active == listObs[i].activeInHierarchy)
                continue;

            if (active && listObs[i].activeInHierarchy == false)
                listObs[i].gameObject.SetActive(true);

            if (((active && addUIAnim != "") || (!active && removeUIAnim != "")) && 
                listObs[i].TryGetComponent(out UIPunchFX uIPunchFX))
            {
                int n = i;
                uIPunchFX.InvokeInstructions(active ? addUIAnim : removeUIAnim);
                DOVirtual.DelayedCall(0.5f, () => listObs[n].SetActive(active)).SetUpdate(false);
            }
            else
            {
                listObs[i].SetActive(active);
            }
        }
        count -= listObs.Count;
        while (count > 0)
        {
            CreateOb();
            count--;
        }
    }

    private void CreateOb()
    {
        if (prefab == null)
            return;

        GameObject ngo = Instantiate(prefab, transform);
        ngo.transform.localPosition += ((listObs.Count > 0 && listObs.Count > CurrentValue) ?
            listObs[(int)CurrentValue - 1].transform.localPosition : Vector3.zero) + (Vector3.right * offset);
        listObs.Add(ngo);
    }

    private void CreateMaxOb()
    {
        GameObject ngo = Instantiate(prefabMaxValue, transform);
        ngo.transform.localPosition += ((listObs.Count > 0 && listObs.Count > CurrentValue) ?
            listObs[(int)CurrentValue - 1].transform.localPosition : Vector3.zero) + (Vector3.right * offset);
        listMaxValueObs.Add(ngo);
    }
}