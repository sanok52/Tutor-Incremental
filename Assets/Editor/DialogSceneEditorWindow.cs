using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogSceneEditorWindow : EditorWindow
{
    private DialogScene currentScene;
    private DialogSceneGraphView graphView;
    private DialogInspectorView inspectorView;

    [MenuItem("Tools/Dialog Scene Editor")]
    public static void Open()
    {
        var window = GetWindow<DialogSceneEditorWindow>();
        window.titleContent = new GUIContent("Dialog Scene Editor");
        window.Show();
    }

    public void CreateGUI()
    {
        rootVisualElement.style.flexDirection = FlexDirection.Row;

        var leftPanel = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };

        var toolbar = new Toolbar();
        var objectField = new ObjectField("Dialog Scene")
        {
            objectType = typeof(DialogScene),
            allowSceneObjects = false
        };
        objectField.RegisterValueChangedCallback(evt =>
        {
            currentScene = evt.newValue as DialogScene;
            graphView?.LoadScene(currentScene);
            inspectorView?.ClearInspector();
        });
        toolbar.Add(objectField);

        var createButton = new Button(() => { if (currentScene == null) Debug.LogWarning("Select Dialog Scene first"); else graphView.CreateNode(); }) { text = "Create Node" };
        toolbar.Add(createButton);

        leftPanel.Add(toolbar);

        graphView = new DialogSceneGraphView(this);
        graphView.style.flexGrow = 1;
        leftPanel.Add(graphView);

        inspectorView = new DialogInspectorView(this, graphView);
        inspectorView.style.width = 300;
        inspectorView.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        inspectorView.style.marginLeft = 5;
        inspectorView.style.paddingTop = 30;

        rootVisualElement.Add(leftPanel);
        rootVisualElement.Add(inspectorView);

        if (currentScene != null) graphView.LoadScene(currentScene);
    }

    public void Save()
    {
        if (currentScene == null) return;
        EditorUtility.SetDirty(currentScene);
        AssetDatabase.SaveAssets();
    }

    public DialogScene CurrentScene => currentScene;
    public DialogInspectorView Inspector => inspectorView;
}

public class DialogSceneGraphView : GraphView
{
    private DialogSceneEditorWindow window;
    private DialogScene scene;
    private Dictionary<string, DialogNodeView> nodeViews = new();

    public DialogSceneGraphView(DialogSceneEditorWindow window)
    {
        this.window = window;
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new ClickSelector());
        this.AddManipulator(new RectangleSelector());
        graphViewChanged += OnGraphViewChanged;

        RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == this)
                window.Inspector?.ClearInspector();
        });
    }

    public void LoadScene(DialogScene newScene)
    {
        scene = newScene;
        nodeViews.Clear();
        DeleteElements(graphElements.ToList());

        if (scene == null) return;

        foreach (var node in scene.dialogScenes)
        {
            var nodeView = new DialogNodeView(node, this, window);
            nodeView.SetPositionWithoutNotify(new Rect(node.EditorPosition, Vector2.zero));
            AddElement(nodeView);
            nodeViews[node.NodeID] = nodeView;
        }

        if (scene.transitions != null)
        {
            foreach (var trans in scene.transitions)
            {
                if (nodeViews.TryGetValue(trans.IDOut, out var from) &&
                    nodeViews.TryGetValue(trans.IDIn, out var to))
                {
                    var edge = from.OutputPort.ConnectTo(to.InputPort);
                    AddElement(edge);
                }
            }
        }
    }

    public void CreateNode()
    {
        if (scene == null) return;
        Vector2 defaultPos = new Vector2(100, 100);
        var newNode = new DialogNode
        {
            NodeID = "Node_" + Guid.NewGuid().ToString().Substring(0, 5),
            nodeType = DialogNodeType.TextView,
            SpikerID = "",
            Text = "New Node",
            EditorPosition = defaultPos
        };
        scene.dialogScenes.Add(newNode);
        Save();

        var nodeView = new DialogNodeView(newNode, this, window);
        nodeView.SetPositionWithoutNotify(new Rect(defaultPos, Vector2.zero));
        AddElement(nodeView);
        nodeViews[newNode.NodeID] = nodeView;
        ClearSelection();
        AddToSelection(nodeView);
        window.Inspector?.Inspect(newNode);
    }

    public void DeleteNode(DialogNodeView nodeView)
    {
        scene.dialogScenes.Remove(nodeView.Node);
        if (scene.transitions != null)
            scene.transitions.RemoveAll(t => t.IDOut == nodeView.Node.NodeID || t.IDIn == nodeView.Node.NodeID);
        Save();

        RemoveElement(nodeView);
        nodeViews.Remove(nodeView.Node.NodeID);
        if (window.Inspector?.CurrentNode == nodeView.Node) window.Inspector.ClearInspector();
    }

    public void UpdateNodePosition(DialogNode node, Vector2 newEditorPos, bool fullSave = false)
    {
        node.EditorPosition = newEditorPos;
        EditorUtility.SetDirty(scene);      // всегда помечаем как изменённое
        if (fullSave)
            window.Save();                  // полная запись на диск
        if (window.Inspector?.CurrentNode == node)
            window.Inspector.Refresh();
    }

    public void RenameNode(string oldId, string newId)
    {
        if (string.IsNullOrEmpty(newId) || oldId == newId) return;
        if (scene.transitions != null)
        {
            var transitions = scene.transitions.ToList();
            bool changed = false;
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i].IDOut == oldId) { transitions[i].IDOut = newId; changed = true; }
                if (transitions[i].IDIn == oldId) { transitions[i].IDIn = newId; changed = true; }
            }
            if (changed)
            {
                scene.transitions = transitions;
                Save();
            }
        }
        if (nodeViews.TryGetValue(oldId, out var nodeView))
        {
            nodeViews.Remove(oldId);
            nodeViews[newId] = nodeView;
            nodeView.Node.NodeID = newId;
            nodeView.title = newId;
        }
        Save();
        if (window.Inspector?.CurrentNode?.NodeID == newId) window.Inspector.Refresh();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (change.edgesToCreate != null)
        {
            foreach (var edge in change.edgesToCreate)
            {
                var fromNode = (edge.output.node as DialogNodeView)?.Node;
                var toNode = (edge.input.node as DialogNodeView)?.Node;
                if (fromNode != null && toNode != null)
                {
                    var transList = scene.transitions?.ToList() ?? new List<DialogTransition>();
                    if (!transList.Any(t => t.IDOut == fromNode.NodeID && t.IDIn == toNode.NodeID))
                    {
                        transList.Add(new DialogTransition
                        {
                            TransitionID = Guid.NewGuid().ToString(),
                            IDOut = fromNode.NodeID,
                            IDIn = toNode.NodeID,
                            dialogTransitions = DialogTransitionType.Empty
                        });
                        scene.transitions = transList;
                        Save();
                        if (window.Inspector?.CurrentNode == fromNode)
                            window.Inspector.Refresh();
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
                    var fromNode = (edge.output.node as DialogNodeView)?.Node;
                    var toNode = (edge.input.node as DialogNodeView)?.Node;
                    if (fromNode != null && toNode != null)
                    {
                        var transList = scene.transitions.ToList();
                        transList.RemoveAll(t => t.IDOut == fromNode.NodeID && t.IDIn == toNode.NodeID);
                        scene.transitions = transList;
                        Save();
                        if (window.Inspector?.CurrentNode == fromNode)
                            window.Inspector.Refresh();
                    }
                }
            }
        }
        return change;
    }

    private void Save()
    {
        EditorUtility.SetDirty(scene);
        AssetDatabase.SaveAssets();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();
    }
}

public class DialogNodeView : Node
{
    public DialogNode Node { get; private set; }
    public Port InputPort { get; private set; }
    public Port OutputPort { get; private set; }
    private DialogSceneGraphView graphView;
    private DialogSceneEditorWindow window;
    private Vector2 lastPosition;

