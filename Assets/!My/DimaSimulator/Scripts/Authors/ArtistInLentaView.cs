using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtistInLentaView : MonoBehaviour
{
    [SerializeField] private TMP_Text nameTmp;

    private Author author;

    public void Init(Author author)
    {
        this.author = author;
        nameTmp.text = author.authorName;
    }

    public void UnSub()
    {
        G.PlayerInGame.UnsubscribeFromAuthor(author);
    }

    public void Delite()
    {
        Destroy(gameObject);
    }
}
