using System.Collections;
using UnityEngine;

public class OutSpikerView : MonoBehaviour, IOutSpikerView
{
    public virtual void Start()
    {
        FindFirstObjectByType<DialogPlayer>().AddSpiker(this);
    }

    public virtual bool IsPlay(OutDialogElement element)
    {
        return true;
    }

    public virtual IEnumerator Play(OutDialogElement element)
    {
        yield break;
    }

    private void OnDestroy()
    {
        FindFirstObjectByType<DialogPlayer>().RemoveSpiker(this);
    }

    public virtual void Break()
    {
        
    }
}
