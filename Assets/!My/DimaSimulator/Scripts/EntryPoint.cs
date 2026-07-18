using System;
using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class EntryPoint : MonoBehaviour
{
    void Awake()
    {
        G.Init();

        G.ResourceManager.Init(DimonGameData.R.playerResources);
        G.ResourceManager["Likes"].OnOverfullValue += LikesOverfull;
    }

    private int starsUp = 3;
    private void LikesOverfull(int over)
    {
        G.ResourceManager.AddResource("Stars", starsUp, null, null);
        starsUp *= 2;
        G.ResourceManager["Likes"].Init(G.ResourceManager["Likes"].Value + over, new Vector2Int(0, G.ResourceManager["Likes"].ClampRange.y * 2));
        InterfaceManager.BarMediator.SetMaxForID("Likes", G.ResourceManager["Likes"].ClampRange.y);
    }
}

public static class G
{
    public static ResourceManager ResourceManager;

    public static void Init()
    {
        ResourceManager = new ResourceManager();
    }
}
