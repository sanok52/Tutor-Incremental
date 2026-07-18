using System;
using System.Collections.Generic;
using UnityEngine;

public class SocialUIManager : MonoBehaviour
{
    private List<SocialPublic> publics = new List<SocialPublic>();
    private Dictionary<string, ISocialPublicView> publicViews = new Dictionary<string, ISocialPublicView>();

    public event Action<SocialPublic> OnPublicChange;
    public event Action<SocialPublic, SocialPost> OnPublicPostChange;
    public event Action<SocialPublic, SocialPost> OnPublicPostAdd;
    public event Action<SocialPublic, SocialPost, SocialReactionData, int> OnPostLike;

    public static SocialUIManager Instance { get; private set; }
    public List<SocialPublic> Publics => publics;
    public Dictionary<string, ISocialPublicView> PublicViews => publicViews;

    private void Awake()
    {
        Instance = this;

        AddPublic(new SocialPublic() { ID = "Test" });
        publicViews.Add("Test", FindFirstObjectByType<MessangerWindow>().CurrentPublicViewGO.GetComponent<ISocialPublicView>());
    }

    public void AddPublic(SocialPublic socialPublic)
    {
        Publics.Add(socialPublic);

        socialPublic.OnChange += () => OnPublicChange?.Invoke(socialPublic);
        socialPublic.OnPostChange += (post) => OnPublicPostChange?.Invoke(socialPublic, post);
        socialPublic.OnAddPost += (post) => OnPublicPostAdd?.Invoke(socialPublic, post);
        socialPublic.OnPostLike += (post, reaction, count) => OnPostLike?.Invoke(socialPublic, post, reaction, count);
    }

    public void RepostPostInPublic(string id, SocialPost socialPost, bool clone = true)
    {
        SocialPublic socialPublic = Publics.Find(p => p.ID == id);
        socialPublic.AddPost(socialPost.CloneForPublic());
    }
}

[Serializable]
public class SocialPublic
{
    public string ID;
    public List<SocialPost> posts = new List<SocialPost>();
    public int Overviews = 3;
    public int Subscrubers = 3;

    public event Action OnChange;
    public event Action<SocialPost> OnAddPost;
    public event Action<SocialPost> OnPostChange;
    public event Action<SocialPost, SocialReactionData, int> OnPostLike;

    public void AddPost(SocialPost post)
    {
        posts.Add(post);
        post.OnPostChange += PostChange;
        post.OnPostLike += PostLikeWork;
        OnChange?.Invoke();
        OnAddPost?.Invoke(post);
    }

    private void PostLikeWork(SocialPost post, SocialReactionData data, int count)
    {
        OnPostLike?.Invoke(post, data, count);
    }

    private void PostChange(SocialPost post)
    {
        OnPostChange?.Invoke(post);
    }
}
public interface ISocialPublicView
{
    void TryShow(SocialPublic socialPublic);
    ISocialPostView GetPostView(SocialPost socialPost);
}