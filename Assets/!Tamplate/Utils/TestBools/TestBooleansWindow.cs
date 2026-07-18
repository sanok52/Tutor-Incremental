
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class TestBooleansWindow : EditorWindow
{
    private TestBooleansConfig config;
    private Vector2 scroll;

    [MenuItem("Tools/Test Booleans")]
    public static void Open()
    {
        GetWindow<TestBooleansWindow>("Test Booleans");
    }

    private void OnEnable()
    {
        config = Resources.Load<TestBooleansConfig>("TestBooleansConfig");
    }

    private void OnGUI()
    {
        if (config == null)
        {
            EditorGUILayout.HelpBox(
                "TestBooleansConfig not found.\nCreate one and place it in Resources folder.",
                MessageType.Error
            );

            if (GUILayout.Button("Create Config"))
            {
                CreateConfig();
            }
            return;
        }

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        config.IsDebug = EditorGUILayout.Toggle("Is Debug", config.IsDebug);
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(config);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Test Booleans", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; i < config.testBools.Count; i++)
        {
            var tb = config.testBools[i];

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            tb.ID = EditorGUILayout.TextField("TargetID", tb.ID);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                config.testBools.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            tb.testValue = EditorGUILayout.Toggle("Test StepsEnverAfter", tb.testValue);
            tb.releaseValue = EditorGUILayout.Toggle("Release StepsEnverAfter", tb.releaseValue);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Add TestBool"))
        {
            config.testBools.Add(new TestBool { ID = "new_flag" });
            EditorUtility.SetDirty(config);
        }
    }

    private void CreateConfig()
    {
        var asset = ScriptableObject.CreateInstance<TestBooleansConfig>();

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AssetDatabase.CreateAsset(asset, "Assets/Resources/TestBooleansConfig.asset");
        AssetDatabase.SaveAssets();

        config = asset;
        Selection.activeObject = asset;
    }
}
#endif