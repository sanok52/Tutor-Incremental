using UnityEngine;

public class WaterEffect : MonoBehaviour
{
    [SerializeField] private FPSHandsItemObject itemObject;

    void Start()
    {
    }

    void Update()
    {
        transform.GetChild(0).gameObject.SetActive(itemObject.overridebleForContextFl.Find(x => x.id == "WaterMop").value > 0);        
    }
}
