using TMPro;
using UnityEngine;

public class ArtistButton : MonoBehaviour
{
    [SerializeField] private TMP_Text nameTmp;

    public void Init(Artist artist)
    {
        nameTmp.text = artist.ArtistData.Name;
    }
}
