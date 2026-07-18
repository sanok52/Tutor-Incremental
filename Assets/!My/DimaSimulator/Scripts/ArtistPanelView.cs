using System.Collections.Generic;
using UnityEngine;

public class ArtistPanelView : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private ArtistButton artistButtonPref;
    private List<ArtistButton> artistButtons = new List<ArtistButton>();    

    public void AddArtistButton(Artist artist)
    {
        var button = Instantiate(artistButtonPref, content);
        artistButtons.Add(button);
        button.Init(artist);
    }
}
