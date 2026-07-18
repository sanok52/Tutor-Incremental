using System;
using System.Collections;
using UnityEngine;

public class DialogUI : OutSpikerView
{
    [SerializeField] private TextTyper textTyper;
    [SerializeField] private float waitToReed = 1.5f;

    public override IEnumerator Play(OutDialogElement element)
    {
        yield return new WaitForEndOfFrame();
        yield return textTyper.ClearAndTypeText(GetSpikerName(element.IDSpiker) + ": " + element.Text);
        yield return new WaitForSeconds(waitToReed);
        textTyper.ClearText();
    }

    private string GetSpikerName(string iDSpiker)
    {
        return $"{iDSpiker}";
    }

    public override void Break()
    {
        textTyper.ClearImmidiatly();
    }
}
