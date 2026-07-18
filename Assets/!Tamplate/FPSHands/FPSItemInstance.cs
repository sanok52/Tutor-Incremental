using System;
using UnityEngine;

public class FPSItemInstance
{
    public FPSItemData Data { get; private set; }
    public FPSHandsItemObject SourceWorldObject { get; private set; }

    public FPSItemInstance(FPSItemData data, FPSHandsItemObject source = null)
    {
        Data = data;
        SourceWorldObject = source;
    }

    public FPSItemInstance()
    {
    }

    public string[] GetTags()
    {
        return SourceWorldObject != null ? SourceWorldObject.GetTags() : Data.tags;
    }
}