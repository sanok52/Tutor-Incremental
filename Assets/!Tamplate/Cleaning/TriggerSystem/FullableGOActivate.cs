using UnityEngine;

public class FullableGOActivate : MonoBehaviour
{
    [SerializeField] private FullableTriggerObject fullableTriggerObject;
    [SerializeField] private GameObject[] ActivateGO;

    private void Start()
    {
        fullableTriggerObject.OnOverfullValue += Overfull;
        fullableTriggerObject.OnDownfullValue += Downfull;
    }

    private void Overfull()
    {
        foreach (var item in ActivateGO)
        {
            item.SetActive(true);
        }
    }

    private void Downfull()
    {
        foreach (var item in ActivateGO)
        {
            item.SetActive(false);
        }
    }
}