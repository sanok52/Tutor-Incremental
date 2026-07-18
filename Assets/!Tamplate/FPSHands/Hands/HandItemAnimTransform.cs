using UnityEngine;

public class HandItemAnimTransform : MonoBehaviour, IHItemAnimator
{
    [SerializeField] private Transform root;

    [Space]
    public bool IsAnimUse = true;
    [SerializeField] private Vector3 useHoldPos;
    [SerializeField] private Vector3 useHoldRot;

    [Space]
    public bool IsAnimGrapDrop = true;
    [SerializeField] private Vector3 grapPos;
    [SerializeField] private Vector3 grapRot;

    private Vector3 positionDefault;
    private Vector3 rotationDefault;

    private bool isUse;

    public virtual void Start()
    {
        GetComponent<FPSHandsItemObject>().OnGrap += (info) => Grap();
        GetComponent<FPSHandsItemObject>().OnDrop += (item) => Drop();

        positionDefault = root.localPosition;
        rotationDefault = root.localRotation.eulerAngles;

        Drop();
    }

    public virtual void Grap()
    {
        if (!IsAnimGrapDrop)
            return;

        root.localPosition = grapPos;
        root.localRotation = Quaternion.Euler(grapRot);
    }

    public virtual void Drop()
    {
        if (!IsAnimGrapDrop)
            return;


        root.localPosition = positionDefault;
        root.localRotation = Quaternion.Euler(rotationDefault);
    }

    public virtual void StartUse()
    {
        if (!IsAnimUse)
            return;

        root.localPosition = useHoldPos;
        isUse = true;
    }

    public virtual void EndUse()
    {
        if (!IsAnimUse)
            return;

        root.localPosition = grapPos;
        root.localRotation = Quaternion.Euler(grapRot);
        isUse = false;
    }

    void IHItemAnimator.SwithUse()
    {
        if (isUse)
            EndUse();
        else
            StartUse();
    }
}
