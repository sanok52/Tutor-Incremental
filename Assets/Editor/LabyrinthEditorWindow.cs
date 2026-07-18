using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class LabyrinthEditorWindow : EditorWindow
{
    private LabyrinthDataSO dataSO;
    private LabyrinthGraphView graphView;
    private InspectorView inspectorView;

    [MenuItem("Window/Labyrinth Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<LabyrinthEditorWindow>();
        window.titleContent = new GUIContent("Labyrinth Editor");
        window.Show();
    }

    public void CreateGUI()
    {
        rootVisualElement.style.flexDirection = FlexDirection.Row;

        var leftPanel = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };

        var toolbar = new Toolbar();
        var objectField = new ObjectField("Labyrinth Data")
        {
            objectType = typeof(LabyrinthDataSO),
            allowSceneObjects = false
        };
        objectField.RegisterValueChangedCallback(evt =>
        {
            dataSO = evt.newValue as LabyrinthDataSO;
            graphView?.LoadData(dataSO);
            inspectorView?.ClearInspector();
        });
        toolbar.Add(objectField);

        var createButton = new Button(() =>
        {
            if (dataSO == null) Debug.LogWarning("Select LabyrinthDataSO first");
            else graphView.CreateNode();
        });
        createButton.text = "Create Node";
        toolbar.Add(createButton);

        leftPanel.Add(toolbar);
        graphView = new LabyrinthGraphView(this);
        graphView.style.flexGrow = 1;
        leftPanel.Add(graphView);

        inspectorView = new InspectorView(this, graphView);
        inspectorView.style.width = 300;
        inspectorView.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        inspectorView.style.marginLeft = 5;
        inspectorView.style.paddingTop = 30;

        rootVisualElement.Add(leftPanel);
        rootVisualElement.Add(inspectorView);

        if (dataSO != null) graphView.LoadData(dataSO);
    }

    public void Save()
    {
        if (dataSO == null) return;
        EditorUtility.SetDirty(dataSO);
        AssetDatabase.SaveAssets();
    }

    public LabyrinthDataSO CurrentData => dataSO;
    public InspectorView Inspector => inspectorView;
}