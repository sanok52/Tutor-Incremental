using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReactionPostView : MonoBehaviour
{
    [SerializeField] private Image spriteRenderer;
    [SerializeField] private TMP_Text countText;

    public SocialReactionInPost currentReaction { get; private set; }

    public void Init(SocialReactionInPost reactionInPost)
    {
        spriteRenderer.sprite = reactionInPost.Reaction.Icon;
        SetCount(reactionInPost.Count);
    }

    public void SetCount(int count)
    {
        countText.text = count.ToString();
        if (count <= 1)
        {
            countText.text = "";
        }
    }
}