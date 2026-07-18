using System;
using System.Collections.Generic;
using UnityEngine;

public class LabyrinthModel : MonoBehaviour
{
    [SerializeField] private LabyrinthData data = null;
    public virtual LabyrinthData Data => data;

    public Action<LabyrinthData> OnLabirintDataUpdate;
    public Action<RoomViewInformation> OnRoomViewUpdate;

    public void Init (LabyrinthData data)
    {
        this.data = data.DeepClone();
    }

    public void UpdateAllData()
    {
        UpdateData(Data);
    }

    private void UpdateData(LabyrinthData data)
    {
        this.data = data;
        OnLabirintDataUpdate?.Invoke(data);
    }

    public void UpdateRoomData (RoomViewInformation roomView)
    {
        OnRoomViewUpdate?.Invoke(roomView);
    }

    public static RoomViewInformation CreateRoomInfo(SceneExecuterContext context, LabyrinthObjectTransform playerTransform)
    {
        var NodeInvoker = context.NodeInvoker;
        var Labyrinth = context.Labyrinth;

        LabyrinthTransition[] transitions = Labyrinth.GetTransitionsOut(NodeInvoker, true);
        GetTransitions(context, playerTransform, transitions, out List<MoveDirection> directions, out List<MoveDirection> lockable, out List<MoveDirection> unlockable);

        //Есть ли в комнате выход?
        MoveDirection directionToExit = MoveDirection.NotConnection;
        if (NodeInvoker.isEnterNode)
        {
            directionToExit = Labyrinth.GetLocalDirection(playerTransform.DirectionView,
                NodeInvoker.toExitDirecrion);
        }

        //Собираем информацию о комнате для отображения её в интерфейсе
        RoomViewInformation info = NodeInvoker.GetRoomInformation();
        info = new RoomViewInformation()
        {
            Node = info.Node,
            transDirections = directions.ToArray(),
            toExitDirecrion = directionToExit,
            localbleDirections = lockable.ToArray(),
            unlockableDirections = unlockable.ToArray()
        };

        return info;
    }

    public static void GetTransitions(SceneExecuterContext context, LabyrinthObjectTransform playerTransform,
        LabyrinthTransition[] transitions,
        out List<MoveDirection> directions,
        out List<MoveDirection> lockDirections,
        out List<MoveDirection> unlockDirections)
    {
        GetTransitions(
            context.NodeInvoker,
            context.Labyrinth,
            playerTransform,
            transitions,
            out directions,
            out lockDirections,
            out unlockDirections);
    }

    public static void GetTransitions(LabyrinthNode NodeInvoker, 
        LabyrinthData Labyrinth, 
        LabyrinthObjectTransform playerTransform,
        LabyrinthTransition[] transitions,
        out List<MoveDirection> directions,
        out List<MoveDirection> lockable,
        out List<MoveDirection> unlockable)
    {
        directions = new List<MoveDirection>();
        lockable = new List<MoveDirection>();
        unlockable = new List<MoveDirection>();

        foreach (var transition in transitions)
        {
            LabyrinthNode labyrinthNodeTo = transition.NodeID1 == NodeInvoker.ID ?
                Labyrinth.GetNode(transition.NodeID2) :
                Labyrinth.GetNode(transition.NodeID1);

            MoveDirection localDirection = Labyrinth.GetLocalDirection(playerTransform.DirectionView,
                NodeInvoker,
                labyrinthNodeTo);

            if (!string.IsNullOrEmpty(labyrinthNodeTo.LockKey))
            {
                if (LabyrintSceneMemory.TryGetLock(labyrinthNodeTo.LockKey))
                    unlockable.Add(localDirection);
                else
                {
                    lockable.Add(localDirection);
                    continue;
                }
            }

            directions.Add(localDirection);
        }
    }
}