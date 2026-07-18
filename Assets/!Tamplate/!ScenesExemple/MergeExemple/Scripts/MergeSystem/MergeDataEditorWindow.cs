#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

// ================== Recipe<T> ДОЛЖЕН ИСПОЛЬЗОВАТЬ SpecialResultGroup<T> ==================
// Убедитесь, что ваш Recipe<T> выглядит так:
/*
[Serializable]
public class Recipe<E> where E : Enum
{
    public E InputA;
    public E InputB;
    public E SimpleResult;
    public List<MergeBehaviour<E>> Behaviours = new List<MergeBehaviour<E>>();
    public List<SpecialResultGroup<E>> SpecialResults = new List<SpecialResultGroup<E>>();
    
    public E GetResult(...) { ... }
}
*/

public class MergeDataEditorWindow : EditorWindow
{
    private ScriptableObject _target;
    private SerializedObject _serializedTarget;
    private MergeGraphView _graphView;
    private Editor _selectedEditor;
    private VisualElement _inspectorContainer;

    [MenuItem("Window/Merge System Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<MergeDataEditorWindow>("Merge Editor");
        window.minSize = new Vector2(1200, 800);
    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
        OnSelectionChanged();
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        DestroyImmediate(_selectedEditor);
    }

    private void OnSelectionChanged()
    {
        if (Selection.activeObject is ScriptableObject so && IsMergeData(so))
        {
            _target = so;
            _serializedTarget = new SerializedObject(_target);
            BuildUI();
        }
        else
        {
            _target = null;
            _serializedTarget = null;
            rootVisualElement.Clear();
        }
    }

    // Теперь ищем MergeData<,,,> (4 generic параметра)
    private bool IsMergeData(ScriptableObject obj)
    {
        Type t = obj.GetType();
        while (t != null)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(MergeData<,,,>))
                return true;
            t = t.BaseType;
        }
        return false;
    }

    // Извлекаем первый generic-аргумент (перечисление E)
    private Type GetEnumType(ScriptableObject obj)
    {
        Type t = obj.GetType();
        while (t != null)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(MergeData<,,,>))
                return t.GetGenericArguments()[0];
            t = t.BaseType;
        }
        return null;
    }

    private void BuildUI()
    {
        rootVisualElement.Clear();
        if (_target == null) return;

        var split = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        split.Add(BuildIngredientPanel());
        _graphView = new MergeGraphView(_serializedTarget, this);
        split.Add(_graphView);
        _inspectorContainer = new VisualElement();
        _inspectorContainer.style.minWidth = 280;
        _inspectorContainer.Add(new Label("Инспектор"));
        split.Add(_inspectorContainer);
        rootVisualElement.Add(split);
        rootVisualElement.Add(BuildBottomPanel());
    }

    private VisualElement BuildIngredientPanel()
    {
        var container = new VisualElement();
        container.Add(new Label("Ингредиенты") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
        var scrollView = new ScrollView();
        SerializedProperty ingredientsProp = _serializedTarget.FindProperty("AllIngredients");
        for (int i = 0; i < ingredientsProp.arraySize; i++)
        {
            int index = i;
            var elem = ingredientsProp.GetArrayElementAtIndex(i);
            string title = elem.FindPropertyRelative("Title").stringValue;
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var btn = new Button(() => ShowInspectorForIngredient(index)) { text = title };
            btn.style.flexGrow = 1;
            row.Add(btn);
            var delBtn = new Button(() => RemoveIngredient(index)) { text = "X" };
            row.Add(delBtn);
            scrollView.Add(row);
        }
        var addBtn = new Button(AddIngredient) { text = "+ Добавить ингредиент" };
        scrollView.Add(addBtn);
        container.Add(scrollView);
        return container;
    }

    private VisualElement BuildBottomPanel()
    {
        var panel = new VisualElement { style = { flexDirection = FlexDirection.Row } };
        panel.Add(new Button(AddRecipe) { text = "Создать ноду слияния" });
        panel.Add(new Button(SaveAll) { text = "Сохранить" });
        return panel;
    }

    private void AddIngredient()
    {
        SerializedProperty ingredientsProp = _serializedTarget.FindProperty("AllIngredients");
        int newIndex = ingredientsProp.arraySize;
        ingredientsProp.arraySize++;
        var newElem = ingredientsProp.GetArrayElementAtIndex(newIndex);
        newElem.FindPropertyRelative("IDSpiker").stringValue = "ing_" + Guid.NewGuid().ToString().Substring(0, 6);
        newElem.FindPropertyRelative("Type").enumValueIndex = 0;
        newElem.FindPropertyRelative("Title").stringValue = "New Ingredient";
        _serializedTarget.ApplyModifiedProperties();
        _graphView.AddIngredientNode(newIndex);
        EditorUtility.SetDirty(_target);
    }

    private void RemoveIngredient(int index)
    {
        _serializedTarget.FindProperty("AllIngredients").DeleteArrayElementAtIndex(index);
        _serializedTarget.ApplyModifiedProperties();
        _graphView.RemoveIngredientNode(index);
        EditorUtility.SetDirty(_target);
    }

    private void AddRecipe()
    {
        SerializedProperty recipesProp = _serializedTarget.FindProperty("AllRecipes");
        int newIndex = recipesProp.arraySize;
        recipesProp.arraySize++;
        _serializedTarget.ApplyModifiedProperties();
        _graphView.AddRecipeNode(newIndex);
        EditorUtility.SetDirty(_target);
    }

    private void SaveAll()
    {
        if (_target == null) return;
        _graphView.ApplyGraphToTarget();
        EditorUtility.SetDirty(_target);
        AssetDatabase.SaveAssets();
        Debug.Log("Сохранено.");
    }

    public void ShowInspectorForIngredient(int index)
    {
        var proxy = CreateInstance<GenericInspectorProxy>();
        proxy.Initialize(_serializedTarget, "AllIngredients", index);
        ShowInspector(proxy);
    }

    public void ShowInspectorForRecipe(int index)
    {
        var proxy = CreateInstance<GenericInspectorProxy>();
        proxy.Initialize(_serializedTarget, "AllRecipes", index);
        ShowInspector(proxy);
    }

    private void ShowInspector(ScriptableObject obj)
    {
        DestroyImmediate(_selectedEditor);
        _inspectorContainer.Clear();
        _inspectorContainer.Add(new Label("Инспектор") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
        _selectedEditor = Editor.CreateEditor(obj);
        var imguiContainer = new IMGUIContainer(() =>
        {
            if (_selectedEditor?.target != null)
                _selectedEditor.OnInspectorGUI();
        });
        _inspectorContainer.Add(imguiContainer);

        var applyBtn = new Button(() =>
        {
            if (_selectedEditor?.target is GenericInspectorProxy proxy)
            {
                proxy.TargetSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(_target);
            }
            UpdateAllNodeTitles();
        });
        applyBtn.text = "Применить изменения";
        _inspectorContainer.Add(applyBtn);
    }

    public void UpdateAllNodeTitles()
    {
        if (_graphView == null) return;
        foreach (var node in _graphView.nodes)
        {
            if (node is IngredientNode ingNode) ingNode.UpdateTitle();
            else if (node is RecipeNode recNode)
            {
                recNode.SyncSpecialResultsWithBehaviours();
                recNode.RefreshBehaviourPorts();
                recNode.UpdateTitle();
            }
        }
    }
}

// ================== GRAPHVIEW ==================
public class MergeGraphView : GraphView
{
    private SerializedObject _serializedTarget;
    private MergeDataEditorWindow _window;
    private List<IngredientNode> _ingredientNodes = new List<IngredientNode>();
    private List<RecipeNode> _recipeNodes = new List<RecipeNode>();

    public MergeGraphView(SerializedObject serializedTarget, MergeDataEditorWindow window)
    {
        _serializedTarget = serializedTarget;
        _window = window;

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        style.flexGrow = 1;

        // Вместо canDeleteSelection используем RegisterCallback для клавиш
        RegisterCallback<KeyDownEvent>(OnKeyDown);

        graphViewChanged += OnGraphChanged;
        BuildGraph();
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
        {
            DeleteSelectedNodes();
            evt.StopPropagation();
        }
    }

    private void DeleteSelectedNodes()
    {
        var selectedNodes = selection.OfType<Node>().ToList();
        foreach (var node in selectedNodes)
        {
            if (node is IngredientNode ingNode)
            {
                var ingredientsProp = _serializedTarget.FindProperty("AllIngredients");
                if (ingredientsProp != null && ingNode.IngredientIndex < ingredientsProp.arraySize)
                {
                    ingredientsProp.DeleteArrayElementAtIndex(ingNode.IngredientIndex);
                    _serializedTarget.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_serializedTarget.targetObject);
                }
                RemoveIngredientNode(ingNode.IngredientIndex);
            }
            else if (node is RecipeNode recNode)
            {
                var recipesProp = _serializedTarget.FindProperty("AllRecipes");
                if (recipesProp != null && recNode.RecipeIndex < recipesProp.arraySize)
                {
                    recipesProp.DeleteArrayElementAtIndex(recNode.RecipeIndex);
                    _serializedTarget.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_serializedTarget.targetObject);
                }
                RemoveRecipeNode(recNode.RecipeIndex);
            }
        }
    }

    private void OnDeleteSelection(string operationName, AskUser askUser)
    {
        var selectedNodes = selection.OfType<Node>().ToList();
        foreach (var node in selectedNodes)
        {
            if (node is IngredientNode ingNode)
            {
                // Удаляем ингредиент из данных
                var ingredientsProp = _serializedTarget.FindProperty("AllIngredients");
                if (ingredientsProp != null && ingNode.IngredientIndex < ingredientsProp.arraySize)
                {
                    ingredientsProp.DeleteArrayElementAtIndex(ingNode.IngredientIndex);
                    _serializedTarget.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_serializedTarget.targetObject);
                }
                RemoveIngredientNode(ingNode.IngredientIndex);
            }
            else if (node is RecipeNode recNode)
            {
                // Удаляем рецепт из данных
                var recipesProp = _serializedTarget.FindProperty("AllRecipes");
                if (recipesProp != null && recNode.RecipeIndex < recipesProp.arraySize)
                {
                    recipesProp.DeleteArrayElementAtIndex(recNode.RecipeIndex);
                    _serializedTarget.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_serializedTarget.targetObject);
                }
                RemoveRecipeNode(recNode.RecipeIndex);
            }
        }
    }

    private void RemoveRecipeNode(int index)
    {
        var node = _recipeNodes.FirstOrDefault(n => n.RecipeIndex == index);
        if (node == null) return;
        foreach (var edge in edges.ToList())
            if (edge.output.node == node || edge.input.node == node)
                RemoveElement(edge);
        RemoveElement(node);
        _recipeNodes.Remove(node);
        for (int i = 0; i < _recipeNodes.Count; i++)
            if (_recipeNodes[i].RecipeIndex > index) _recipeNodes[i].RecipeIndex--;
    }

    private GraphViewChange OnGraphChanged(GraphViewChange change)
    {
        if (change.edgesToCreate != null)
        {
            foreach (var edge in change.edgesToCreate)
            {
                if (edge.input.node is RecipeNode rIn) rIn.UpdateTitle();
                if (edge.output.node is RecipeNode rOut) rOut.UpdateTitle();
            }
        }
        if (change.elementsToRemove != null)
        {
            foreach (var edge in change.elementsToRemove.OfType<Edge>())
            {
                if (edge.input.node is RecipeNode rIn) rIn.UpdateTitle();
                if (edge.output.node is RecipeNode rOut) rOut.UpdateTitle();
            }
        }
        return change;
    }

    private void BuildGraph()
    {
        _serializedTarget.Update();
        SerializedProperty ingredientsProp = _serializedTarget.FindProperty("AllIngredients");
        SerializedProperty recipesProp = _serializedTarget.FindProperty("AllRecipes");

        var savedIngPos = GetSavedPositions("_editorIngredientPositions", ingredientsProp.arraySize);
        var savedRecPos = GetSavedPositions("_editorRecipePositions", recipesProp.arraySize);

        // Создаём ноды ингредиентов
        for (int i = 0; i < ingredientsProp.arraySize; i++)
        {
            var node = new IngredientNode(i, _serializedTarget, _window);
            if (i < savedIngPos.Count) node.SetPosition(new Rect(savedIngPos[i], node.GetPosition().size));
            else node.SetPosition(new Rect(100, 100 + i * 150, 150, 80));
            AddElement(node);
            _ingredientNodes.Add(node);
        }

        // Создаём ноды рецептов
        for (int i = 0; i < recipesProp.arraySize; i++)
        {
            var recipeProp = recipesProp.GetArrayElementAtIndex(i);
            int inputA = GetIngredientIndexByType(recipeProp.FindPropertyRelative("InputA").enumValueIndex);
            int inputB = GetIngredientIndexByType(recipeProp.FindPropertyRelative("InputB").enumValueIndex);
            var node = new RecipeNode(i, _serializedTarget, _window);
            if (i < savedRecPos.Count) node.SetPosition(new Rect(savedRecPos[i], node.GetPosition().size));
            else node.SetPosition(new Rect(500, 100 + i * 200, 180, 100));
            AddElement(node);
            _recipeNodes.Add(node);

            // Подключаем входы
            if (inputA >= 0 && inputA < _ingredientNodes.Count)
                AddElement(_ingredientNodes[inputA].Output.ConnectTo(node.InputA));
            if (inputB >= 0 && inputB < _ingredientNodes.Count)
                AddElement(_ingredientNodes[inputB].Output.ConnectTo(node.InputB));

            // Синхронизируем порты поведений
            node.SyncSpecialResultsWithBehaviours();
            node.RefreshBehaviourPorts();

            // SimpleResult
            int simpleIdx = recipeProp.FindPropertyRelative("SimpleResult").enumValueIndex;
            int resultIng = GetIngredientIndexByType(simpleIdx);
            if (resultIng >= 0 && resultIng < _ingredientNodes.Count)
                AddElement(node.Output.ConnectTo(_ingredientNodes[resultIng].Input));

            // SpecialResults — гарантированно выравниваем порты и восстанавливаем ВСЕ связи
            var specProp = recipeProp.FindPropertyRelative("SpecialResults");
            if (specProp != null)
            {
                for (int b = 0; b < specProp.arraySize; b++)
                {
                    // Добиваем списки BehaviourOutputs, если их меньше
                    while (node.BehaviourOutputs.Count <= b)
                        node.BehaviourOutputs.Add(new List<Port>());

                    var group = specProp.GetArrayElementAtIndex(b);
                    var results = group.FindPropertyRelative("Results");
                    var ports = node.BehaviourOutputs[b];

                    // Добавляем недостающие порты
                    while (ports.Count < results.arraySize)
                    {
                        var newPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output,
                            Port.Capacity.Single, typeof(bool));
                        newPort.portName = $"Spec {b}.{ports.Count}";
                        node.outputContainer.Add(newPort);
                        ports.Add(newPort);
                    }
                    // Удаляем лишние порты
                    while (ports.Count > results.arraySize)
                    {
                        var last = ports[ports.Count - 1];
                        node.outputContainer.Remove(last);
                        ports.RemoveAt(ports.Count - 1);
                    }

                    // Теперь восстанавливаем все соединения
                    for (int j = 0; j < results.arraySize; j++)
                    {
                        int val = results.GetArrayElementAtIndex(j).enumValueIndex;
                        int ingIdx = GetIngredientIndexByType(val);
                        if (ingIdx >= 0 && ingIdx < _ingredientNodes.Count)
                            AddElement(ports[j].ConnectTo(_ingredientNodes[ingIdx].Input));
                    }
                }
            }

            node.UpdateTitle();
        }
    }

    private List<Vector2> GetSavedPositions(string fieldName, int expectedCount)
    {
        var target = _serializedTarget?.targetObject;
        if (target == null) return new List<Vector2>();
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        return (field?.GetValue(target) as List<Vector2>) ?? new List<Vector2>();
    }

    private void SavePositions(string fieldName, List<Vector2> positions)
    {
        var target = _serializedTarget?.targetObject;
        if (target == null) return;
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        field?.SetValue(target, positions);
    }

    public void AddIngredientNode(int index)
    {
        var node = new IngredientNode(index, _serializedTarget, _window);
        node.SetPosition(new Rect(100, 100 + index * 150, 150, 80));
        AddElement(node);
        _ingredientNodes.Add(node);
    }

    public void RemoveIngredientNode(int index)
    {
        var node = _ingredientNodes.FirstOrDefault(n => n.IngredientIndex == index);
        if (node == null) return;
        foreach (var edge in edges.ToList())
            if (edge.output.node == node || edge.input.node == node)
                RemoveElement(edge);
        RemoveElement(node);
        _ingredientNodes.Remove(node);
        for (int i = 0; i < _ingredientNodes.Count; i++)
            if (_ingredientNodes[i].IngredientIndex > index) _ingredientNodes[i].IngredientIndex--;
        foreach (var rNode in _recipeNodes) rNode.UpdateTitle();
    }

    public void AddRecipeNode(int index)
    {
        var node = new RecipeNode(index, _serializedTarget, _window);
        node.SetPosition(new Rect(500, 100 + index * 200, 180, 100));
        AddElement(node);
        _recipeNodes.Add(node);
    }

    private int GetIngredientIndexByType(int enumValueIndex)
    {
        SerializedProperty ingredientsProp = _serializedTarget.FindProperty("AllIngredients");
        for (int i = 0; i < ingredientsProp.arraySize; i++)
            if (ingredientsProp.GetArrayElementAtIndex(i).FindPropertyRelative("Type").enumValueIndex == enumValueIndex)
                return i;
        return -1;
    }

    // ===== В MergeGraphView замените ApplyGraphToTarget на эту версию =====
    public void ApplyGraphToTarget()
    {
        if (_serializedTarget?.targetObject == null) return;
        _serializedTarget.Update();

        // 1. Синхронизируем структуру SpecialResults через SerializedProperty
        foreach (var rNode in _recipeNodes)
            rNode.SyncSpecialResultsWithBehaviours();

        // 2. Применяем изменения структуры, чтобы они попали в целевой объект
        _serializedTarget.ApplyModifiedProperties();

        // 3. Снова обновляем SerializedObject (теперь он видит новую структуру)
        _serializedTarget.Update();

        SerializedProperty recipesProp = _serializedTarget.FindProperty("AllRecipes");
        SerializedProperty ingredientsProp = _serializedTarget.FindProperty("AllIngredients");
        if (recipesProp == null || ingredientsProp == null) return;

        // 4. Записываем соединения
        foreach (var rNode in _recipeNodes)
        {
            if (rNode.RecipeIndex >= recipesProp.arraySize) continue;
            SerializedProperty recipeProp = recipesProp.GetArrayElementAtIndex(rNode.RecipeIndex);

            // InputA и InputB
            var edgeA = edges.ToList().FirstOrDefault(e => e.input == rNode.InputA);
            var edgeB = edges.ToList().FirstOrDefault(e => e.input == rNode.InputB);

            if (edgeA?.output.node is IngredientNode ingA && ingA.IngredientIndex >= 0 && ingA.IngredientIndex < ingredientsProp.arraySize)
                recipeProp.FindPropertyRelative("InputA").enumValueIndex =
                    ingredientsProp.GetArrayElementAtIndex(ingA.IngredientIndex).FindPropertyRelative("Type").enumValueIndex;

            if (edgeB?.output.node is IngredientNode ingB && ingB.IngredientIndex >= 0 && ingB.IngredientIndex < ingredientsProp.arraySize)
                recipeProp.FindPropertyRelative("InputB").enumValueIndex =
                    ingredientsProp.GetArrayElementAtIndex(ingB.IngredientIndex).FindPropertyRelative("Type").enumValueIndex;

            // SimpleResult
            var resEdge = edges.ToList().FirstOrDefault(e => e.output == rNode.Output && e.input.node is IngredientNode);
            if (resEdge?.input.node is IngredientNode resIng && resIng.IngredientIndex >= 0 && resIng.IngredientIndex < ingredientsProp.arraySize)
                recipeProp.FindPropertyRelative("SimpleResult").enumValueIndex =
                    ingredientsProp.GetArrayElementAtIndex(resIng.IngredientIndex).FindPropertyRelative("Type").enumValueIndex;
            else
                recipeProp.FindPropertyRelative("SimpleResult").enumValueIndex = 0;

            // SpecialResults
            SerializedProperty specialResultsProp = recipeProp.FindPropertyRelative("SpecialResults");
            if (specialResultsProp != null)
            {
                for (int i = 0; i < specialResultsProp.arraySize; i++)
                {
                    SerializedProperty groupProp = specialResultsProp.GetArrayElementAtIndex(i);
                    SerializedProperty resultsProp = groupProp.FindPropertyRelative("Results");
                    for (int j = 0; j < resultsProp.arraySize; j++)
                    {
                        if (i < rNode.BehaviourOutputs.Count && j < rNode.BehaviourOutputs[i].Count)
                        {
                            var port = rNode.BehaviourOutputs[i][j];
                            var edge = edges.ToList().FirstOrDefault(e => e.output == port && e.input.node is IngredientNode);
                            if (edge?.input.node is IngredientNode ing && ing.IngredientIndex >= 0 && ing.IngredientIndex < ingredientsProp.arraySize)
                            {
                                resultsProp.GetArrayElementAtIndex(j).enumValueIndex =
                                    ingredientsProp.GetArrayElementAtIndex(ing.IngredientIndex).FindPropertyRelative("Type").enumValueIndex;
                            }
                            // else – НЕ трогаем значение, оставляем то, что установлено в инспекторе
                        }
                    }
                }
            }
        }

        // Сохраняем позиции нод
        SavePositions("_editorIngredientPositions", _ingredientNodes.Select(n => n.GetPosition().position).ToList());
        SavePositions("_editorRecipePositions", _recipeNodes.Select(n => n.GetPosition().position).ToList());

        // 5. Финальное применение и пометка «грязным»
        _serializedTarget.ApplyModifiedProperties();
        EditorUtility.SetDirty(_serializedTarget.targetObject);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();
    }
}

