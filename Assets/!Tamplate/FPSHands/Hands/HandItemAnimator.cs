using UnityEngine;

public class HandItemAnimator : MonoBehaviour, IHItemAnimator
{
    [SerializeField] private Animator animator;

    [Space]
    public bool IsAnimUse = true;
    [SerializeField] private string blIsUseHold;
    [SerializeField] private string tgUseEnd;

    [Space]
    public bool IsAnimGrapDrop = true;
    [SerializeField] private string tgGrap;
    [SerializeField] private string tgDrop;

    public virtual void Start()
    {
        GetComponent<FPSHandsItemObject>().OnGrap += (info) => Grap();
        GetComponent<FPSHandsItemObject>().OnDrop += (item) => Drop();

        Drop();
    }

    public virtual void Grap()
    {
        if (!IsAnimGrapDrop)
            return;

        animator.SetTrigger(tgGrap);
    }

    public virtual void Drop()
    {
        if (!IsAnimGrapDrop)
            return;

        animator.SetTrigger(tgDrop);
    }

    public virtual void StartUse()
    {
        animator.SetBool(blIsUseHold, true);
    }

    public virtual void EndUse()
    {
        animator.SetBool(blIsUseHold, false);
    }

    public void InvokeTrigger(string trigger)
    {
        animator.SetTrigger(trigger);
    }
}

public interface IHItemAnimator
{
    public void Grap();
    public void Drop();
    public void StartUse();
    public void EndUse();
    public virtual void SwithUse() { }
}