using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class LabyrinthGraphView : GraphView
{
    private LabyrinthEditorWindow window;
    private LabyrinthDataSO dataSO;
    private Dictionary<string, LabyrinthNodeView> nodeViews = new();

    public LabyrinthGraphView(LabyrinthEditorWindow window)
    {
        this.window = window;
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new ClickSelector());
        this.AddManipulator(new RectangleSelector());
        graphViewChanged += OnGraphViewChanged;

        //  лик по фону Ц очистить инспектор
        RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == this)
                window.Inspector?.ClearInspector();
        });
    }

    public void LoadData(LabyrinthDataSO data)
    {
        dataSO = data;
        nodeViews.Clear();
        DeleteElements(graphElements.ToList());

        if (dataSO?.Data?.labyrinthNodes == null) return;

        float scale = dataSO.Data.editorScale;
        foreach (var node in dataSO.Data.labyrinthNodes)
        {
            if (node.editorPosition == Vector2.zero && node.position != Vector2Int.zero)
                node.editorPosition = new Vector2(node.position.x, node.position.y) / scale;

            var nodeView = new LabyrinthNodeView(node, this, window);
            nodeView.SetPositionWithoutNotify(new Rect(node.editorPosition, Vector2.zero));
            AddElement(nodeView);
            nodeViews[node.ID] = nodeView;
        }

        if (dataSO.Data.labyrinthTransitions != null)
        {
            foreach (var trans in dataSO.Data.labyrinthTransitions)
            {
                if (nodeViews.TryGetValue(trans.NodeID1, out var from) &&
                    nodeViews.TryGetValue(trans.NodeID2, out var to))
                {
                    var edge = from.OutputPort.ConnectTo(to.InputPort);
                    AddElement(edge);
                }
            }
        }
    }

    public void CreateNode()
    {
        if (dataSO == null) return;
        float scale = dataSO.Data.editorScale;
        Vector2 defaultPos = new Vector2(100, 100);
        var newNode = new LabyrinthNode
        {
            ID = "Node_" + System.Guid.NewGuid().ToString().Substring(0, 5),
            position = Vector2Int.RoundToInt(defaultPos * scale),
            editorPosition = defaultPos
        };
        var list = dataSO.Data.labyrinthNodes.ToList();
        list.Add(newNode);
        dataSO.Data.labyrinthNodes = list.ToArray();
        window.Save();

        var nodeView = new LabyrinthNodeView(newNode, this, window);
        nodeView.SetPositionWithoutNotify(new Rect(defaultPos, Vector2.zero));
        AddElement(nodeView);
        nodeViews[newNode.ID] = nodeView;
        ClearSelection();
        AddToSelection(nodeView);
        window.Inspector?.Inspect(newNode);
    }

    public void DeleteNode(LabyrinthNodeView nodeView)
    {
        var nodes = dataSO.Data.labyrinthNodes.ToList();
        nodes.Remove(nodeView.Node);
        dataSO.Data.labyrinthNodes = nodes.ToArray();

        var transitions = dataSO.Data.labyrinthTransitions?.ToList() ?? new List<LabyrinthTransition>();
        transitions.RemoveAll(t => t.NodeID1 == nodeView.Node.ID || t.NodeID2 == nodeView.Node.ID);
        dataSO.Data.labyrinthTransitions = transitions.ToArray();

        window.Save();
        RemoveElement(nodeView);
        nodeViews.Remove(nodeView.Node.ID);
        if (window.Inspector?.CurrentNode == nodeView.Node) window.Inspector.ClearInspector();
    }

    public void UpdateNodePosition(LabyrinthNode node, Vector2 newEditorPos)
    {
        node.editorPosition = newEditorPos;
        float scale = dataSO.Data.editorScale;
        node.position = Vector2Int.RoundToInt(newEditorPos * scale);
        window.Save();
        if (nodeViews.TryGetValue(node.ID, out var nodeView))
            nodeView.SetPositionWithoutNotify(new Rect(newEditorPos, Vector2.zero));
        // ќбновить инспектор, если он показывает эту ноду
        if (window.Inspector?.CurrentNode == node)
            window.Inspector.Refresh();
    }

    public void RenameNode(string oldId, string newId)
    {
        var transitions = dataSO.Data.labyrinthTransitions?.ToList() ?? new List<LabyrinthTransition>();
        bool changed = false;
        for (int i = 0; i < transitions.Count; i++)
        {
            if (transitions[i].NodeID1 == oldId) { transitions[i].NodeID1 = newId; changed = true; }
            if (transitions[i].NodeID2 == oldId) { transitions[i].NodeID2 = newId; changed = true; }
        }
        if (changed)
        {
            dataSO.Data.labyrinthTransitions = transitions.ToArray();
            window.Save();
        }
        if (nodeViews.TryGetValue(oldId, out var nodeView))
        {
            nodeViews.Remove(oldId);
            nodeViews[newId] = nodeView;
            nodeView.Node.ID = newId;
            nodeView.title = newId;
        }
        window.Save();
        if (window.Inspector?.CurrentNode?.ID == newId) window.Inspector.Refresh();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (change.edgesToCreate != null)
        {
            foreach (var edge in change.edgesToCreate)
            {
                var fromNode = (edge.output.node as LabyrinthNodeView)?.Node;
                var toNode = (edge.input.node as LabyrinthNodeView)?.Node;
                if (fromNode != null && toNode != null)
                {
                    var transList = dataSO.Data.labyrinthTransitions?.ToList() ?? new List<LabyrinthTransition>();
                    if (!transList.Any(t => t.NodeID1 == fromNode.ID && t.NodeID2 == toNode.ID))
                    {
                        transList.Add(new LabyrinthTransition
                        {
                            ID = System.Guid.NewGuid().ToString(),
                            NodeID1 = fromNode.ID,
                            NodeID2 = toNode.ID
                        });
                        dataSO.Data.labyrinthTransitions = transList.ToArray();
                        window.Save();
                    }
                }
            }
        }
        if (change.elementsToRemove != null)
        {
            foreach (var element in change.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    var fromNode = (edge.output.node as LabyrinthNodeView)?.Node;
                    var toNode = (edge.input.node as LabyrinthNodeView)?.Node;
                    if (fromNode != null && toNode != null)
                    {
                        var transList = dataSO.Data.labyrinthTransitions.ToList();
                        transList.RemoveAll(t => t.NodeID1 == fromNode.ID && t.NodeID2 == toNode.ID);
                        dataSO.Data.labyrinthTransitions = transList.ToArray();
                        window.Save();
                    }
                }
            }
        }
        return change;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();
    }
}