    public DialogNodeView(DialogNode node, DialogSceneGraphView graphView, DialogSceneEditorWindow window)
    {
        Node = node;
        this.graphView = graphView;
        this.window = window;
        title = node.NodeID;
        lastPosition = node.EditorPosition;

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

        SetPositionWithoutNotify(new Rect(node.EditorPosition, Vector2.zero));
        RefreshExpandedState();

        RegisterCallback<MouseUpEvent>(evt =>
        {
            // При отпускании мыши – сохраняем на диск
            graphView.UpdateNodePosition(Node, GetPosition().position, true);
        });
    }

    public void SetPositionWithoutNotify(Rect newPos)
    {
        base.SetPosition(newPos);
        lastPosition = newPos.position;
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        graphView.UpdateNodePosition(Node, newPos.position, false);
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

public class DialogInspectorView : VisualElement
{
    private DialogSceneEditorWindow window;
    private DialogSceneGraphView graphView;
    private DialogNode currentNode;
    private ScrollView scrollView;

    public DialogInspectorView(DialogSceneEditorWindow window, DialogSceneGraphView graphView)
    {
        this.window = window;
        this.graphView = graphView;
        scrollView = new ScrollView();
        Add(scrollView);
    }

    public DialogNode CurrentNode => currentNode;

    public void Inspect(DialogNode node)
    {
        currentNode = node;
        scrollView.Clear();

        if (node == null) return;

        // --- Node ID (isDelayed = true) ---
        var idField = new TextField("Node ID") { value = node.NodeID, isDelayed = true };
        idField.RegisterValueChangedCallback(evt =>
        {
            if (currentNode == null) return;
            string oldId = currentNode.NodeID;
            string newId = evt.newValue;
            if (string.IsNullOrEmpty(newId) || oldId == newId) return;
            currentNode.NodeID = newId;
            graphView.RenameNode(oldId, newId);
        });
        scrollView.Add(idField);

        // --- Spiker ID ---
        var spikerField = new TextField("Spiker ID") { value = node.SpikerID };
        spikerField.RegisterValueChangedCallback(evt =>
        {
            if (currentNode == null) return;
            currentNode.SpikerID = evt.newValue;
            EditorUtility.SetDirty(window.CurrentScene);
        });
        scrollView.Add(spikerField);

        // --- Node Type ---
        var typeField = new EnumField("Node Type", node.nodeType);
        typeField.RegisterValueChangedCallback(evt =>
        {
            if (currentNode == null) return;
            currentNode.nodeType = (DialogNodeType)evt.newValue;
            EditorUtility.SetDirty(window.CurrentScene);
        });
        scrollView.Add(typeField);

        // --- Text ---
        var textField = new TextField("Text") { value = node.Text, multiline = true };
        textField.style.whiteSpace = WhiteSpace.Normal;
        textField.RegisterValueChangedCallback(evt =>
        {
            if (currentNode == null) return;
            currentNode.Text = evt.newValue;
            EditorUtility.SetDirty(window.CurrentScene);
        });
        scrollView.Add(textField);

        // ---------- ACTIONS (Node) ----------
        var actionsLabel = new Label("Actions (executed when node starts)");
        actionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        actionsLabel.style.marginTop = 10;
        scrollView.Add(actionsLabel);
        CreateActionListUI(scrollView, node.actions, () => EditorUtility.SetDirty(window.CurrentScene));

        // ---------- TRANSITIONS ----------
        var transitionsLabel = new Label("Transitions (outgoing)");
        transitionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        transitionsLabel.style.marginTop = 10;
        scrollView.Add(transitionsLabel);

        var transitions = window.CurrentScene?.transitions?.Where(t => t.IDOut == node.NodeID).ToList() ?? new List<DialogTransition>();

        if (transitions.Count == 0)
        {
            var noTransLabel = new Label("No outgoing transitions. Drag from output port to create.");
            noTransLabel.style.color = Color.gray;
            scrollView.Add(noTransLabel);
        }
        else
        {
            foreach (var transition in transitions)
            {
                var container = new VisualElement();
                container.style.marginBottom = 10;
                container.style.borderTopWidth = 1;
                container.style.borderTopColor = Color.gray;
                container.style.paddingTop = 5;

                var targetNode = window.CurrentScene?.dialogScenes.FirstOrDefault(n => n.NodeID == transition.IDIn);
                var header = new Label($"→ {transition.IDIn} {(targetNode != null ? $"({targetNode.NodeID})" : "")}");
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                container.Add(header);

                // Transition Type
                var typeTransField = new EnumField("Type", transition.dialogTransitions);
                typeTransField.RegisterValueChangedCallback(evt =>
                {
                    transition.dialogTransitions = (DialogTransitionType)evt.newValue;
                    EditorUtility.SetDirty(window.CurrentScene);
                    Inspect(currentNode); // перерисовка для показа/скрытия полей
                });
                container.Add(typeTransField);

                // Choice Text (PlayerChoice)
                var choiceField = new TextField("Choice Text") { value = transition.choiceText ?? "" };
                choiceField.RegisterValueChangedCallback(evt =>
                {
                    transition.choiceText = evt.newValue;
                    EditorUtility.SetDirty(window.CurrentScene);
                });
                choiceField.style.display = transition.dialogTransitions == DialogTransitionType.PlayerChoice ? DisplayStyle.Flex : DisplayStyle.None;
                container.Add(choiceField);

                // Condition (PlayerChoice / ConditionTransition)
                bool showCondition = transition.dialogTransitions == DialogTransitionType.PlayerChoice ||
                                     transition.dialogTransitions == DialogTransitionType.ConditionTransition;
                var conditionContainer = new VisualElement();
                conditionContainer.style.display = showCondition ? DisplayStyle.Flex : DisplayStyle.None;
                conditionContainer.style.marginTop = 5;
                conditionContainer.style.marginLeft = 10;

                var conditionHeader = new Label("Condition (optional)");
                conditionHeader.style.fontSize = 10;
                conditionHeader.style.color = Color.gray;
                conditionContainer.Add(conditionHeader);

                var condNameField = new TextField("Value Name") { value = transition.transitionCondition?.ValueName ?? "" };
                condNameField.RegisterValueChangedCallback(evt =>
                {
                    if (transition.transitionCondition == null) transition.transitionCondition = new DialogTransitionCondition();
                    transition.transitionCondition.ValueName = evt.newValue;
                    EditorUtility.SetDirty(window.CurrentScene);
                });
                conditionContainer.Add(condNameField);

                var condCmdField = new TextField("Compare Command") { value = transition.transitionCondition?.CompareCommand ?? "" };
                condCmdField.RegisterValueChangedCallback(evt =>
                {
                    if (transition.transitionCondition == null) transition.transitionCondition = new DialogTransitionCondition();
                    transition.transitionCondition.CompareCommand = evt.newValue;
                    EditorUtility.SetDirty(window.CurrentScene);
                });
                conditionContainer.Add(condCmdField);

                var condFloatField = new FloatField("Float Value") { value = transition.transitionCondition?.ValueFl ?? 0f };
                condFloatField.RegisterValueChangedCallback(evt =>
                {
                    if (transition.transitionCondition == null) transition.transitionCondition = new DialogTransitionCondition();
                    transition.transitionCondition.ValueFl = evt.newValue;
                    EditorUtility.SetDirty(window.CurrentScene);
                });
                conditionContainer.Add(condFloatField);

                var condStrField = new TextField("String Value") { value = transition.transitionCondition?.ValueStr ?? "" };
                condStrField.RegisterValueChangedCallback(evt =>
                {
                    if (transition.transitionCondition == null) transition.transitionCondition = new DialogTransitionCondition();
                    transition.transitionCondition.ValueStr = evt.newValue;
                    EditorUtility.SetDirty(window.CurrentScene);
                });
                conditionContainer.Add(condStrField);

                container.Add(conditionContainer);

                // ---------- ACTIONS (Transition) ----------
                var transActionsLabel = new Label("Transition Actions (executed when taking this transition)");
                transActionsLabel.style.fontSize = 10;
                transActionsLabel.style.marginTop = 10;
                container.Add(transActionsLabel);
                CreateActionListUI(container, transition.dialogAction, () => EditorUtility.SetDirty(window.CurrentScene));

                // Delete transition button
                var deleteTransBtn = new Button(() =>
                {
                    window.CurrentScene.transitions.Remove(transition);
                    EditorUtility.SetDirty(window.CurrentScene);
                    Inspect(currentNode);
                    var fromView = graphView.GetNodeViews().FirstOrDefault(v => v.Node == currentNode);
                    var toView = graphView.GetNodeViews().FirstOrDefault(v => v.Node == targetNode);
                    if (fromView != null && toView != null)
                    {
                        var edge = graphView.edges.FirstOrDefault(e => e.output.node == fromView && e.input.node == toView);
                        if (edge != null) graphView.RemoveElement(edge);
                    }
                })
                { text = "Delete Transition" };
                container.Add(deleteTransBtn);

                scrollView.Add(container);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Вспомогательный метод для создания редактируемого списка DialogActionInfo
    // -------------------------------------------------------------------------
    private void CreateActionListUI(VisualElement parent, List<DialogActionInfo> actionList, Action onChanged)
    {
        var listView = new ListView(actionList);
        listView = new ListView(actionList)
        {
            makeItem = () =>
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.marginBottom = 5;

                var commandNameField = new TextField("Command") { style = { flexGrow = 1, marginRight = 5 } };
                var actionCommandField = new TextField("Action") { style = { flexGrow = 1, marginRight = 5 } };
                var floatField = new FloatField("Float") { style = { width = 80, marginRight = 5 } };
                var stringField = new TextField("String") { style = { flexGrow = 1, marginRight = 5 } };
                var removeBtn = new Button(() => { }) { text = "X", style = { width = 30 } };

                row.Add(commandNameField);
                row.Add(actionCommandField);
                row.Add(floatField);
                row.Add(stringField);
                row.Add(removeBtn);

                return row;
            },
            bindItem = (element, i) =>
            {
                var action = actionList[i];
                var fields = element.Children().OfType<TextField>().ToList();
                var floatField = element.Q<FloatField>();

                // Поля CommandName и ActionCommand
                if (fields.Count >= 2)
                {
                    fields[0].value = action.CommandName ?? "";
                    fields[1].value = action.ActionCommand ?? "";
                    fields[0].RegisterValueChangedCallback(evt => { action.CommandName = evt.newValue; onChanged?.Invoke(); });
                    fields[1].RegisterValueChangedCallback(evt => { action.ActionCommand = evt.newValue; onChanged?.Invoke(); });
                }
                // Float поле
                if (floatField != null)
                {
                    floatField.value = action.ValueFl;
                    floatField.RegisterValueChangedCallback(evt => { action.ValueFl = evt.newValue; onChanged?.Invoke(); });
                }
                // String поле (третье текстовое поле)
                if (fields.Count >= 3)
                {
                    fields[2].value = action.ValueStr ?? "";
                    fields[2].RegisterValueChangedCallback(evt => { action.ValueStr = evt.newValue; onChanged?.Invoke(); });
                }

                // Кнопка удаления
                var btn = element.Children().OfType<Button>().FirstOrDefault();
                if (btn != null)
                {
                    btn.clicked -= () => { };
                    btn.clicked += () =>
                    {
                        actionList.RemoveAt(i);
                        listView.Rebuild();
                        onChanged?.Invoke();
                    };
                }
            },
            itemsSource = actionList,
            fixedItemHeight = 30,
            reorderable = true
        };
        parent.Add(listView);

        var addButton = new Button(() =>
        {
            actionList.Add(new DialogActionInfo());
            listView.Rebuild();
            onChanged?.Invoke();
        })
        { text = "Add Action" };
        parent.Add(addButton);
    }

    public void Refresh()
    {
        if (currentNode != null)
            Inspect(currentNode);
        else
            ClearInspector();
    }

    public void ClearInspector()
    {
        scrollView.Clear();
        currentNode = null;
    }
}

public static class GraphViewExtensions
{
    public static IEnumerable<DialogNodeView> GetNodeViews(this DialogSceneGraphView graphView)
    {
        return graphView.nodes.OfType<DialogNodeView>();
    }
}