using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class LabyrinthNodeView : Node
{
    public LabyrinthNode Node { get; private set; }
    public Port InputPort { get; private set; }
    public Port OutputPort { get; private set; }
    private LabyrinthGraphView graphView;
    private LabyrinthEditorWindow window;

    public LabyrinthNodeView(LabyrinthNode node, LabyrinthGraphView graphView, LabyrinthEditorWindow window)
    {
        Node = node;
        this.graphView = graphView;
        this.window = window;
        title = node.ID;

        InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        InputPort.portName = "In";
        OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        OutputPort.portName = "Out";

        inputContainer.Add(InputPort);
        outputContainer.Add(OutputPort);

        var deleteBtn = new Button(() => graphView.DeleteNode(this)) { text = "X" };
        deleteBtn.style.position = Position.Absolute;
        deleteBtn.style.top = 5;
        deleteBtn.style.right = 5;
        titleContainer.Add(deleteBtn);

        SetPositionWithoutNotify(new Rect(node.editorPosition, Vector2.zero));
        RefreshExpandedState();
    }

    public void SetPositionWithoutNotify(Rect newPos)
    {
        base.SetPosition(newPos);
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        graphView.UpdateNodePosition(Node, newPos.position);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        window.Inspector?.Inspect(Node);
    }

    public override void OnUnselected()
    {
        base.OnUnselected();
    }
}