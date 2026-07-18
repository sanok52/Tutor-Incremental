using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogNode
{
    public string NodeID;
    public DialogNodeType nodeType;
    public string SpikerID;
    public string Text;
    public List<DialogActionInfo> actions = new List<DialogActionInfo>();

    [SerializeField] private Vector2 editorPosition; // <-- добавляем приватное поле с атрибутом
    public Vector2 EditorPosition
    {
        get => editorPosition;
        set => editorPosition = value;
    }
}