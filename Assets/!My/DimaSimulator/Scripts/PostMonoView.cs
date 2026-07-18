using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostMonoView : MonoBehaviour, ISocialPostView
{
    [SerializeField] private Image imageMain;
    [SerializeField] private TMP_Text likesTmp;
    [SerializeField] private TMP_Text dislikesTmp;

    [Space]
    [SerializeField] private GameObject lentaInfoLine;
    [SerializeField] private GameObject lentaActionLine;
    [SerializeField] private ReactionsLine reactionsLine;

    private SocialPost post;

    public SocialPost Post => post;

    public void Init(SocialPost socialPost, bool isAnimation, bool inLenta)
    {
        post = socialPost;
        imageMain.sprite = socialPost.GetMainImageSprite();
        likesTmp.text = socialPost.Likes.ToString();
        dislikesTmp.text = socialPost.Dislikes.ToString();

        lentaActionLine.SetActive(inLenta);
        lentaInfoLine.SetActive(inLenta);
        reactionsLine.gameObject.SetActive(!inLenta);

        reactionsLine.PostUpdate(socialPost.Reactions.ToArray());

        if (isAnimation)
            StartCoroutine(ShowAnimationRoutine());

        socialPost.OnPostChange += (post) => UpdateData(post, inLenta);
    }

    private IEnumerator ShowAnimationRoutine()
    {
        yield return null;
        ContentSizeFitter contentSizeFitter = GetComponent<ContentSizeFitter>();
        VerticalLayoutGroup verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        float targetHeight = verticalLayoutGroup.preferredHeight;
        contentSizeFitter.enabled = false;
        verticalLayoutGroup.childControlHeight = true;
        verticalLayoutGroup.childScaleHeight = true;

        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0f);

        yield return rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, targetHeight), 0.5f).SetEase(Ease.OutCubic).WaitForCompletion();

        verticalLayoutGroup.childControlHeight = false;
        verticalLayoutGroup.childScaleHeight = false;
        contentSizeFitter.enabled = true;
    }

    public void RepostInPublic(string id)
    {
        SocialUIManager.Instance.RepostPostInPublic(id, post);
    }

    public void UpdateData(SocialPost socialPost, bool inLenta)
    {
        reactionsLine.PostUpdate(socialPost.Reactions.ToArray());
    }

    public Transform GetReactionTransform(PostRectionType type)
    {
        return reactionsLine.GetReactionTransform(type);
    }
}

public interface ISocialPostView
{
    Transform GetReactionTransform(PostRectionType type);
    void Init(SocialPost socialPost, bool isAnimation, bool inLenta);
}