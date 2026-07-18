using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SocialPost
{
    public string MainImageID;
    public int Likes;
    public int Dislikes;
    public float virality = 1f;
    public PostRectionType[] CanEmoji;

    public List<SocialReactionInPost> Reactions = new List<SocialReactionInPost>();
    public event Action<SocialPost> OnPostChange;
    public event Action<SocialPost, SocialReactionData, int> OnPostLike;

    [Space]
    public Sprite MainSpriteDefault;

    public Sprite GetMainImageSprite()
    {
        return DimonGameFabric.GetSpriteAnime(string.IsNullOrEmpty(MainImageID) ? MainSpriteDefault.name : MainImageID);
    }

    public SocialPost CloneForPublic()
    {
        return new SocialPost
        {
            MainImageID = string.IsNullOrEmpty(MainImageID) ? MainSpriteDefault.name : MainImageID,
            Likes = 0,
            Dislikes = 0,
            CanEmoji = this.CanEmoji,
            virality = virality,
            MainSpriteDefault = MainSpriteDefault
        };
    }

    public SocialPost CloneForLenta()
    {
        return new SocialPost
        {
            MainImageID = string.IsNullOrEmpty(MainImageID) ? MainSpriteDefault.name : MainImageID,
            CanEmoji = this.CanEmoji,
            virality = virality,
            MainSpriteDefault = MainSpriteDefault
        };
    }

    public void AddRandomReactions(bool isNegative, int count)
    {
        SocialReactionData socialReaction = DimonGameFabric.GetAnyReactoion(CanEmoji, isNegative);
        AddReaction(socialReaction, count);
    }

    private void AddReaction(SocialReactionData socialReaction, int count)
    {
        if(Reactions.Any(x => x.Reaction.Type == socialReaction.Type))
        {
            var reactionInPost = Reactions.First(x => x.Reaction.Type == socialReaction.Type);
            reactionInPost.Count += count;
        }
        else
        {
            Reactions.Add(new SocialReactionInPost
            {
                Reaction = socialReaction,
                Count = count
            });
        }

        OnPostChange?.Invoke(this);
        OnPostLike?.Invoke(this, socialReaction, count);
    }
}

[Serializable]
public class SocialReactionInPost
{
    public SocialReactionData Reaction;
    public int Count;
}
