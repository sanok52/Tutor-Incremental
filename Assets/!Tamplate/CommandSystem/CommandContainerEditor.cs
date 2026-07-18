#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(CommandContainer<>), true)]
public class CommandContainerEditor : Editor
{
    private SerializedProperty behavioursProp;
    private Type commandBaseType;        // CommandBehaviour<C>
    private List<Type> availableCommandTypes;
    private string[] typeNames;
    private int selectedTypeIndex = -1;

    private void OnEnable()
    {
        behavioursProp = serializedObject.FindProperty("behaviours");

        // Находим базовый тип CommandContainer<C> и извлекаем C
        var containerType = target.GetType();
        while (containerType != null && !(containerType.IsGenericType && containerType.GetGenericTypeDefinition() == typeof(CommandContainer<>)))
            containerType = containerType.BaseType;

        if (containerType != null)
        {
            var contextType = containerType.GetGenericArguments()[0];
            commandBaseType = typeof(CommandBehaviour<>).MakeGenericType(contextType);
        }

        RefreshAvailableTypes();
    }

    private void RefreshAvailableTypes()
    {
        if (commandBaseType == null) return;

        availableCommandTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && commandBaseType.IsAssignableFrom(t))
            .ToList();

        typeNames = availableCommandTypes.Select(t => t.Name).ToArray();
        selectedTypeIndex = -1;
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();

        // Рисуем все сериализованные поля, кроме behaviours (чтобы не дублировать)
        var property = serializedObject.GetIterator();
        property.NextVisible(true); // пропускаем скрытое поле m_Script
        while (property.NextVisible(false))
        {
            if (property.name == "behaviours") continue;
            EditorGUILayout.PropertyField(property, true);
        }

        // Заголовок с количеством команд
        EditorGUILayout.LabelField($"Commands in container: {behavioursProp.arraySize}", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;


        // Список существующих команд
        for (int i = 0; i < behavioursProp.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();
            var cmdProp = behavioursProp.GetArrayElementAtIndex(i);
            var cmd = cmdProp.objectReferenceValue as MonoBehaviour;

            // Отображаем имя типа (если команда существует)
            string typeName = cmd != null ? cmd.GetType().Name : "None (missing)";
            EditorGUILayout.LabelField(typeName, GUILayout.Width(150));

            // Поле для выбора/переназначения компонента
            EditorGUILayout.PropertyField(cmdProp, GUIContent.none, true);

            // Кнопка удаления
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                if (cmd != null)
                    DestroyImmediate(cmd);
                behavioursProp.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();
                break; // перерисовываем UI
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        // Кнопка Refresh – синхронизирует список с компонентами на объекте
        if (GUILayout.Button("Refresh from GameObject"))
        {
            RefreshCommandList();
        }

        EditorGUILayout.Space();

        // Выпадающий список для добавления новой команды
        if (availableCommandTypes.Count > 0)
        {
            selectedTypeIndex = EditorGUILayout.Popup("New commandExecuter type", selectedTypeIndex, typeNames);
            if (selectedTypeIndex >= 0 && GUILayout.Button("Add Command"))
            {
                var typeToAdd = availableCommandTypes[selectedTypeIndex];
                var container = target as MonoBehaviour;
                // Добавляем компонент на тот же GameObject
                var newCommand = container.gameObject.AddComponent(typeToAdd) as MonoBehaviour;
                if (newCommand != null && commandBaseType.IsAssignableFrom(newCommand.GetType()))
                {
                    behavioursProp.arraySize++;
                    behavioursProp.GetArrayElementAtIndex(behavioursProp.arraySize - 1).objectReferenceValue = newCommand;
                    serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    Debug.LogError($"Failed to add commandExecuter of type {typeToAdd.Name}");
                }
                selectedTypeIndex = -1;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No CommandBehaviour<> types found in project.", MessageType.Info);
        }

        EditorGUI.indentLevel--;
        serializedObject.ApplyModifiedProperties();
    }

    private void RefreshCommandList()
    {
        var container = target as MonoBehaviour;
        if (container == null) return;

        // Находим все компоненты на том же GameObject, которые являются CommandBehaviour<C>
        var allComponents = container.GetComponents<MonoBehaviour>();
        var validCommands = allComponents.Where(c => c != null && commandBaseType.IsAssignableFrom(c.GetType())).ToList();

        // Очищаем список и заполняем заново
        behavioursProp.ClearArray();
        behavioursProp.arraySize = validCommands.Count;
        for (int i = 0; i < validCommands.Count; i++)
        {
            behavioursProp.GetArrayElementAtIndex(i).objectReferenceValue = validCommands[i];
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"Refreshed: found {validCommands.Count} commands on {container.name}");
    }
}
#endif