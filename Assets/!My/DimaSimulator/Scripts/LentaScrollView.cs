using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LentaScrollView : MonoBehaviour
{
    [SerializeField] private Transform contentPost;
    [SerializeField] private PostMonoView postPref;
    [SerializeField] private bool isLenta;

    private List<PostMonoView> postsMono = new List<PostMonoView>();

    private ScrollRect _scrollRect;
    private ContentSizeFitter _contentFitter;

    [Space(20)]
    [SerializeField] private PostRectionType[] testIDEmoji;

    void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _contentFitter = contentPost.GetComponent<ContentSizeFitter>();
    }

    void Start()
    {
        if (isLenta)
            TestButtons.SubOnClick("AddRandomPost", _ => AddRandomPost());
    }

    public void AddRandomPost()
    {
        CreatePost(new SocialPost
        {
            MainImageID = DimonGameData.R.AnimeSprites.RandomElement().name,
            Likes = Random.Range(0, 1000),
            Dislikes = Random.Range(0, 1000),
            CanEmoji = testIDEmoji
        });
    }

    public void CreatePost(SocialPost socialPost, bool isAnimation = true)
    {
        PostMonoView postView = Instantiate(postPref, contentPost);
        (postView as ISocialPostView).Init(socialPost, isAnimation, isLenta);
        postsMono.Add(postView);
        socialPost.OnPostChange += (post) => postView.UpdateData(post, isLenta);

        // Принудительно перестраиваем лэйаут поста и контейнера
        LayoutRebuilder.ForceRebuildLayoutImmediate(postView.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPost as RectTransform);
        _scrollRect.Rebuild(CanvasUpdate.PreRender);
        Canvas.ForceUpdateCanvases(); // финальный апдейт
    }

    public void Clear()
    {
        foreach (var item in postsMono)
        {
            Destroy(item.gameObject);
        }
        postsMono.Clear();
    }

    public ISocialPostView GetPostView(SocialPost socialPost)
    {
        return postsMono.First(x => x.Post == socialPost);
    }
}