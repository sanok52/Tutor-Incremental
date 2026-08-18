using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class InternetBank : IListable
{
    public List<Post> AllPosts = new List<Post>();
    public List<Lenta> AllLents = new List<Lenta>();
    public List<Author> AllAuthors = new List<Author>(); // активные авторы (подписанные)

    public InternetBank()
    {
        ObjectRegistry.Register(this);
    }

    // Добавить пост и поместить в указанную ленту
    public void AddPostToLenta(Post post, Lenta lenta)
    {
        if (!AllPosts.Contains(post))
            AllPosts.Add(post);
        if (!AllLents.Contains(lenta))
            AllLents.Add(lenta);
        lenta.AddPost(post);
    }

    // Создать новую ленту
    public Lenta CreateLenta()
    {
        var lenta = new Lenta(AllLents.Count + 1);
        AllLents.Add(lenta);
        return lenta;
    }

    // Добавить автора в активные
    public void AddAuthor(Author author)
    {
        if (!AllAuthors.Contains(author))
            AllAuthors.Add(author);
    }

    // Удалить автора
    public void RemoveAuthor(Author author)
    {
        var lenta = G.InternetBank.GetLentaForArtist(author);
        lenta.RemoveAuthor(author);
        AllAuthors.Remove(author);
    }

    // Поставить реакцию на пост (вызывается из PlayerGameData)
    public void PlaceReaction(Post post, ReactionType type)
    {
        post.AddReaction(type, 1);
        // Здесь можно вызвать событие обновления UI через PostView (через делегаты)
    }

    public Lenta GetLentaForArtist(Author author)
    {
        Debug.Log($"{author != null}");

        foreach (var lenta in AllLents)
        {
            if (lenta.authors.Any(x => x.id == author.id))
                return lenta;
        }
        //var lentaNew = CreateLenta();
        //lentaNew.AddAuthor(author);

        Debug.Log($"lenta no found");

        return null;
    }
}