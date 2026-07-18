using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class LabyrinthNode
{
    public string ID;
    public Vector2Int position;
    public bool isEnterNode;
    public Vector2Int toExitDirecrion = Vector2Int.down;
    public Vector2 editorPosition; // Не используется в рантайме

    [Space]
    [SerializeField] public string Description;

    [Space]
    [SerializeField] public LNodeBehContainer[] Behaviours;
    public bool IsOnlyOut;
    public string LockKey;

    public Color GetEditorColor()
    {
        return isEnterNode ? Color.magenta : Color.white;
    }

    public LNodeBehContainer[] GetBehaviourContainers()
    {
        return Behaviours;
    }

    public RoomViewInformation GetRoomInformation()
    {
        return new RoomViewInformation
        {
            Node = this            
        };
    }

    public SceneExecuterContext ApplyDataToContext (SceneExecuterContext context)
    {
        context.NodeInvoker = this;
        return context;
    }
}