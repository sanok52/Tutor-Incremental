using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class DimonGameData : MonoBehaviour
{
    public Sprite[] AnimeSprites;
    public SocialReactionData[] SocialReactions;
    public ArtistData[] Artists;

    [Space]
    public IntContainer[] playerResources;

    public static DimonGameData R;
}

public static class DimonGameFabric
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void Init()
    {
        DimonGameData.R = Resources.Load<DimonGameData>("GlobalData");
        InterfaceManager.Init();
    }

    public static Sprite GetSpriteAnime (string id)
    {
        return DimonGameData.R.AnimeSprites.FirstOrDefault(x => x.name == id);
    }

    public static SocialReactionData GetAnyReactoion(PostRectionType[] canEmoji, bool isNegative)
    {
        if(canEmoji == null || canEmoji.Length == 0)
            return DimonGameData.R.SocialReactions[0];

        SocialReactionData[] suitables = DimonGameData.R.SocialReactions.Where(x => canEmoji.Contains(x.Type) && (isNegative ? x.Likes <= 0 : x.Likes >= 0)).ToArray();
        return suitables.Length > 0 ? suitables.RandomElement() : DimonGameData.R.SocialReactions[0];
    }
}

[Serializable]
public struct SocialReactionData
{
    public PostRectionType Type;
    public Sprite Icon;
    public int Likes;
    public int Power;

    public bool IsNegative => Likes < 0;
}

[Serializable]
public struct ArtistData
{
    public string ID;
    public string Name;

    [Space, Header("Posting Settings")]
    public Sprite[] Arts;
    public SocialPost[] posts;
    public Vector2 PostInterval;

    public Sprite GetRandomArt()
    {
        return Arts.RandomElement();
    }
} 