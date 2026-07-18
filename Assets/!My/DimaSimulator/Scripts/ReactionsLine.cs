using System;
using System.Collections.Generic;
using UnityEngine;

public class ReactionsLine : MonoBehaviour
{
    [SerializeField] private ReactionPostView ReactionPostViewPrefab;
    [SerializeField] private Transform reactionsContainer;

    private Dictionary<PostRectionType, ReactionPostView> views = new Dictionary<PostRectionType, ReactionPostView>();

    public void PostUpdate(SocialReactionInPost[] reactionInPosts)
    {
        foreach (var socialReaction in reactionInPosts)
        {
            if (views.TryGetValue(socialReaction.Reaction.Type, out ReactionPostView reactionPostView))
            {
                reactionPostView.SetCount(socialReaction.Count);
            }
            else
            {
                var newReactionView = Instantiate(ReactionPostViewPrefab, reactionsContainer);
                newReactionView.Init(socialReaction);
                views.Add(socialReaction.Reaction.Type, newReactionView);
            }
        }
    }

    public Transform GetReactionTransform(PostRectionType type)
    {
        if (views.TryGetValue(type, out ReactionPostView reactionPostView))        
            return reactionPostView.transform;
        return null;
    }
}