using UnityEngine;

public class UIFull : MonoBehaviour
{
    [SerializeField] private FullableTriggerObject fullable;
    private RadialProgress3D radial;
    private LineRenderer line;
    [SerializeField] private GameObject back;

    void Start()
    {
        radial = GetComponent<RadialProgress3D>();   
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        bool end = fullable.currentValue == 0 || fullable.currentValue == fullable.maxValue;
        line.enabled = !end;
        back.SetActive(!end);
        if (end)        
            radial.SetProgress(0f);        
        else
            radial.SetProgress((float)fullable.currentValue / fullable.maxValue + 0.1f);
    }
}
