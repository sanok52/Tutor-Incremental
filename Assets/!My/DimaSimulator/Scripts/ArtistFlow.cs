using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ArtistFlow : MonoBehaviour
{
    public static ArtistFlow Instance;

    // Словарь для хранения корутин каждого автора
    private Dictionary<Author, Coroutine> artistCoroutines = new Dictionary<Author, Coroutine>();
    private Dictionary<Author, AuthorAdditionData> additionData = new Dictionary<Author, AuthorAdditionData>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        PocketRandomazer.CreatePocket("RangOffset", G.Data.RandomRangOffset);
    }

    // Запустить генерацию для автора
    public void StartArtist(Author author)
    {
        if (artistCoroutines.ContainsKey(author))
            return;

        if(!additionData.ContainsKey(author))
            additionData.Add(author, new AuthorAdditionData(author.id, author.StartRang));
        Coroutine coroutine = StartCoroutine(ArtistRoutine(author));
        artistCoroutines.Add(author, coroutine);
    }

    // Остановить генерацию для автора
    public void StopArtist(Author author)
    {
        if (artistCoroutines.TryGetValue(author, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            artistCoroutines.Remove(author);
        }
    }

    private IEnumerator ArtistRoutine(Author author)
    {
        while (true)
        {
            // Создаём пост, если у автора есть шаблоны
            if (author.possiblePosts.Count == 0)
                continue;

            // Выбираем случайный шаблон
            Post template = author.possiblePosts[Random.Range(0, author.possiblePosts.Count)];
            // Создаём новый пост (клон)
            Post newPost = new Post(author, template.image, template.caption, template.HashTags.ToArray());
            newPost.Rang = GetRandomRang(author);
            // Копируем дополнительные данные, если есть

            // Выбираем случайную ленту (если их несколько)
            Lenta targetLenta = G.InternetBank.GetLentaForArtist(author);
            if (targetLenta != null)
            {

                if (G.InternetBank.AllLents.Count > 0)
                {
                    targetLenta = G.InternetBank.AllLents[Random.Range(0, G.InternetBank.AllLents.Count)];
                }
                else
                {
                    // Если лент нет – создаём новую
                    targetLenta = G.InternetBank.CreateLenta();
                }

                // Добавляем пост в ленту
                G.InternetBank.AddPostToLenta(newPost, targetLenta);
                Debug.Log($"Автор {author.authorName} создал пост в ленте {targetLenta.id}");
            }

            // Случайная задержка
            float delay = Random.Range(author.minInterval, author.maxInterval);
            yield return new WaitForSeconds(delay);
        }
    }

    private int GetRandomRang(Author author)
    {
        return Mathf.Clamp(additionData[author].rang + PocketRandomazer.GetRandomElement<int>("RangOffset"), 0, 4);
    }

    // Запустить всех уже подписанных авторов
    public void StartAllArtists()
    {
        foreach (var author in G.PlayerInGame.subscribedAuthors)
        {
            StartArtist(author);
        }
    }

    public void UnSubAuthor(Author author)
    {
        StopArtist(author);
    }
}