// ================== НОДЫ ==================
public class IngredientNode : Node
{
    public int IngredientIndex;
    public Port Output, Input;
    private SerializedObject _serializedTarget;

    public IngredientNode(int index, SerializedObject serializedTarget, MergeDataEditorWindow window)
    {
        IngredientIndex = index;
        _serializedTarget = serializedTarget;
        title = GetTitle();

        Input = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        Input.portName = "In";
        inputContainer.Add(Input);

        Output = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        Output.portName = "Out";
        outputContainer.Add(Output);

        RegisterCallback<MouseDownEvent>(e => window.ShowInspectorForIngredient(index));
    }

    private string GetTitle()
    {
        var prop = _serializedTarget.FindProperty("AllIngredients");
        if (prop != null && IngredientIndex >= 0 && IngredientIndex < prop.arraySize)
        {
            var elem = prop.GetArrayElementAtIndex(IngredientIndex);
            string title = elem.FindPropertyRelative("Title").stringValue;
            return string.IsNullOrEmpty(title) ? elem.FindPropertyRelative("IDSpiker").stringValue : title;
        }
        return "???";
    }

    public void UpdateTitle() => title = GetTitle();
}

public class RecipeNode : Node
{
    public int RecipeIndex;
    public Port InputA, InputB, Output;
    public List<List<Port>> BehaviourOutputs = new List<List<Port>>();
    private SerializedObject _serializedTarget;
    private MergeDataEditorWindow _window;

