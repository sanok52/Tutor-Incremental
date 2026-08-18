using System.Collections.Generic;
using UnityEngine;

public class ReactionLine : MonoBehaviour
{
    [SerializeField] private GameObject reactionButtonPrefab;
    [SerializeField] private Transform buttonsContainer;

    private Post currentPost;
    private List<ReactionButton> buttons = new List<ReactionButton>();

    public void Initialize(Post post)
    {
        currentPost = post;
        foreach (var btn in buttons)
            Destroy(btn.gameObject);
        buttons.Clear();

        if (G.PlayerInGame != null)
        {
            foreach (var setting in G.PlayerInGame.reactionSettings)
            {
                GameObject go = Instantiate(reactionButtonPrefab, buttonsContainer);
                ReactionButton btn = go.GetComponent<ReactionButton>();
                btn.Initialize(setting.type, post);
                buttons.Add(btn);
            }
        }
    }
}