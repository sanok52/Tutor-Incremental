using System;
using System.Collections.Generic;
using System.Linq;

public static class LabyrintSceneMemory
{
    private static List<LabyrinthNodeMemoryElement> NodesMemory = new List<LabyrinthNodeMemoryElement>();
    private static List<string> KeyMemory = new List<string>();

    public static LabyrinthNodeMemoryElement GetNodeMemory(LabyrinthNode node)
    {
        LabyrinthNodeMemoryElement nodeMemory = NodesMemory.Find(n => n.Node == node);
        if (nodeMemory == null)
        {
            nodeMemory = new LabyrinthNodeMemoryElement() { Node = node };
            NodesMemory.Add(nodeMemory);
        }
        return nodeMemory;
    }

    public static void InitNodeMemory(LabyrinthNode node)
    {
        if (NodesMemory.Any(x => x.Node == node))
            return;

        if (node.Behaviours == null)
            return;

        foreach (var behContainer in node.Behaviours)
        {
            if (behContainer == null)
                continue;

            if (behContainer.Behaviours == null)
                continue;

            foreach (var command in behContainer.Behaviours)
            {
                if (command == null)
                    continue;

                var mem = new LabyrinthNodeMemoryElement { Node = node };
                (command as LNodeBehCommand).ApplyDataToMemory(mem);
                NodesMemory.Add(mem);
            }
        }
    }

    public static void Clear()
    {
        NodesMemory.Clear();
        KeyMemory.Clear();
    }

    public static bool TryGetLock(string lockKey)
    {
        return KeyMemory.Contains(lockKey);
    }

    public static void GrapKey(string lockKey)
    {
        KeyMemory.Add(lockKey);
    }
}

[Serializable]
public class LabyrinthNodeMemoryElement
{
    public LabyrinthNode Node;
    public Dictionary<string, List<object>> Items = new Dictionary<string, List<object>>();
    public bool IsEnemy;
}