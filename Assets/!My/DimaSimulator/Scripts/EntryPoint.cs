using UnityEngine;

[DefaultExecutionOrder(-100)]
public static class EntryPoint
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitGame()
    {
        G.Init();

        InterfaceManager.Init();
        G.ResourceManager.Init(G.Data.PlayerResourcesInit.ToArray());

        Lenta mainLenta = G.InternetBank.CreateLenta();
        mainLenta.SetMaxSize(2);
        var lentaView = Object.FindFirstObjectByType<LentaView>();
        lentaView.Initialize(mainLenta);

        foreach (var item in G.Data.availableReactions)
        {
            G.PlayerInGame.AddReaction(item);
        }
        G.PlayerInGame.SubscribeToAuthor(G.Data.allAuthors[0], mainLenta);
        G.PlayerInGame.SubscribeToAuthor(G.Data.allAuthors[1], mainLenta);
        G.ArtistFlow.StartAllArtists();
    }
}

public static class G
{
    public static GameGlobalData Data;
    public static PlayerInGame PlayerInGame;
    public static InternetBank InternetBank;
    public static MissionManager MissionManager;
    public static MissionWindiowUI MissionWindiowUI;

    public static ArtistFlow ArtistFlow;
    public static MissionGameFlow MissionGameFlow;

    public static ResourceManager ResourceManager;

    public static void Init()
    {
        Data = Resources.Load<GameGlobalData>("GlobalData");

        InternetBank = new InternetBank();
        ResourceManager = new ResourceManager();
        MissionManager = new MissionManager();

        PlayerInGame = new GameObject("PlayerGameData").AddComponent<PlayerInGame>();
        ArtistFlow = new GameObject("ArtistFlow").AddComponent<ArtistFlow>();
        MissionGameFlow = new GameObject("MissionGameFlow").AddComponent<MissionGameFlow>();

        MissionWindiowUI = Object.FindFirstObjectByType<MissionWindiowUI>();
    }
}
