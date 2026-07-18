using System;
using UnityEngine;

public enum MoveDirection
{
    Fwd,
    Back,
    Left,
    Right,
    NotConnection
}

[Serializable]
public class LabyrinthObjectTransform
{
    public string NodeID;
    public Vector2Int DirectionView;
    private LabyrinthData data;

    public Action<Vector2Int> OnPositionUpdate;
    public Action<Vector2Int> OnDirectionUpdate;
    public Action<Vector2Int> OnTranslate;

    public LabyrinthData Data => data;

    public void Init(LabyrinthData data, string startNodeID, Vector2Int startDirection)
    {
        Debug.Log($"data {startNodeID}");

        this.data = data;
        NodeID = startNodeID;
        DirectionView = startDirection;

        UpdatePosition();
        UpdateDirection();
    }    

    public void SetPosition(string nodeID)
    {
        NodeID = nodeID;
        UpdatePosition();
    }

    public void SetPosition(LabyrinthNode node)
    {
        NodeID = node.ID;
        UpdatePosition();
    }

    public void SetDirection(Vector2Int direction)
    {
        DirectionView = direction;
        UpdateDirection();
    }

    public bool Translate(MoveDirection move, bool rotateToMoveDirection = true)
    {
        switch (move)
        {
            case MoveDirection.Fwd:
                return TranslateGlobal(DirectionView, rotateToMoveDirection);

            case MoveDirection.Left:
                    return TranslateGlobal(new Vector2Int(DirectionView.y, -DirectionView.x), rotateToMoveDirection);

            case MoveDirection.Back:
                return TranslateGlobal(-DirectionView, rotateToMoveDirection);

            case MoveDirection.Right:
                return TranslateGlobal(new Vector2Int(-DirectionView.y, DirectionView.x), rotateToMoveDirection);

            default:
                return false;
        }
    }

    public bool TranslateGlobal(Vector2Int directionMove, bool rotateToMoveDirection = true)
    {
        LabyrinthNode targetNode = data.TryMoveDirection(NodeID, directionMove);
        if (targetNode != null)
        {
            SetPosition(targetNode.ID);
            if(rotateToMoveDirection)
                SetDirection(directionMove);
        }

        OnTranslate?.Invoke(directionMove);

        return targetNode != null;
    }

    public void Rotate(MoveDirection move)
    {
        switch (move)
        {
            case MoveDirection.Left:
                SetDirection(new Vector2Int(DirectionView.y, -DirectionView.x));
                break;
            case MoveDirection.Right:
                SetDirection(new Vector2Int(-DirectionView.y, DirectionView.x));
                break;
        }
    }

    private void UpdatePosition()
    {
        UpdatePosition(GetPosition());
    }

    public virtual void UpdatePosition(Vector2Int position)
    {
        OnPositionUpdate?.Invoke(position);
    }

    private void UpdateDirection()
    {
        UpdateDirection(DirectionView);
    }

    public virtual void UpdateDirection(Vector2Int direction)
    {
        OnDirectionUpdate?.Invoke(direction);
    }

    public Vector2Int GetPosition()
    {
        try
        {
            return data.GetNode(NodeID).position;
        }
        catch (Exception e)
        {
            Debug.Log($"NodeID: {NodeID}");
            Debug.Log($"Position: {data.GetNode(NodeID) != null}");
            Debug.Log($"{data.GetNode(NodeID).position}");
            throw;
        }
    }

    internal Vector2Int GetDirection()
    {
        return DirectionView;
    }
}