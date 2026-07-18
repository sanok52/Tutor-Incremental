using System;
using System.Collections.Generic;
using UnityEngine;

public class MainLentaBehaviour : MonoBehaviour
{
    [SerializeField] private ArtistPanelView artistPanelView;
    [SerializeField] private LentaScrollView mainLentaScroll;
    private List<Artist> artists = new List<Artist>();

    private void Start()
    {
        foreach (var artist in DimonGameData.R.Artists)
        {
            CreateArtist(artist);
        }
    }

    private void Update()
    {
        foreach (var artist in artists)
        {
            artist.NotPostTime += Time.deltaTime;
            if(artist.NotPostTime >= artist.PostInterval)
            {
                artist.NotPostTime = 0f;
                artist.BreakNotPostTimer();
                ArtistPost(artist);
            }
        }
    }

    private void ArtistPost(Artist artist)
    {
        mainLentaScroll.CreatePost(artist.CreateRandomPost(), true);
    }

    public void CreateArtist(ArtistData artistData)
    {
        var artist = new Artist { ArtistData = artistData };
        artists.Add(artist);
        ArtistPost(artist);
        artist.BreakNotPostTimer();
        artistPanelView.AddArtistButton(artist);
    }
}

[Serializable]
public class Artist
{
    public ArtistData ArtistData;
    public float NotPostTime;
    public float PostInterval;

    [Space]
    public int Carma = 1;

    public void BreakNotPostTimer()
    {
        NotPostTime = 0f;
        PostInterval = UnityEngine.Random.Range(ArtistData.PostInterval.x, ArtistData.PostInterval.y);
    }

    public SocialPost CreateRandomPost()
    {
        SocialPost post = ArtistData.posts.RandomElement().CloneForLenta();
        post.Likes *= Carma;
        post.Dislikes += Carma;
        post.virality = Mathf.Clamp((Carma / 10f), 0.1f, 1f);
        return post;
    }
}