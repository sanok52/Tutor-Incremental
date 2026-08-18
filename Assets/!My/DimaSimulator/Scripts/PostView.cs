using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PostView : MonoBehaviour
{
    [SerializeField] private Image postImage;
    [SerializeField] private TMP_Text authorText;
    [SerializeField] private ReactionLine reactionLine; // ссылка на дочерний объект

    [Space]
    [SerializeField] private Button repostButton;

    [Space]
    [SerializeField] private Image imageRang;
    [SerializeField] private Color[] rangColors;
    [SerializeField] private Transform rangStarsContainer;
 
    private Post currentPost;

    public void Initialize(Post post, bool isAnimation = true)
    {
        currentPost = post;
        // Заполняем данные
        postImage.sprite = post.image;
        authorText.text = post.author.authorName;

        // Инициализируем линию реакций
        if (reactionLine != null)
            reactionLine.Initialize(post);

        if (isAnimation)
            StartCoroutine(ShowAnimationRoutine());

        InitRang(post.Rang);

        repostButton.onClick.AddListener(() => ClickRepost(post));
        G.PlayerInGame.OnReactionRepostCooldownStarted += RepostCooldownStart;
        G.PlayerInGame.OnReactionRepostCooldownEnded += RepostCooldownEnd;
        repostButton.interactable = !G.PlayerInGame.IsRechargingRepost;
    }

    private void InitRang(int rang)
    {
        imageRang.color = rangColors[Mathf.Clamp(rang, 0, rangColors.Length)];

        for (int i = 0; i < rangStarsContainer.childCount - 1; i++)
        {
            rangStarsContainer.GetChild(i + 1).gameObject.SetActive(rang >= i);
        }
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

    public void RemoveAnim()
    {
        StartCoroutine(HideAnimationRoutine());
    }

    private IEnumerator HideAnimationRoutine()
    {
        yield return null;
        ContentSizeFitter contentSizeFitter = GetComponent<ContentSizeFitter>();
        VerticalLayoutGroup verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        RectTransform rectTransform = GetComponent<RectTransform>();

        float targetHeight = verticalLayoutGroup.preferredHeight;
        contentSizeFitter.enabled = false;
        verticalLayoutGroup.childControlHeight = true;
        verticalLayoutGroup.childScaleHeight = true;

        yield return rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, 0f), 0.25f).SetEase(Ease.OutCubic).WaitForCompletion();
    }

    private void ClickRepost(Post post)
    {
        G.PlayerInGame.TryRepost(post);
    }

    private void RepostCooldownStart()
    {
        repostButton.interactable = false;
    }

    private void RepostCooldownEnd()
    {
        repostButton.interactable = true;
    }

    private void OnDisable()
    {
        G.PlayerInGame.OnReactionRepostCooldownStarted -= RepostCooldownStart;
        G.PlayerInGame.OnReactionRepostCooldownEnded -= RepostCooldownEnd;
    }
}