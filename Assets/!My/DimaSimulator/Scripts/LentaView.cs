using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LentaView : MonoBehaviour
{
    [SerializeField] private Transform postsContainer; // родитель для элементов постов
    [SerializeField] private GameObject postPrefab;    // префаб PostView
    [SerializeField] private ArtistsPresenter authorsPresenter;

    private Lenta currentLenta;
    private Dictionary<Post, PostView> postViews = new Dictionary<Post, PostView>();

    private ScrollRect _scrollRect;
    private ContentSizeFitter _contentFitter;

    void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _contentFitter = postsContainer.GetComponent<ContentSizeFitter>();
    }

    public void Initialize(Lenta lenta)
    {
        // Отписываемся от старой ленты, если была
        if (currentLenta != null)
        {
            UnSubs();
        }

        currentLenta = lenta;
        // Подписываемся на события
        currentLenta.OnPostAdded += OnPostAdded;
        currentLenta.OnPostRemoved += OnPostRemoved;
        currentLenta.OnAuthorRemove += OnAuthorRemove;
        currentLenta.OnAuthorAdd += OnAuthorAdd;

        // Очищаем текущие посты
        foreach (var view in postViews.Values)
            Destroy(view.gameObject);
        postViews.Clear();

        // Отображаем все уже существующие посты
        foreach (var post in lenta.posts)
        {
            CreatePostView(post);
        }
    }

    private void OnPostAdded(Post post)
    {
        CreatePostView(post);
    }

    private void OnPostRemoved(Post post)
    {
        if (postViews.TryGetValue(post, out PostView view))
        {
            postViews.Remove(post);
            view.RemoveAnim();
            Destroy(view.gameObject, 0.5f);
        }
    }

    private void CreatePostView(Post post)
    {
        GameObject go = Instantiate(postPrefab, postsContainer);
        PostView view = go.GetComponent<PostView>();
        view.Initialize(post);
        postViews[post] = view;

        // Принудительно перестраиваем лэйаут поста и контейнера
        LayoutRebuilder.ForceRebuildLayoutImmediate(view.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(postsContainer as RectTransform);
        _scrollRect.Rebuild(CanvasUpdate.PreRender);
        Canvas.ForceUpdateCanvases(); // финальный апдейт
    }

    private void OnAuthorAdd(Author author)
    {
        authorsPresenter.AddAuthorButton(author);
    }

    private void OnAuthorRemove(Author author)
    {
        authorsPresenter.RemoveAuthorButton(author);
    }

    private void OnDestroy()
    {
        if (currentLenta != null)
        {
            UnSubs();
        }
    }

    private void UnSubs()
    {
        currentLenta.OnPostAdded -= OnPostAdded;
        currentLenta.OnPostRemoved -= OnPostRemoved;
        currentLenta.OnAuthorAdd -= OnAuthorAdd;
        currentLenta.OnAuthorRemove -= OnAuthorRemove;
    }
}