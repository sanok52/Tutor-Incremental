using System;
using UnityEngine;

[Serializable]
public struct OutDialogSpikerData
{
    public string ID;
    public string Name;
    public Sprite Icon;

    [Space]
    public AudioDataPlay voiceType;
}
