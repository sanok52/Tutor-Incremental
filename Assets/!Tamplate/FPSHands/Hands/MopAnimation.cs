using DG.Tweening;
using System;
using UnityEngine;

public class MopAnimation : HandItemAnimator, IHItemAnimator
{
    [Space]
    [SerializeField] private Transform root;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform rootLocal;
    [SerializeField] private Vector3 offsetLocal;
    [SerializeField] private DOTweenPositionData animData;
    [SerializeField] private float animationScale = 0.1f;

    [Space]
    [SerializeField] private bool isPositionInPoint;

    Quaternion startRot;

    public override void Start()
    {
        base.Start();
        startRot = root.localRotation;
    }

    public override void StartUse()
    {
        base.StartUse();
        root.transform.position = transform.position + offset + rootLocal.TransformVector(offsetLocal);

        if (Physics.Raycast(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out RaycastHit hitInfo, 3f))
            root.LookAt(hitInfo.point + offset);
    }

    public void MopAnimInvoke(Vector3 point)
    {
        Vector3 randomV3 = (Vector3)(UnityEngine.Random.insideUnitCircle * animationScale);
        randomV3 = new Vector3(randomV3.x, 0, randomV3.y);
        Vector3 rootPos = isPositionInPoint ? point : transform.position;
        animData.TransformMove(root, rootPos + offset + rootLocal.TransformVector(offsetLocal) + randomV3);
        root.LookAt(point + offset);
    }

    public override void Drop()
    {
        base.Drop();
        EndTween();
    }    

    public void EndTween()
    {
        animData.lastTweener.Kill(false);
        root.localRotation = startRot;
        root.localPosition = Vector3.zero;
    }

    void IHItemAnimator.SwithUse()
    {
        
    }
}