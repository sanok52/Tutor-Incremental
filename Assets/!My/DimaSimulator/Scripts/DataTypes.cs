using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReactionTypeInfo
{
    public ReactionType type;
    public Sprite icon;
    public int likesValue;        // на сколько лайков влияет
    public bool isNegative;       // отрицательная реакция?

    public ReactionTypeInfo Clone()
    {
        return new ReactionTypeInfo()
        {
            type = type,
            icon = icon,
            likesValue = likesValue,
            isNegative = isNegative,
        };
    }
}
// ---- Автор ----
[Serializable]
public class Author : IListable
{
    public string id;
    public string authorName;
    public List<Post> possiblePosts; // шаблоны постов, которые автор может создать

    [Header("Интервалы (сек)")]
    public float minInterval = 5f;
    public float maxInterval = 20f;

    [Space]
    public int StartRang = 3;

    public Author(string id, string name, List<Post> possiblePosts)
    {
        this.id = id;
        this.authorName = name;
        this.possiblePosts = possiblePosts ?? new List<Post>();
    }

    public Author Clone()
    {
        return new Author(id, authorName, possiblePosts)
        {
            maxInterval = minInterval,
            minInterval = maxInterval,
            StartRang = StartRang
        };
    }
}

public class AuthorAdditionData
{
    public string id;
    public int rang;

    public AuthorAdditionData(string id, int startRang)
    {
        this.rang = startRang;
        this.id = id;
    }
}

// ---- Пост ----
[System.Serializable]
public class Post
{
    public int id;                 // уникальный ID (генерируется при создании)
    public Author author;
    public Sprite image;
    public string caption;
    public DateTime creationTime;
    public int Rang;

    public List<string> HashTags = new List<string>();

    // Реакции: словарь тип → количество
    public Dictionary<ReactionType, int> reactions = new Dictionary<ReactionType, int>();

    public event System.Action<ReactionType, int> OnReactionChanged; // тип, новое количество

    public Post(Author author, Sprite image, string caption, string[] hashTags)
    {
        this.id = UnityEngine.Random.Range(1, int.MaxValue); // упрощённо
        this.author = author;
        this.image = image;
        this.caption = caption;
        this.creationTime = DateTime.Now;
        this.HashTags.AddRange(hashTags);
    }


    public void AddReaction(ReactionType type, int count = 1)
    {
        if (reactions.ContainsKey(type))
            reactions[type] += count;
        else
            reactions[type] = count;
        OnReactionChanged?.Invoke(type, reactions[type]);
    }

    // Получить количество реакций типа
    public int GetReactionCount(ReactionType type)
    {
        return reactions.TryGetValue(type, out int val) ? val : 0;
    }
}

// ---- Лента постов ----
[System.Serializable]
public class Lenta
{
    public int id;
    public List<Post> posts = new List<Post>();
    public List<Author> authors = new List<Author>();
    public int maxSize = 100; // максимальное количество постов в ленте

    // События для уведомления View
    public event Action<Post> OnPostAdded;
    public event Action<Post> OnPostRemoved;
    public event Action<Author> OnAuthorAdd;
    public event Action<Author> OnAuthorRemove;

    public Lenta(int id, int maxSize = 100)
    {
        this.id = id;
        this.maxSize = maxSize;
    }

    // Метод для изменения максимального размера
    public void SetMaxSize(int newMaxSize)
    {
        if (newMaxSize < 1) newMaxSize = 1;
        maxSize = newMaxSize;
        // Если текущее количество постов превышает новый лимит – удаляем самые старые
        while (posts.Count > maxSize)
        {
            RemovePost(posts[0]); // удаляем самый старый (индекс 0)
        }
    }

    public void AddPost(Post post)
    {
        // Если при добавлении нового поста лимит будет превышен – удаляем самый старый
        if (posts.Count >= maxSize)
        {
            // Удаляем самый старый пост (первый в списке)
            RemovePost(posts[0]);
        }
        // Теперь добавляем новый
        posts.Add(post);
        OnPostAdded?.Invoke(post);
    }

    public void RemovePost(Post post)
    {
        if (posts.Remove(post))
            OnPostRemoved?.Invoke(post);
    }

    public void AddAuthor(Author author)
    {
        authors.Add(author);
        OnAuthorAdd?.Invoke(author);
    }

    public void RemoveAuthor(Author author)
    {
        authors.Remove(author);
        OnAuthorRemove?.Invoke(author);
    }
}

[System.Serializable]
public class ReactionSetting
{
    public ReactionType type;
    public int maxCount = 5;
    public float cooldown = 10f;
}

[Serializable]
public class Mission
{
    // События, которые генерирует сама миссия
    public event Action<int> OnTargetProgressChanged;   // (индекс цели, новое значение)
    public event Action<int> OnPhaseChanged;            // (новый индекс фазы)
    public event Action OnCompleted;                    // миссия полностью завершена

    public MissionTarget[] Targets;
    public int Phase = 0;
    public bool IsCompleted;

    public Post LastPost;
    private int LastLikePhase = -1;

    public void AddInCurrentPhase(int count)
    {
        if (Phase >= Targets.Length || IsCompleted) return;

        Targets[Phase].Count += count;
        LastLikePhase = Phase;
        OnTargetProgressChanged?.Invoke(Phase);

        Phase++;
        OnPhaseChanged?.Invoke(Phase);

        if (Phase >= Targets.Length)
            Complete();
    }

    public void AddInLastPhase(int count)
    {
        if (LastLikePhase < 0 || LastLikePhase >= Targets.Length || IsCompleted) return;
        Targets[LastLikePhase].Count += count;
        OnTargetProgressChanged?.Invoke(LastLikePhase);
    }

    public void Complete()
    {
        IsCompleted = true;
        OnCompleted?.Invoke();
    }
}


[Serializable]
public class MissionTarget
{
    public string TargetTag;
    public int Count;
}