using System.Collections;
using UnityEngine;

public class TextSpriteScoreUpLR : TextScoreUpLR
{
    [Space, Header("Sprite")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void SetSprite(Sprite sprite)
    {
        SetSprite(sprite, Color.white);
    }
    public void SetSprite(Sprite sprite, Color color)
    {
        SetSprite(sprite, color, Vector3.one);
    }
    public void SetSprite(Sprite sprite, Vector3 coefSize)
    {
        SetSprite(sprite, Color.white, coefSize, Vector2.zero);
    }
    public void SetSprite(Sprite sprite, Color color, Vector3 coefSize)
    {
        SetSprite(sprite, color, coefSize, Vector2.zero);
    }
    public void SetSprite(Sprite sprite, Vector2 offset)
    {
        SetSprite(sprite, Color.white, Vector3.one, offset);
    }
    public void SetSprite(Sprite sprite, Color color, Vector2 offset)
    {
        SetSprite(sprite, color, Vector3.one, offset);
    }

    public void SetSprite (Sprite sprite, Color color, Vector3 coefSize, Vector2 offset)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.transform.localScale = Vector3.Scale(spriteRenderer.transform.localScale, coefSize);
        spriteRenderer.transform.localPosition += (Vector3)offset;
        spriteRenderer.color = color;
    }
}