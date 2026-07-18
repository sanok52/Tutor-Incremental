using System;
using System.Collections.Generic;
using System.Linq;

public static class DialogGlobalData
{
    public static List<OutDialogSpikerData> spikerDatas = new List<OutDialogSpikerData>();

    public static OutDialogSpikerData GetSpiker (string ID)
    {
        return spikerDatas.FirstOrDefault(x => x.ID == ID);
    }

    public static void Clear()
    {
        spikerDatas.Clear();
    }
} 
