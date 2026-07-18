using System;
using System.Collections.Generic;
using UnityEngine;

public class SocialPublicView : MonoBehaviour, ISocialPublicView
{
    [SerializeField] private LentaScrollView lentaScroll;
    private SocialPublic currentPublic;
    private List<SocialPost> currentPosts = new List<SocialPost>();

    public void TryShow(SocialPublic socialPublic)
    {
        bool openNew = true;
        if (socialPublic == currentPublic)
            openNew = false;
        else
            lentaScroll.Clear();

        foreach (var post in socialPublic.posts)
        {
            if(openNew || !currentPosts.Contains(post))
                lentaScroll.CreatePost(post, !openNew);
        }

        currentPublic = socialPublic;
        currentPosts.Clear();
        currentPosts.AddRange(socialPublic.posts);
    }

    public ISocialPostView GetPostView(SocialPost socialPost)
    {
        return lentaScroll.GetPostView(socialPost);
    }
}