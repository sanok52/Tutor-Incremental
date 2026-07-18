using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class EnumEditorWindow : EditorWindow
{
    private EnumData data;
    private string enumFolderPath = "Assets/Scripts/GeneratedEnums";
    private Vector2 scrollPos;

    [MenuItem("Tools/Enum Editor")]
    public static void ShowWindow()
    {
        GetWindow<EnumEditorWindow>("Enum Editor");
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void LoadData()
    {
        string path = "Assets/EnumData.asset";
        data = AssetDatabase.LoadAssetAtPath<EnumData>(path);
        if (data == null)
        {
            data = CreateInstance<EnumData>();
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
        }
        if (!string.IsNullOrEmpty(data.folderPath))
            enumFolderPath = data.folderPath;
    }

    private void SaveData()
    {
        data.folderPath = enumFolderPath;
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }

    private void OnGUI()
    {
        // ---------- Настройки ----------
        EditorGUILayout.LabelField("Enum Generation Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Folder:", GUILayout.Width(100));
        enumFolderPath = EditorGUILayout.TextField(enumFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                {
                    enumFolderPath = "Assets" + selected.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Folder must be inside Assets folder.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Save All Enums", GUILayout.Height(30)))
        {
            GenerateAllEnums();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Enums", EditorStyles.boldLabel);

        // ---------- Список enum-ов ----------
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < data.enums.Count; i++)
        {
            var enumDef = data.enums[i];
            EditorGUILayout.BeginVertical("box");

            // Заголовок: имя + кнопка удалить
            EditorGUILayout.BeginHorizontal();
            string newName = EditorGUILayout.TextField(enumDef.name);
            if (newName != enumDef.name)
            {
                if (IsValidEnumName(newName) && !IsDuplicateEnumName(newName, i))
                {
                    enumDef.name = newName;
                    SaveData();
                }
                else
                {
                    EditorGUILayout.HelpBox("Invalid name or duplicate.", MessageType.Error);
                }
            }
            if (GUILayout.Button("Remove Enum", GUILayout.Width(100)))
            {
                data.enums.RemoveAt(i);
                SaveData();
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();

            // Список значений
            EditorGUILayout.LabelField("Values:", EditorStyles.miniLabel);
            for (int j = 0; j < enumDef.values.Count; j++)
            {
                EditorGUILayout.BeginHorizontal();
                string val = EditorGUILayout.TextField(enumDef.values[j]);
                if (val != enumDef.values[j])
                {
                    if (IsValidEnumValue(val, enumDef, j))
                    {
                        enumDef.values[j] = val;
                        SaveData();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Invalid value", MessageType.Error);
                    }
                }
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    enumDef.values.RemoveAt(j);
                    SaveData();
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Value"))
            {
                enumDef.values.Add("NewValue" + (enumDef.values.Count + 1));
                SaveData();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add New Enum"))
        {
            data.enums.Add(new EnumDefinition { name = "NewEnum" + (data.enums.Count + 1), values = new List<string> { "Value1", "Value2" } });
            SaveData();
        }

        EditorGUILayout.EndScrollView();

        if (GUI.changed)
            SaveData();
    }

    // ------------------ Валидация ------------------
    private bool IsValidEnumName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (!char.IsLetter(name[0]) && name[0] != '_') return false;
        foreach (char c in name)
            if (!char.IsLetterOrDigit(c) && c != '_') return false;
        return true;
    }

    private bool IsDuplicateEnumName(string name, int currentIndex)
    {
        for (int i = 0; i < data.enums.Count; i++)
            if (i != currentIndex && data.enums[i].name == name) return true;
        return false;
    }

    private bool IsValidEnumValue(string value, EnumDefinition enumDef, int currentIndex)
    {
        if (string.IsNullOrEmpty(value)) return false;
        if (!char.IsLetter(value[0]) && value[0] != '_') return false;
        foreach (char c in value)
            if (!char.IsLetterOrDigit(c) && c != '_') return false;
        for (int i = 0; i < enumDef.values.Count; i++)
            if (i != currentIndex && enumDef.values[i] == value) return false;
        return true;
    }

    // ------------------ Генерация файлов ------------------
    private void GenerateAllEnums()
    {
        // Проверяем / создаём папку
        if (!AssetDatabase.IsValidFolder(enumFolderPath))
        {
            string parent = Path.GetDirectoryName(enumFolderPath);
            if (string.IsNullOrEmpty(parent) || !AssetDatabase.IsValidFolder(parent))
            {
                EditorUtility.DisplayDialog("Error", "Invalid parent folder: " + parent, "OK");
                return;
            }
            string folder = Path.GetFileName(enumFolderPath);
            AssetDatabase.CreateFolder(parent, folder);
        }

        // Подтверждение удаления старых файлов
        if (!EditorUtility.DisplayDialog("Confirm", "This will delete all .cs files in folder: " + enumFolderPath + ". Continue?", "Yes", "No"))
            return;

        // Удаляем все .cs файлы в папке
        string fullPath = Path.Combine(Application.dataPath, enumFolderPath.Substring("Assets/".Length));
        foreach (string file in Directory.GetFiles(fullPath, "*.cs"))
            File.Delete(file);
        AssetDatabase.Refresh();

        // Генерируем новые
        foreach (var enumDef in data.enums)
        {
            if (!IsValidEnumName(enumDef.name)) continue;
            string filePath = Path.Combine(fullPath, enumDef.name + ".cs");
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("public enum " + enumDef.name);
                writer.WriteLine("{");
                for (int i = 0; i < enumDef.values.Count; i++)
                {
                    string val = enumDef.values[i];
                    if (!string.IsNullOrEmpty(val))
                    {
                        writer.Write("    " + val);
                        writer.WriteLine(i < enumDef.values.Count - 1 ? "," : "");
                    }
                }
                writer.WriteLine("}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Enums generated successfully!", "OK");
    }
}