    public RecipeNode(int index, SerializedObject serializedTarget, MergeDataEditorWindow window)
    {
        RecipeIndex = index;
        _serializedTarget = serializedTarget;
        _window = window;

        InputA = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        InputA.portName = "A";
        inputContainer.Add(InputA);

        InputB = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        InputB.portName = "B";
        inputContainer.Add(InputB);

        Output = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        Output.portName = "Result";
        outputContainer.Add(Output);

        RefreshBehaviourPorts();
        RegisterCallback<MouseDownEvent>(e => _window.ShowInspectorForRecipe(index));
        UpdateTitle();
    }

    public void SyncSpecialResultsWithBehaviours()
    {
        if (_serializedTarget == null) return;
        var recipesProp = _serializedTarget.FindProperty("AllRecipes");
        if (recipesProp == null || RecipeIndex < 0 || RecipeIndex >= recipesProp.arraySize) return;
        var recipeProp = recipesProp.GetArrayElementAtIndex(RecipeIndex);

        var behavioursProp = recipeProp.FindPropertyRelative("Behaviours");
        var specialResultsProp = recipeProp.FindPropertyRelative("SpecialResults");
        if (behavioursProp == null || specialResultsProp == null) return;

        // Удаляем/добавляем группы
        while (specialResultsProp.arraySize > behavioursProp.arraySize)
            specialResultsProp.DeleteArrayElementAtIndex(specialResultsProp.arraySize - 1);
        while (specialResultsProp.arraySize < behavioursProp.arraySize)
        {
            specialResultsProp.InsertArrayElementAtIndex(specialResultsProp.arraySize);
            var newGroup = specialResultsProp.GetArrayElementAtIndex(specialResultsProp.arraySize - 1);
            newGroup.FindPropertyRelative("Results").ClearArray();
        }

        // Подгоняем размеры Results
        for (int i = 0; i < behavioursProp.arraySize; i++)
        {
            var beh = behavioursProp.GetArrayElementAtIndex(i).objectReferenceValue;
            int required = 1;
            if (beh != null)
            {
               var method = beh.GetType().GetMethod("GetSpecialResultTitles",
    BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
if (method != null)
{
    var titles = (string[])method.Invoke(beh, null);
    Debug.Log($"Behaviour {beh.GetType().Name} titles: [{string.Join(", ", titles)}]");
    required = titles.Length;
}
else
{
    Debug.LogWarning($"Behaviour {beh.GetType()} does not have GetSpecialResultTitles method. Using default 1.");
}
            }
            var resultsProp = specialResultsProp.GetArrayElementAtIndex(i).FindPropertyRelative("Results");
            while (resultsProp.arraySize > required)
                resultsProp.DeleteArrayElementAtIndex(resultsProp.arraySize - 1);
            while (resultsProp.arraySize < required)
            {
                resultsProp.InsertArrayElementAtIndex(resultsProp.arraySize);
                // новый элемент по умолчанию 0
            }
        }
        // Не вызываем ApplyModifiedProperties здесь, это сделает вызывающий код
    }

    public void RefreshBehaviourPorts()
    {
        // Сохраняем старые связи
        var oldConnections = new Dictionary<(int, int), int>();
        var gv = GetFirstAncestorOfType<GraphView>();
        if (gv != null)
        {
            for (int bi = 0; bi < BehaviourOutputs.Count; bi++)
                for (int pi = 0; pi < BehaviourOutputs[bi].Count; pi++)
                {
                    var e = gv.edges.ToList().FirstOrDefault(ed => ed.output == BehaviourOutputs[bi][pi]);
                    if (e?.input.node is IngredientNode ing)
                        oldConnections[(bi, pi)] = ing.IngredientIndex;
                }
        }

        SyncSpecialResultsWithBehaviours();

        // Удаляем старые порты
        if (BehaviourOutputs != null)
        {
            foreach (var list in BehaviourOutputs)
                foreach (var p in list)
                    outputContainer.Remove(p);
        }
        BehaviourOutputs = new List<List<Port>>();

        var recipesProp = _serializedTarget.FindProperty("AllRecipes");
        if (recipesProp == null || RecipeIndex < 0 || RecipeIndex >= recipesProp.arraySize) return;
        var recipeProp = recipesProp.GetArrayElementAtIndex(RecipeIndex);
        var behavioursProp = recipeProp.FindPropertyRelative("Behaviours");
        var specialResultsProp = recipeProp.FindPropertyRelative("SpecialResults");
        if (behavioursProp == null || specialResultsProp == null) return;

        for (int i = 0; i < behavioursProp.arraySize; i++)
        {
            var behObj = behavioursProp.GetArrayElementAtIndex(i).objectReferenceValue;
            string[] titles = null;
            if (behObj != null)
            {
                var m = behObj.GetType().GetMethod("GetSpecialResultTitles");
                if (m != null) titles = (string[])m.Invoke(behObj, null);
            }
            titles ??= new[] { $"Spec {i}" };

            var ports = new List<Port>();
            for (int j = 0; j < titles.Length; j++)
            {
                var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                port.portName = titles[j];
                outputContainer.Add(port);
                ports.Add(port);
            }
            BehaviourOutputs.Add(ports);
        }

        // Восстанавливаем соединения
        if (gv != null)
        {
            foreach (var kv in oldConnections)
            {
                int bi = kv.Key.Item1, pi = kv.Key.Item2, ingIdx = kv.Value;
                if (bi < BehaviourOutputs.Count && pi < BehaviourOutputs[bi].Count)
                {
                    var ingNode = gv.nodes.ToList().OfType<IngredientNode>().FirstOrDefault(n => n.IngredientIndex == ingIdx);
                    if (ingNode != null)
                        gv.AddElement(BehaviourOutputs[bi][pi].ConnectTo(ingNode.Input));
                }
            }
        }

        RefreshExpandedState();
    }

    public void UpdateTitle()
    {
        string a = GetConnectedName(InputA);
        string b = GetConnectedName(InputB);
        title = $"{(string.IsNullOrEmpty(a) ? "?" : a)} + {(string.IsNullOrEmpty(b) ? "?" : b)}";
    }

    private string GetConnectedName(Port port)
    {
        var gv = GetFirstAncestorOfType<GraphView>();
        var edge = gv?.edges.ToList().FirstOrDefault(e => e.input == port);
        if (edge?.output.node is IngredientNode ing)
        {
            var prop = _serializedTarget.FindProperty("AllIngredients");
            if (prop != null && ing.IngredientIndex >= 0 && ing.IngredientIndex < prop.arraySize)
            {
                var elem = prop.GetArrayElementAtIndex(ing.IngredientIndex);
                string t = elem.FindPropertyRelative("Title").stringValue;
                return string.IsNullOrEmpty(t) ? elem.FindPropertyRelative("IDSpiker").stringValue : t;
            }
        }
        return null;
    }
}

// ================== ИНСПЕКТОР ==================
public class GenericInspectorProxy : ScriptableObject
{
    [NonSerialized] public SerializedObject TargetSO;
    [NonSerialized] public string ListPropertyName;
    [NonSerialized] public int Index;
    public void Initialize(SerializedObject so, string name, int idx)
    {
        TargetSO = so;
        ListPropertyName = name;
        Index = idx;
    }
}

[CustomEditor(typeof(GenericInspectorProxy))]
public class GenericInspectorProxyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var proxy = (GenericInspectorProxy)target;
        if (proxy?.TargetSO == null) return;
        proxy.TargetSO.Update();

