using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class MopHItemBeh : HandsItemBeh, IHandsItemUsedHold, IHandsItemUseReciver
{
    public float duration = 1f;
    public float radius = 1f;
    public Texture2D mask;

    [Space]
    public float deltaMin = 0.2f;
    public float deltaCoef = 0.2f;

    [Space]
    public float waterMopMax = 5f;
    public float waterMopRemoveForce = 0.5f;

    [Space]
    public string[] instrumetTags = { "Mop" };

    private Vector3 prevPoint = Vector3.zero;

    public void ExecuteEnter(HandsItemBehContext context)
    {

    }

    public void ExecuteExit(HandsItemBehContext context)
    {
        MopAnimEnd(context);
    }

    public void ExecuteUpdate(HandsItemBehContext context)
    {

        if (Physics.Raycast(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out RaycastHit hitInfo, 2f))
        {
            TouchMop(context, hitInfo.point, hitInfo.transform.gameObject);
            prevPoint = hitInfo.point;
        }
        else
        {
            MopAnimEnd(context);
        }
    }

    private void MopAnimEnd(HandsItemBehContext context)
    {
        if (context.ItemAnimator == null)
            return;

        var mopAnim = context.ItemAnimator as MopAnimation;
        if (mopAnim == null)
            return;

        mopAnim.EndTween();
    }

    private void TouchMop(HandsItemBehContext context, Vector3 point, GameObject target)
    {
        string[] _instrumentTags = instrumetTags;

        var overDataInstr = context.ItemInstance.SourceWorldObject.overridebleForContextStr.FirstOrDefault(x => x.id == "InstrumentTags");
        if (overDataInstr != null)
            _instrumentTags = new string[] { overDataInstr.value };

        TryDirt(context, point, target, _instrumentTags);

        if (context.ItemAnimator == null)
            return;

        var mopAnim = context.ItemAnimator as MopAnimation;
        if (mopAnim == null)
            return;

        mopAnim.MopAnimInvoke(point);
    }

    private bool TryDirt(HandsItemBehContext context, Vector3 point, GameObject target, string[] _instrumentTags)
    {
        if (target.TryGetComponent(out Dirt dirt) &&
                    dirt.Instruments.Contains(_instrumentTags))
        {
            var overData = context.ItemInstance.SourceWorldObject.overridebleForContextFl.FirstOrDefault(x => x.id == "WaterMop");
            if (overData != null && overData.value <= 0f)
            {
                if (overData.value <= 0f)
                    context.ItemInstance.SourceWorldObject.behaviourAction?.Invoke("MopFalse", null);
                return false;
            }

            float d = (prevPoint - point).magnitude;
            if (d >= deltaMin)
            {
                dirt.Clean(point, radius, deltaCoef * d, mask, Camera.main.transform.rotation.eulerAngles.y);

                context.ItemInstance.SourceWorldObject.behaviourAction?.Invoke("MopTrue", null);
                if (overData != null)
                    overData.value -= d * waterMopRemoveForce;
            }
        }

        return true;
    }

    public void ExecureReciver(HandsItemBehContext context)
    {
        if (context.Receiver.includedTags.Contains("WaterMop"))
        {
            var overData = context.ItemInstance.SourceWorldObject.overridebleForContextFl.FirstOrDefault(x => x.id == "WaterMop");
            if (overData == null)
                return;

            overData.value = waterMopMax;
        }
    }
}
