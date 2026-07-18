using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
    private LabyrinthEditorWindow window;
    private LabyrinthGraphView graphView;
    private LabyrinthNode currentNode;

    public InspectorView(LabyrinthEditorWindow window, LabyrinthGraphView graphView)
    {
        this.window = window;
        this.graphView = graphView;
    }

    public LabyrinthNode CurrentNode => currentNode;

    public void Inspect(LabyrinthNode node)
    {
        ClearInspector();
        currentNode = node;
        if (node == null) return;

        var scroll = new ScrollView();
        var data = window.CurrentData;
        if (data?.Data == null) return;
        float scale = data.Data.editorScale;

        // --- IDSpiker ---
        var idField = new TextField("IDSpiker") { value = node.ID };
        idField.RegisterValueChangedCallback(evt =>
        {
            if (currentNode == null) return;
            string oldId = currentNode.ID;
            currentNode.ID = evt.newValue;
            graphView.RenameNode(oldId, evt.newValue);
        });
        scroll.Add(idField);

        // --- Is Enter Node ---
        var enterToggle = new Toggle("Is Enter Node") { value = node.isEnterNode };
        enterToggle.RegisterValueChangedCallback(evt =>
        {
            if (currentNode == null) return;
            currentNode.isEnterNode = evt.newValue;
            window.Save();
            Refresh();
        });
        scroll.Add(enterToggle);

        // --- Logical position ---
        var posX = new IntegerField("Position X") { value = node.position.x };
        posX.RegisterValueChangedCallback(evt =>
        {
            if (currentNode == null) return;
            currentNode.position.x = evt.newValue;
            currentNode.editorPosition = new Vector2(currentNode.position.x, currentNode.position.y) / scale;
            window.Save();
            graphView.UpdateNodePosition(currentNode, currentNode.editorPosition);
            Refresh();
        });
        var posY = new IntegerField("Position Y") { value = node.position.y };
        posY.RegisterValueChangedCallback(evt =>
        {
            if (currentNode == null) return;
            currentNode.position.y = evt.newValue;
            currentNode.editorPosition = new Vector2(currentNode.position.x, currentNode.position.y) / scale;
            window.Save();
            graphView.UpdateNodePosition(currentNode, currentNode.editorPosition);
            Refresh();
        });
        scroll.Add(posX);
        scroll.Add(posY);

        // --- Editor position ---
        var edX = new FloatField("Editor X") { value = node.editorPosition.x };
        edX.RegisterValueChangedCallback(evt =>
        {
            if (currentNode == null) return;
            currentNode.editorPosition.x = evt.newValue;
            currentNode.position = Vector2Int.RoundToInt(currentNode.editorPosition * scale);
            window.Save();
            graphView.UpdateNodePosition(currentNode, currentNode.editorPosition);
            Refresh();
        });
        var edY = new FloatField("Editor Y") { value = node.editorPosition.y };
        edY.RegisterValueChangedCallback(evt =>
        {
            if (currentNode == null) return;
            currentNode.editorPosition.y = evt.newValue;
            currentNode.position = Vector2Int.RoundToInt(currentNode.editorPosition * scale);
            window.Save();
            graphView.UpdateNodePosition(currentNode, currentNode.editorPosition);
            Refresh();
        });
        scroll.Add(edX);
        scroll.Add(edY);

        // --- Global scale ---
        var scaleField = new FloatField("Scale") { value = scale };
        scaleField.RegisterValueChangedCallback(evt =>
        {
            if (window.CurrentData == null) return;
            window.CurrentData.Data.editorScale = evt.newValue;
            foreach (var n in window.CurrentData.Data.labyrinthNodes)
            {
                n.editorPosition = new Vector2(n.position.x, n.position.y) / evt.newValue;
                graphView.UpdateNodePosition(n, n.editorPosition);
            }
            window.Save();
            Refresh();
        });
        scroll.Add(scaleField);

        // --- Автоматические поля для всех остальных public полей LabyrinthNode ---
        var fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            // Пропускаем поля, которые уже обработаны вручную
            if (field.Name == "IDSpiker" || field.Name == "position" || field.Name == "editorPosition" ||
                field.Name == "isEnterNode" || field.Name == "Behaviours")
                continue;

            CreateFieldForFieldInfo(scroll, field, node);
        }

        // --- Массив Behaviours (ручная кастомная обработка) ---
        var behavioursLabel = new Label("Behaviours (LNodeBehContainer[])");
        behavioursLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        behavioursLabel.style.marginTop = 10;
        scroll.Add(behavioursLabel);

        var behaviourList = new List<LNodeBehContainer>(node.Behaviours ?? Array.Empty<LNodeBehContainer>());
        ListView listView = null;
        listView = new ListView(behaviourList)
        {
            makeItem = () =>
            {
                var objField = new ObjectField("Behaviour")
                {
                    objectType = typeof(LNodeBehContainer),
                    allowSceneObjects = false
                };
                objField.style.flexGrow = 1;
                var removeBtn = new Button(() => { }) { text = "X" };
                removeBtn.style.width = 30;
                var container = new VisualElement();
                container.style.flexDirection = FlexDirection.Row;
                container.Add(objField);
                container.Add(removeBtn);
                return container;
            },
            bindItem = (element, i) =>
            {
                var container = element;
                var objField = container.Q<ObjectField>();
                var removeBtn = container.Q<Button>();
                objField.SetValueWithoutNotify(behaviourList[i]);
                objField.RegisterValueChangedCallback(evt =>
                {
                    behaviourList[i] = evt.newValue as LNodeBehContainer;
                    node.Behaviours = behaviourList.ToArray();
                    window.Save();
                });
                removeBtn.clicked -= () => { };
                removeBtn.clicked += () =>
                {
                    behaviourList.RemoveAt(i);
                    listView?.Rebuild();
                    node.Behaviours = behaviourList.ToArray();
                    window.Save();
                };
            },
            itemsSource = behaviourList,
            fixedItemHeight = 30,
            reorderable = true
        };
        scroll.Add(listView);

        var addButton = new Button(() =>
        {
            behaviourList.Add(null);
            listView.Rebuild();
            node.Behaviours = behaviourList.ToArray();
            window.Save();
        })
        { text = "Add Behaviour" };
        scroll.Add(addButton);

        // --- Предпросмотр лабиринта ---
        var previewLabel = new Label("Labyrinth Preview (based on positions)");
        previewLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        previewLabel.style.marginTop = 10;
        scroll.Add(previewLabel);
        var previewContainer = new IMGUIContainer(() => DrawPreview(data));
        previewContainer.style.height = 200;
        previewContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        previewContainer.style.marginTop = 5;
        previewContainer.style.marginBottom = 10;
        scroll.Add(previewContainer);

        Add(scroll);
    }

    // Создаёт UI-элемент для редактирования поля произвольного типа
    private void CreateFieldForFieldInfo(ScrollView scroll, FieldInfo field, LabyrinthNode targetNode)
    {
        Type fieldType = field.FieldType;
        object currentValue = field.GetValue(targetNode);

        // string
        if (fieldType == typeof(string))
        {
            var textField = new TextField(field.Name) { value = (string)currentValue };
            textField.RegisterValueChangedCallback(evt =>
            {
                field.SetValue(targetNode, evt.newValue);
                window.Save();
            });
            scroll.Add(textField);
        }
        // int
        else if (fieldType == typeof(int))
        {
            var intField = new IntegerField(field.Name) { value = (int)currentValue };
            intField.RegisterValueChangedCallback(evt =>
            {
                field.SetValue(targetNode, evt.newValue);
                window.Save();
            });
            scroll.Add(intField);
        }
        // float
        else if (fieldType == typeof(float))
        {
            var floatField = new FloatField(field.Name) { value = (float)currentValue };
            floatField.RegisterValueChangedCallback(evt =>
            {
                field.SetValue(targetNode, evt.newValue);
                window.Save();
            });
            scroll.Add(floatField);
        }
        // bool
        else if (fieldType == typeof(bool))
        {
            var toggle = new Toggle(field.Name) { value = (bool)currentValue };
            toggle.RegisterValueChangedCallback(evt =>
            {
                field.SetValue(targetNode, evt.newValue);
                window.Save();
            });
            scroll.Add(toggle);
        }
        // Vector2
        else if (fieldType == typeof(Vector2))
        {
            var vecField = new Vector2Field(field.Name) { value = (Vector2)currentValue };
            vecField.RegisterValueChangedCallback(evt =>
            {
                field.SetValue(targetNode, evt.newValue);
                window.Save();
            });
            scroll.Add(vecField);
        }
        // Vector2Int
        else if (fieldType == typeof(Vector2Int))
        {
            var vecField = new Vector2IntField(field.Name) { value = (Vector2Int)currentValue };
            vecField.RegisterValueChangedCallback(evt =>
            {
                field.SetValue(targetNode, evt.newValue);
                window.Save();
            });
            scroll.Add(vecField);
        }
        // Vector3
        else if (fieldType == typeof(Vector3))
        {
            var vecField = new Vector3Field(field.Name) { value = (Vector3)currentValue };
            vecField.RegisterValueChangedCallback(evt =>
            {
                field.SetValue(targetNode, evt.newValue);
                window.Save();
            });
            scroll.Add(vecField);
        }
        // Color
        else if (fieldType == typeof(Color))
        {
            var colorField = new ColorField(field.Name) { value = (Color)currentValue };
            colorField.RegisterValueChangedCallback(evt =>
            {
                field.SetValue(targetNode, evt.newValue);
                window.Save();
            });
            scroll.Add(colorField);
        }
        // Enum
        else if (fieldType.IsEnum)
        {
            var enumField = new EnumField(field.Name, (Enum)currentValue);
            enumField.RegisterValueChangedCallback(evt =>
            {
                field.SetValue(targetNode, evt.newValue);
                window.Save();
            });
            scroll.Add(enumField);
        }
        // Для всех остальных типов – только чтение
        else
        {
            var label = new Label($"{field.Name} ({fieldType.Name}): {currentValue?.ToString() ?? "null"}");
            scroll.Add(label);
        }
    }

    private void DrawPreview(LabyrinthDataSO data)
    {
        if (data?.Data?.labyrinthNodes == null || data.Data.labyrinthNodes.Length == 0)
        {
            GUILayout.Label("No nodes");
            return;
        }

        var nodes = data.Data.labyrinthNodes;
        var transitions = data.Data.labyrinthTransitions;

        Vector2Int min = nodes[0].position;
        Vector2Int max = nodes[0].position;
        foreach (var n in nodes)
        {
            min.x = Mathf.Min(min.x, n.position.x);
            min.y = Mathf.Min(min.y, n.position.y);
            max.x = Mathf.Max(max.x, n.position.x);
            max.y = Mathf.Max(max.y, n.position.y);
        }
        Vector2Int size = new Vector2Int(max.x - min.x + 1, max.y - min.y + 1);
        if (size.x <= 0 || size.y <= 0) return;

        Rect previewRect = GUILayoutUtility.GetRect(150, 150, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        float cellW = previewRect.width / size.x;
        float cellH = previewRect.height / size.y;
        float cellSize = Mathf.Min(cellW, cellH);
        float startX = previewRect.x + (previewRect.width - size.x * cellSize) / 2;
        float startY = previewRect.y + (previewRect.height - size.y * cellSize) / 2;

        Handles.BeginGUI();
        if (transitions != null)
        {
            foreach (var t in transitions)
            {
                var fromNode = nodes.FirstOrDefault(n => n.ID == t.NodeID1);
                var toNode = nodes.FirstOrDefault(n => n.ID == t.NodeID2);
                if (fromNode != null && toNode != null)
                {
                    Vector2 from = new Vector2(
                        startX + (fromNode.position.x - min.x) * cellSize + cellSize / 2,
                        startY + (fromNode.position.y - min.y) * cellSize + cellSize / 2
                    );
                    Vector2 to = new Vector2(
                        startX + (toNode.position.x - min.x) * cellSize + cellSize / 2,
                        startY + (toNode.position.y - min.y) * cellSize + cellSize / 2
                    );
                    Handles.DrawLine(from, to);
                }
            }
        }

        foreach (var node in nodes)
        {
            Vector2 center = new Vector2(
                startX + (node.position.x - min.x) * cellSize + cellSize / 2,
                startY + (node.position.y - min.y) * cellSize + cellSize / 2
            );
            float radius = cellSize * 0.35f;
            Color color = node.GetEditorColor();
            Handles.color = color;
            Handles.DrawSolidDisc(center, Vector3.forward, radius);
            Handles.color = Color.black;
            Handles.DrawWireDisc(center, Vector3.forward, radius);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = Mathf.RoundToInt(cellSize * 0.3f);
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(center.x - cellSize / 2, center.y - cellSize / 2, cellSize, cellSize), node.ID, style);
        }
        Handles.EndGUI();
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
        Clear();
        currentNode = null;
    }
}