        var listProp = proxy.TargetSO.FindProperty(proxy.ListPropertyName);
        if (listProp != null && proxy.Index >= 0 && proxy.Index < listProp.arraySize)
        {
            var element = listProp.GetArrayElementAtIndex(proxy.Index);
            // Отображаем все поля, кроме SpecialResults (если рецепт)
            var prop = element.Copy();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (proxy.ListPropertyName == "AllRecipes" && prop.name == "SpecialResults")
                        continue;
                    EditorGUILayout.PropertyField(prop, true);
                } while (prop.NextVisible(false));
            }

            // Явно отображаем SpecialResults для рецепта
            if (proxy.ListPropertyName == "AllRecipes")
            {
                var specProp = element.FindPropertyRelative("SpecialResults");
                if (specProp != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Special Results", EditorStyles.boldLabel);
                    for (int i = 0; i < specProp.arraySize; i++)
                    {
                        var group = specProp.GetArrayElementAtIndex(i);
                        var results = group.FindPropertyRelative("Results");
                        EditorGUILayout.LabelField($"Behaviour {i}:");
                        EditorGUI.indentLevel++;
                        for (int j = 0; j < results.arraySize; j++)
                            EditorGUILayout.PropertyField(results.GetArrayElementAtIndex(j), new GUIContent($"Result {j}"));
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }
        proxy.TargetSO.ApplyModifiedProperties();
    }
}
#endif