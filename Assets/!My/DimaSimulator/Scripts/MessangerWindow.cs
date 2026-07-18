using System;
using UnityEngine;

public class MessangerWindow : MonoBehaviour
{
    [SerializeField] private GameObject currentPublicViewGO;
    [SerializeField] private string currentPublicID;
    private ISocialPublicView currentPublicView;

    public GameObject CurrentPublicViewGO => currentPublicViewGO;

    private void Start()
    {
        currentPublicView = currentPublicViewGO.GetComponent<ISocialPublicView>();

        SocialUIManager.Instance.OnPublicChange += OnPublicChangeWork;
        SocialUIManager.Instance.OnPublicPostChange += OnPostChangeWork;
        SocialUIManager.Instance.OnPostLike += OnPostLikeWork;
    }

    private void OnPublicChangeWork(SocialPublic socialPublic)
    {
        if(socialPublic.ID == currentPublicID)
        {
            currentPublicView.TryShow(socialPublic);
        }
    }

    public void SetCurrentPublic(string publicID)
    {
        currentPublicID = publicID;
        currentPublicView.TryShow(SocialUIManager.Instance.Publics.Find(p => p.ID == publicID));
    }

    private void OnPostChangeWork(SocialPublic socialPublic, SocialPost post)
    {

    }

    private void OnPostLikeWork(SocialPublic socialPublic, SocialPost post, SocialReactionData data, int count)
    {
        if (data.IsNegative)
            return;

        Vector3? point = null;
        Transform pointTr = null;
        if(socialPublic.ID == currentPublicID)
        {
            pointTr = SocialUIManager.Instance.PublicViews[socialPublic.ID].GetPostView(post).GetReactionTransform(data.Type);
            if(pointTr != null)
                point = pointTr.position;
        }

        G.ResourceManager.AddResource("Likes", data.Power * count, null, null);
    }
}
