using System;
using System.Collections.Generic;
using UnityEngine;

public class ArtistsPresenter : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private ArtistInLentaView artistInLenta;

    private Dictionary<Author, ArtistInLentaView> keyValuesAuthors = new Dictionary<Author, ArtistInLentaView>();

    public void AddAuthorButton(Author author)
    {
        var element = Instantiate(artistInLenta, container);
        keyValuesAuthors.Add(author, element);
        element.Init(author);
    }

    public void RemoveAuthorButton(Author author)
    {
        keyValuesAuthors[author].Delite();
        keyValuesAuthors.Remove(author);
    }
}
