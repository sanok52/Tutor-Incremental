using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LabyrinthData
{
    public LabyrinthNode[] labyrinthNodes;
    public LabyrinthTransition[] labyrinthTransitions;
    public float editorScale = 0.01f;

    public (LabyrinthNode, LabyrinthNode) GetNodes(LabyrinthTransition transition)
    {
        return GetNodes(transition.NodeID1, transition.NodeID2);
    }

    public (LabyrinthNode, LabyrinthNode) GetNodes(string ID1, string ID2)
    {
        return (GetNode(ID1), GetNode(ID2));
    }

    public LabyrinthNode GetNode(string id)
    {
        return labyrinthNodes.FirstOrDefault(x => x.ID == id);
    }

    public LabyrinthNode TryMoveDirection(string NodeIDOut, Vector2Int directionView)
    {
        if (directionView == Vector2Int.zero)
            return null;

        LabyrinthNode nodeOut = GetNode(NodeIDOut);
        LabyrinthTransition[] transitions = labyrinthTransitions.Where(x => x.NodeID1 == NodeIDOut || x.NodeID2 == NodeIDOut).ToArray();
        foreach (LabyrinthTransition transition in transitions)
        {
            (LabyrinthNode node1, LabyrinthNode node2) = GetNodes(transition);
            LabyrinthNode nodeIn = node1 == nodeOut ? node2 : node1;

            if ((directionView.x == 0 && nodeIn.position.x == nodeOut.position.x) && directionView.y != 0) //Ось Y
            {
                if (directionView.y < 0) //Движемся вверх
                {
                    if (nodeIn.position.y < nodeOut.position.y) //Если узел, в который мы хотим попасть, выше текущего узла, то можно двигаться
                        return nodeIn;
                }
                else  //Движемся вниз
                {
                    if (nodeIn.position.y > nodeOut.position.y) //Если узел, в который мы хотим попасть, ниже текущего узла, то можно двигаться
                        return nodeIn;
                }
            }
            else if ((directionView.y == 0 && nodeIn.position.y == nodeOut.position.y) && directionView.x != 0) //Ось X
            {
                if (directionView.x > 0) //Движемся вправо
                {
                    if (nodeIn.position.x > nodeOut.position.x) //Если узел, в который мы хотим попасть, правее текущего узла, то можно двигаться
                        return nodeIn;
                }
                else  //Движемся влево
                {
                    if (nodeIn.position.x < nodeOut.position.x) //Если узел, в который мы хотим попасть, левее текущего узла, то можно двигаться
                        return nodeIn;
                }
            }
        }

        return null;
    }

    public LabyrinthNode GetEnterNode()
    {
        return labyrinthNodes.First(x => x.isEnterNode);
    }

    public LabyrinthTransition[] GetTransitionsOut(LabyrinthNode node, bool onlyCloser)
    {
        // Все переходы, где node является одним из двух концов   
        List<LabyrinthTransition> allResult = labyrinthTransitions
            .Where(t => t.NodeID1 == node.ID || t.NodeID2 == node.ID).ToList();

        allResult.RemoveAll((result) =>
        {
            var neighbor = GetNode(result.NodeID1 == node.ID ? result.NodeID2 : result.NodeID1);
            return neighbor.IsOnlyOut && result.NodeID1 == neighbor.ID;
        });

        if (!onlyCloser)
            return allResult.ToArray(); // возвращаем всё

        // Переменные для хранения лучшего перехода в каждом направлении
        LabyrinthTransition bestUp = null;
        LabyrinthTransition bestDown = null;
        LabyrinthTransition bestLeft = null;
        LabyrinthTransition bestRight = null;

        // Также храним сами узлы, чтобы не вызывать GetNode повторно
        LabyrinthNode nodeUp = null;
        LabyrinthNode nodeDown = null;
        LabyrinthNode nodeLeft = null;
        LabyrinthNode nodeRight = null;

        foreach (var trans in allResult)
        {
            // Соседний узел (не node)
            var neighbor = GetNode(trans.NodeID1 == node.ID ? trans.NodeID2 : trans.NodeID1);
            int dx = neighbor.position.x - node.position.x;
            int dy = neighbor.position.y - node.position.y;

            // Игнорируем диагональные (хотя по условию их быть не должно)
            if (dx != 0 && dy != 0) continue;

            // Расстояние (можно для сравнения)
            int distance = Math.Abs(dx) + Math.Abs(dy);

            // Направление вверх (dy > 0)
            if (dy > 0 && dx == 0)
            {
                if (bestUp == null || distance < (nodeUp.position.y - node.position.y))
                {
                    bestUp = trans;
                    nodeUp = neighbor;
                }
            }
            // Направление вниз (dy < 0)
            else if (dy < 0 && dx == 0)
            {
                if (bestDown == null || distance < (node.position.y - nodeDown.position.y))
                {
                    bestDown = trans;
                    nodeDown = neighbor;
                }
            }
            // Направление вправо (dx > 0)
            else if (dx > 0 && dy == 0)
            {
                if (bestRight == null || distance < (nodeRight.position.x - node.position.x))
                {
                    bestRight = trans;
                    nodeRight = neighbor;
                }
            }
            // Направление влево (dx < 0)
            else if (dx < 0 && dy == 0)
            {
                if (bestLeft == null || distance < (node.position.x - nodeLeft.position.x))
                {
                    bestLeft = trans;
                    nodeLeft = neighbor;
                }
            }
        }

        // Собираем результат: все ненулевые переходы
        var result = new List<LabyrinthTransition>();
        if (bestUp != null) result.Add(bestUp);
        if (bestDown != null) result.Add(bestDown);
        if (bestLeft != null) result.Add(bestLeft);
        if (bestRight != null) result.Add(bestRight);

        return result.ToArray();
    }

    public Vector2Int DirectionNodeInNode (LabyrinthNode nodeOut, LabyrinthNode nodeIn)
    {
        if(nodeIn.position.x == nodeOut.position.x || nodeIn.position.y == nodeOut.position.y)
        {
            Vector2Int delta = (nodeIn.position - nodeOut.position);
            return new Vector2Int(Mathf.Clamp(delta.x, -1, 1), Mathf.Clamp(delta.y, -1, 1));
        }

        return Vector2Int.zero;
    }

    public MoveDirection GetLocalDirection(Vector2Int directionView, Vector2Int directionMove)
    {
        // Инвертируем Y, чтобы работать в стандартной системе (Y вверх)
        Vector2Int v = new Vector2Int(directionView.x, -directionView.y);
        Vector2Int m = new Vector2Int(directionMove.x, -directionMove.y);

        int dot = v.x * m.x + v.y * m.y;
        int cross = v.x * m.y - v.y * m.x;

        if (dot == 1)
            return MoveDirection.Fwd;
        if (dot == -1)
            return MoveDirection.Back;
        // cross > 0 означает, что m находится слева от v
        if (cross > 0)
            return MoveDirection.Left;
        if (cross < 0)
            return MoveDirection.Right;
        return MoveDirection.NotConnection;

    }

    public MoveDirection GetLocalDirection (Vector2Int directionView, LabyrinthTransition transition)
    {
        return GetLocalDirection(directionView, DirectionNodeInNode(GetNode(transition.NodeID1), GetNode(transition.NodeID2)));
    }

    public MoveDirection GetLocalDirection(Vector2Int directionView,
        LabyrinthNode nodeOut, LabyrinthNode nodeIn)
    {
        return GetLocalDirection(directionView, DirectionNodeInNode(nodeOut, nodeIn));
    }

    public LabyrinthData Clone()
    {
        return DeepClone();
    }

    public LabyrinthData DeepClone()
    {
        string json = JsonUtility.ToJson(this);
        return JsonUtility.FromJson<LabyrinthData>(json);
    }
}